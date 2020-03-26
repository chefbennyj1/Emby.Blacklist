using System.Collections.Generic;
using MediaBrowser.Model.Plugins;

namespace Blacklist.Configuration
{
    public class PluginConfiguration : BasePluginConfiguration
    {
        public List<ConnectionData> BannedConnections { get; set; }
        public int ConnectionAttemptsBeforeBan        { get; set; }
        public int BanDurationMinutes                 { get; set; }
        public bool BanIndefinite                     { get; set; }
        public int RuleNameCount                      { get; set; }
        public bool EnableReverseLookup               { get; set; }
        public string IpStackApiKey                   { get; set; }
    }
}