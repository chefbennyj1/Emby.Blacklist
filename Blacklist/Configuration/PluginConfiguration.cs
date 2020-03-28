using System.Collections.Generic;
using MediaBrowser.Model.Dto;
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
        public bool EnableReverseLookup                                        { get; set; }
        public string IpStackApiKey                                            { get; set; }
        public List<AuthorizedUserRemoteAddress> AuthorizedUserRemoteAddresses { get; set; }
    }

    public class AuthorizedUserRemoteAddress
    {
        public string UserName      { get; set; }
        public string UserId        { get; set; }
        public string DeviceName    { get; set; }
        public string RemoteAddress { get; set; }
    }
}