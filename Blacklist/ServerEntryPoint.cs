using System;
using System.Collections.Generic;
using System.Linq;
using Blacklist.Api;
using Blacklist.Api.ReverseLookup;
using Blacklist.Configuration;
using MediaBrowser.Common.Net;
using MediaBrowser.Controller.Plugins;
using MediaBrowser.Controller.Session;
using MediaBrowser.Model.Events;
using MediaBrowser.Model.Logging;
using MediaBrowser.Model.Serialization;

namespace Blacklist
{
    public class ServerEntryPoint : IServerEntryPoint
    {
        private ISessionManager SessionManager                            { get; set; }
        private ILogger Logger                                            { get; set; }
        private ILogManager LogManager                                    { get; set; }
        private List<ConnectionData> FailedAuthAttemptLog                     { get; set; }
        private IHttpClient HttpClient                                    { get; set; }
        private IJsonSerializer JsonSerializer                            { get; set; }

        public ServerEntryPoint(ISessionManager man, ILogManager logManager, IHttpClient client, IJsonSerializer json) //, IServerConfigurationManager sys)
        {
            SessionManager              = man;
            LogManager                  = logManager;
            Logger                      = LogManager.GetLogger(Plugin.Instance.Name);
            FailedAuthAttemptLog        = new List<ConnectionData>();
            JsonSerializer              = json;
            HttpClient                  = client;
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }

        // ReSharper disable once MethodNameNotMeaningful
        public void Run()
        {
            Plugin.Instance.UpdateConfiguration(Plugin.Instance.Configuration);

            var config = Plugin.Instance.Configuration;
            foreach (var connection in config.BannedConnections)
            {
                if (!FirewallController.FirewallConnectionRuleExists(connection))
                {
                    FirewallController.AddFirewallRule(connection);
                }
            }

            SessionManager.AuthenticationFailed += SessionManager_AuthenticationFailed;
        }
        
        private void SessionManager_AuthenticationFailed(object sender, GenericEventArgs<AuthenticationRequest> e)
        {
            var config         = Plugin.Instance.Configuration;
            var connectionList = CheckConnectionAttempt(e.Argument.RemoteAddress.ToString(), config);

            foreach (var connection in connectionList)
            {
                if (!connection.IsBanned) continue;
                if (config.BannedConnections.Exists(c => c == connection)) continue;
                
                connection.BannedDateTime = DateTime.UtcNow;
                connection.IsBanned       = true;
                connection.RuleName       = "Emby_Authentication_Request_Blocked_" + config.RuleNameCount;
                connection.Id             = "Emby_Authentication_Request_Blocked_" + config.RuleNameCount;
                connection.LookupData     = config.IpStackApiKey != null ? ReverseLookupController.GetReverseLookupData(connection, HttpClient, JsonSerializer) : null;

                config.RuleNameCount += 1;
                config.BannedConnections.Add(connection);
                
                Plugin.Instance.UpdateConfiguration(config); 
                
                var result = FirewallController.AddFirewallRule(connection);

                Logger.Info($"Firewall Rule {connection.RuleName} added for Ip {connection.Ip} - {result}");

                //Remove the connection data from our ConnectionAttemptLog list because they are banned. We no longer have to track their attempts
                FailedAuthAttemptLog.Remove(connection);
            }
        }

        private IEnumerable<ConnectionData> CheckConnectionAttempt(string remoteEndPoint, PluginConfiguration config)
        {
            if (FailedAuthAttemptLog.Exists(a => a.Ip == remoteEndPoint))
            {
                var connection = FailedAuthAttemptLog.FirstOrDefault(c => c.Ip == remoteEndPoint);
                
                if (connection?.LoginAttempts < (config.ConnectionAttemptsBeforeBan != 0 ? config.ConnectionAttemptsBeforeBan : 3))
                {
                    connection.LoginAttempts += 1;
                    connection.FailedAuthDateTimes.Add(DateTime.UtcNow);

                    return FailedAuthAttemptLog;
                }

                if (connection?.FailedAuthDateTimes.FirstOrDefault() > DateTime.UtcNow.AddSeconds(-30))
                {
                    connection.IsBanned = true;
                    return FailedAuthAttemptLog;
                }
            }
            else
            {
                FailedAuthAttemptLog.Add(new ConnectionData()
                {
                    Ip                                 = remoteEndPoint,
                    LoginAttempts                      = 1,
                    IsBanned                           = false,
                    FailedAuthDateTimes                = new List<DateTime> { DateTime.UtcNow },
                });
            }

            return FailedAuthAttemptLog;
        }
        
    }
}
