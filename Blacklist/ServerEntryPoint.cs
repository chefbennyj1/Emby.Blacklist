using System;
using System.Collections.Generic;
using System.Linq;
using Blacklist.Api;
using Blacklist.Configuration;
using MediaBrowser.Controller.Configuration;
using MediaBrowser.Controller.Plugins;
using MediaBrowser.Controller.Session;
using MediaBrowser.Model.Events;
using MediaBrowser.Model.Logging;

namespace Blacklist
{
    public class ServerEntryPoint : IServerEntryPoint
    {
        private ISessionManager SessionManager                          { get; set; }
        private ILogger Logger                                          { get; set; }
        private ILogManager LogManager                                  { get; set; }
        private IServerConfigurationManager SystemConfiguration         { get; set; }
        private List<Connection> FailedConnectionAttemptLog             { get; set; }
        
        public ServerEntryPoint(ISessionManager man, ILogManager logManager, IServerConfigurationManager sys)
        {
            SessionManager              = man;
            LogManager                  = logManager;
            Logger                      = LogManager.GetLogger(Plugin.Instance.Name);
            FailedConnectionAttemptLog  = new List<Connection>();
            SystemConfiguration         = sys;
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

            SessionManager.AuthenticationFailed      += SessionManager_AuthenticationFailed;
            
        }
        
        private void SessionManager_AuthenticationFailed(object sender, GenericEventArgs<AuthenticationRequest> e)
        {
            var config         = Plugin.Instance.Configuration;
            var connectionList = CheckConnectionAttempt(e.Argument.RemoteEndPoint, config);

            foreach (var connection in connectionList)
            {
                if (!connection.IsBanned) continue;
                if (config.BannedConnections.Exists(c => c == connection)) continue;
                
                connection.BannedDateTime = DateTime.UtcNow;
                connection.IsBanned       = true;
                connection.RuleName       = "Emby_Authentication_Request_Blocked_" + config.RuleNameCount;
                connection.Id             = "Emby_Authentication_Request_Blocked_" + config.RuleNameCount;

                config.RuleNameCount += 1;
                config.BannedConnections.Add(connection);

                Plugin.Instance.UpdateConfiguration(config); 
                
                var result = FirewallController.AddFirewallRule(connection);

                Logger.Info($"Firewall Rule {connection.RuleName} added for Ip {connection.Ip} - {result}");

                //Remove the connection data from our ConnectionAttemptLog list because they are banned. We no longer have to track their attempts
                FailedConnectionAttemptLog.Remove(connection);
            }
        }

        private IEnumerable<Connection> CheckConnectionAttempt(string remoteEndPoint, PluginConfiguration config)
        {
            if (FailedConnectionAttemptLog.Exists(a => a.Ip == remoteEndPoint))
            {
                var connection = FailedConnectionAttemptLog.FirstOrDefault(c => c.Ip == remoteEndPoint);
                
                if (connection?.LoginAttempts < (config.ConnectionAttemptsBeforeBan != 0 ? config.ConnectionAttemptsBeforeBan : 3))
                {
                    connection.LoginAttempts += 1;
                    connection.FailAuthenticationRequestDateTimes.Add(DateTime.UtcNow);

                    return FailedConnectionAttemptLog;
                }

                if (connection?.FailAuthenticationRequestDateTimes.FirstOrDefault() > DateTime.UtcNow.AddSeconds(-30))
                {
                    connection.IsBanned = true;
                    return FailedConnectionAttemptLog;
                }

            }
            else
            {
                FailedConnectionAttemptLog.Add(new Connection()
                {
                    Ip = remoteEndPoint,
                    LoginAttempts = 1,
                    IsBanned = false,
                    FailAuthenticationRequestDateTimes = new List<DateTime> { DateTime.UtcNow },
                });
            }

            return FailedConnectionAttemptLog;
        }
        
    }
}
