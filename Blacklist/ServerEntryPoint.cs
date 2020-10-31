using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Blacklist.Api.Firewall;
using Blacklist.Api.ReverseLookup;
using Blacklist.Configuration;
using MediaBrowser.Common.Configuration;
using MediaBrowser.Common.Net;
using MediaBrowser.Controller.Plugins;
using MediaBrowser.Controller.Session;
using MediaBrowser.Model.Activity;
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
        private IConfigurationManager ConfigurationManager     { get; }
        private IActivityManager ActivityManager               { get; }
        private HackerTarget Target                            { get; }

        // ReSharper disable once TooManyDependencies
        public ServerEntryPoint(ISessionManager man, ILogManager logManager, IHttpClient client, IJsonSerializer json, IConfigurationManager configMan, IActivityManager activityManager)
        {
            SessionManager            = man;
            LogManager                = logManager;
            Logger                    = LogManager.GetLogger(Plugin.Instance.Name);
            FailedAuthenticationAudit = new List<Connection>();
            ActivityManager           = activityManager;
            ConfigurationManager      = configMan;
            Target                    = new HackerTarget(client, logManager, json);
        }

        public void Dispose()
        {
            
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

            var config = Plugin.Instance.Configuration;

            if (!config.EnableFirewallBlock) return;

            if (IsLocalNetworkIp(e.Argument.RemoteAddress) && config.IgnoreInternalFailedLoginAttempts) return;

            var connection = CheckConnectionAttempt(e.Argument, config).Result;

            if (!connection.IsBanned) return;
            if (config.BannedConnections.Exists(c => c == connection)) return;
            
            connection.BannedDateTime = DateTime.Now;
            connection.IsBanned       = true;
            connection.RuleName       = "Emby_Authentication_Request_Blocked_" + config.RuleNameCount;
            connection.Id             = "Emby_Authentication_Request_Blocked_" + config.RuleNameCount;
            
            config.RuleNameCount += 1;

            config.BannedConnections.Add(connection);

            Plugin.Instance.UpdateConfiguration(config);

            var result = FirewallController.AddFirewallRule(connection);

            Logger.Info($"Firewall Rule {connection.RuleName} added for Ip {connection.Ip} - {result}");

            ActivityManager.Create(new ActivityLogEntry()
            {
                Date          = connection.BannedDateTime,
                Id            = Convert.ToInt64(000 + config.RuleNameCount),
                Name          = "Firewall Blocked Ip", 
                Severity      = LogSeverity.Warn,
                Overview      = $"{connection.Ip} blocked: too many failed login attempts on {connection.UserAccountName}'s account, from ISP: {connection.Isp}, on device {connection.DeviceName}",
                ShortOverview = $"{connection.Ip}: too many failed login attempts.",
                Type          = "Alert"
            });

            //Remove the connection data from our ConnectionAttemptLog list because they are banned. We no longer have to track their attempts
            FailedAuthenticationAudit.Remove(connection);
            SessionManager.SendMessageToAdminSessions("FirewallAdded", connection, CancellationToken.None);
        }

        private static bool IsLocalNetworkIp(IPAddress ip)
        {
            return ip.ToString().Substring(0, 3).Equals("192");
        }

        private async Task<Connection> CheckConnectionAttempt(AuthenticationRequest authenticationRequest, PluginConfiguration config)
        {
            Connection connection = null;

            if (FailedAuthenticationAudit.Exists(a => Equals(a.Ip, authenticationRequest.RemoteAddress.ToString())))
            {
                connection = FailedAuthenticationAudit.FirstOrDefault(c => Equals(c.Ip, authenticationRequest.RemoteAddress.ToString()));
                
                var connectionLoginAttemptThreshold = config.ConnectionAttemptsBeforeBan != 0 ? config.ConnectionAttemptsBeforeBan : 3;
                
                //If this connection has tried and failed, and is not Banned - but has waited over thirty seconds to try again - reset the attempt count and clear FailedAuthDateTimes List.
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
                ReverseLookupData targetData = null;
                
                if (Plugin.Instance.Configuration.EnableGeoIp && !IsLocalNetworkIp(authenticationRequest.RemoteAddress))
                {
                    targetData = await Target.GetLocation(authenticationRequest.RemoteAddress.ToString());
                }

                // ReSharper disable once ComplexConditionExpression
                connection = new Connection
                {
                    FlagIconUrl         = targetData is null ? string.Empty : targetData.countryFlag,
                    Isp                 = targetData is null ? string.Empty : targetData.isp,
                    Ip                  = authenticationRequest.RemoteAddress.ToString(),
                    DeviceName          = authenticationRequest.DeviceName,
                    UserAccountName     = authenticationRequest.Username,
                    Proxy               = targetData?.proxy ?? false,
                    ServiceProvider     = targetData is null ? string.Empty : targetData.isp,
                    Longitude           = targetData?.lon ?? 0,
                    Latitude            = targetData?.lat ?? 0,
                    LoginAttempts       = 1,
                    IsBanned            = false,
                    Region              = targetData?.regionName ?? string.Empty,
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
            
                //SessionManager.SendMessageToUserDeviceAndAdminSessions(authRequest.DeviceId, "UpdateDisclaimer", string.Empty, CancellationToken.None);
        }
    }
}