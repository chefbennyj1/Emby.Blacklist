using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Blacklist.Api.Firewall;
using Blacklist.Api.ReverseLookup;
using Blacklist.Configuration;
using MediaBrowser.Common.Configuration;
using MediaBrowser.Common.Net;
using MediaBrowser.Controller.Plugins;
using MediaBrowser.Controller.Session;
using MediaBrowser.Model.Branding;
using MediaBrowser.Model.Events;
using MediaBrowser.Model.Logging;
using MediaBrowser.Model.Serialization;

namespace Blacklist
{
    public class ServerEntryPoint : IServerEntryPoint
    {
        private ISessionManager SessionManager                 { get; }
        private ILogger Logger                                 { get; }
        private ILogManager LogManager                         { get; }
        private List<ConnectionData> FailedAuthenticationAudit { get; }
        private IHttpClient HttpClient                         { get; }
        private IJsonSerializer JsonSerializer                 { get; }
        private IConfigurationManager ConfigurationManager     { get; }

        // ReSharper disable once TooManyDependencies
        public ServerEntryPoint(ISessionManager man, ILogManager logManager, IHttpClient client, IJsonSerializer json, IConfigurationManager configMan)
        {
            SessionManager            = man;
            LogManager                = logManager;
            Logger                    = LogManager.GetLogger(Plugin.Instance.Name);
            FailedAuthenticationAudit = new List<ConnectionData>();
            JsonSerializer            = json;
            HttpClient                = client;
            ConfigurationManager      = configMan;
            
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
            var remoteEndpoint = e.Argument.RemoteAddress;
            var deviceId       = e.Argument.DeviceId;
            var connectionList = CheckConnectionAttempt(remoteEndpoint.ToString(), deviceId, config);
           
            foreach (var connection in connectionList)
            {
                if (!connection.IsBanned) continue;
                if (config.BannedConnections.Exists(c => c == connection)) continue;

                connection.BannedDateTime = DateTime.UtcNow;
                connection.IsBanned       = true;
                connection.RuleName       = "Emby_Authentication_Request_Blocked_" + config.RuleNameCount;
                connection.Id             = "Emby_Authentication_Request_Blocked_" + config.RuleNameCount;
                connection.LookupData     = config.IpStackApiKey != null
                    ? ReverseLookupController.GetReverseLookupData(connection, HttpClient, JsonSerializer)
                    : null;

                config.RuleNameCount += 1;
                config.BannedConnections.Add(connection);

                Plugin.Instance.UpdateConfiguration(config);

                var result = FirewallController.AddFirewallRule(connection);

                Logger.Info($"Firewall Rule {connection.RuleName} added for Ip {connection.Ip} - {result}");

                //Remove the connection data from our ConnectionAttemptLog list because they are banned. We no longer have to track their attempts
                FailedAuthenticationAudit.Remove(connection);
                SessionManager.SendMessageToAdminSessions("FirewallAdded", connection, CancellationToken.None);
                
            }
        }

        private IEnumerable<ConnectionData> CheckConnectionAttempt(string remoteEndPoint, string deviceId, PluginConfiguration config)
        {
            if (FailedAuthenticationAudit.Exists(a => a.Ip == remoteEndPoint))
            {
                var connection = FailedAuthenticationAudit.FirstOrDefault(c => c.Ip == remoteEndPoint);

                var connectionAttemptThreshold = config.ConnectionAttemptsBeforeBan != 0 ? config.ConnectionAttemptsBeforeBan : 3;
                
                //If this connection has tried and failed, and is not Banned -  but has waited over thirty seconds to try again - reset the attempt count and clear FailedAuthDateTimes List.
                if (DateTime.UtcNow > connection?.FailedAuthDateTimes.LastOrDefault().AddSeconds(30))
                {
                    connection.FailedAuthDateTimes.Clear();
                    connection.LoginAttempts = 0;
                }
                
                //Log the attempt
                if (connection?.LoginAttempts < connectionAttemptThreshold)
                {
                    connection.LoginAttempts += 1;
                    connection.FailedAuthDateTimes.Add(DateTime.UtcNow);
                    connection.deviceId = deviceId;
                    //updateBrandingDisclaimer(connection, config);

                    return FailedAuthenticationAudit;
                }

                //Tried to many times in a row, and too quickly  -Ban the IP - could be a brute force attack.
                if (connection?.FailedAuthDateTimes.FirstOrDefault() > DateTime.UtcNow.AddSeconds(-30))
                {
                    connection.IsBanned = true;
                    return FailedAuthenticationAudit;
                }
            }
            else
            {
                FailedAuthenticationAudit.Add(new ConnectionData
                {
                    Ip                  = remoteEndPoint,
                    LoginAttempts       = 1,
                    IsBanned            = false,
                    FailedAuthDateTimes = new List<DateTime> {DateTime.UtcNow}
                });
            }

            return FailedAuthenticationAudit;
        }

        private void updateBrandingDisclaimer(ConnectionData connection, PluginConfiguration config)
        {
                var branding = ConfigurationManager.GetConfiguration<BrandingOptions>("branding");

                branding.LoginDisclaimer =
                    $"{(config.ConnectionAttemptsBeforeBan > 3 ? config.ConnectionAttemptsBeforeBan : 3) - connection.LoginAttempts} login attempt(s) attempts left.";
                ConfigurationManager.SaveConfiguration("branding", branding);

                SessionManager.SendMessageToUserDeviceAndAdminSessions(connection.deviceId, "UpdateDisclaimer", string.Empty, CancellationToken.None);
        }
    }
}