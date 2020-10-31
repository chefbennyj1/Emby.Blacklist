using System.Collections.Generic;
using MediaBrowser.Model.Plugins;

namespace Blacklist.Configuration
{
    public class PluginConfiguration : BasePluginConfiguration
    {
        public List<Connection> BannedConnections                              { get; set; }
        public int ConnectionAttemptsBeforeBan                                 { get; set; }
        public int BanDurationMinutes                                          { get; set; }
        public bool BanIndefinite                                              { get; set; }
        public int RuleNameCount                                               { get; set; }
        public bool EnableFirewallBlock                                        { get; set; }
        public bool EnableGeoIp                                                { get; set; }
        public string ipStackAccessToken                                       { get; set; }
        public bool IgnoreInternalFailedLoginAttempts                          { get; set; }
    }
}