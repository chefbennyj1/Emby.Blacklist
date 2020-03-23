using System;
using Blacklist.Configuration;

namespace Blacklist.Api.Linux
{
    public class LinuxFirewall
    {
        public static string BlockIpConnection(Connection connectionBan, PluginConfiguration config)
        {
            var ipTablesArgs = $"iptables -A INPUT -s {connectionBan.Ip} -j DROP";
            var result = LinuxBash.GetCommandOutput(ipTablesArgs);
            return "OK";
        }

        public static string AllowIpConnection(Connection connectionBan, PluginConfiguration config)
        {
            config.BannedConnections.Remove(connectionBan);
            Plugin.Instance.UpdateConfiguration(config);

            var ipTablesArgs = $"iptables -D INPUT -s {connectionBan.Ip} -j DROP";
            var result = LinuxBash.GetCommandOutput(ipTablesArgs);
            return "OK";

        }
    }
}
