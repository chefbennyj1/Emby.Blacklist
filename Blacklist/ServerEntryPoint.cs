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
        private List<Connection> FailedAuthenticationAudit     { get; }
        private IHttpClient HttpClient                         { get; }
        private IJsonSerializer JsonSerializer                 { get; }
        private IConfigurationManager ConfigurationManager     { get; }

        // ReSharper disable once TooManyDependencies
        public ServerEntryPoint(ISessionManager man, ILogManager logManager, IHttpClient client, IJsonSerializer json, IConfigurationManager configMan)
        {
            SessionManager            = man;
            LogManager                = logManager;
            Logger                    = LogManager.GetLogger(Plugin.Instance.Name);
            FailedAuthenticationAudit = new List<Connection>();
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
            var connection     = CheckConnectionAttempt(e.Argument, config);

            if (!connection.IsBanned) return;
            if (config.BannedConnections.Exists(c => c == connection)) return;
            
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

        private Connection CheckConnectionAttempt(AuthenticationRequest authenticationRequest, PluginConfiguration config)
        {
            Connection connection = null;

            if (FailedAuthenticationAudit.Exists(a => Equals(a.Ip, authenticationRequest.RemoteAddress.ToString())))
            {
                connection = FailedAuthenticationAudit.FirstOrDefault(c => Equals(c.Ip, authenticationRequest.RemoteAddress.ToString()));

                
                var connectionLoginAttemptThreshold = config.ConnectionAttemptsBeforeBan != 0 ? config.ConnectionAttemptsBeforeBan : 3;
                
                //If this connection has tried and failed, and is not Banned -  but has waited over thirty seconds to try again - reset the attempt count and clear FailedAuthDateTimes List.
                if (DateTime.UtcNow > connection?.FailedAuthDateTimes.LastOrDefault().AddSeconds(30))
                {
                    connection.FailedAuthDateTimes.Clear();
                    connection.LoginAttempts = 0;
                }
                
                //Log the attempt
                if (connection?.LoginAttempts < connectionLoginAttemptThreshold)
                {
                    connection.LoginAttempts += 1;
                    connection.FailedAuthDateTimes.Add(DateTime.UtcNow);
                    
                    return connection;
                }

                //Tried to many times in a row, and too quickly  -Ban the IP - could be a brute force attack.
                if (connection?.FailedAuthDateTimes.FirstOrDefault() > DateTime.UtcNow.AddSeconds(-30))
                {
                    connection.IsBanned = true;
                    return connection;
                }
            }
            else
            {
                connection = new Connection
                {
                    Ip                  = authenticationRequest.RemoteAddress.ToString(),
                    LoginAttempts       = 1,
                    IsBanned            = false,
                    FailedAuthDateTimes = new List<DateTime> {DateTime.UtcNow}
                };
                FailedAuthenticationAudit.Add(connection);
            }

            return connection;
        }

        
        private void updateBrandingDisclaimer(Connection connection, PluginConfiguration config, AuthenticationRequest authRequest)
        {
                var branding = ConfigurationManager.GetConfiguration<BrandingOptions>("branding");

                branding.LoginDisclaimer =
                    $"{(config.ConnectionAttemptsBeforeBan > 3 ? config.ConnectionAttemptsBeforeBan : 3) - connection.LoginAttempts} login attempt(s) attempts left.";
                ConfigurationManager.SaveConfiguration("branding", branding);

                SessionManager.SendMessageToUserDeviceAndAdminSessions(authRequest.DeviceId, "UpdateDisclaimer", string.Empty, CancellationToken.None);
        }
    }
}