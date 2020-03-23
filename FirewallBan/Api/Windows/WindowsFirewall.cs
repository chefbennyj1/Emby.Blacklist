using System;
using Blacklist.Configuration;

namespace Blacklist.Api.Windows
{
    public class WindowsFirewall
    {
        
        public static string BlockIpConnection(Connection connectionBan)
        {
            var netshArgs = $"advfirewall firewall add rule name=\"{connectionBan.RuleName}\" dir=in interface=any action=block enable=yes profile=Any remoteip=\"{connectionBan.Ip}\"";
            var result = WindowsCmd.GetCommandOutput("netsh.exe", netshArgs);
            return result;
        }

        public static string AllowIpConnection(Connection connectionBan, PluginConfiguration config)
        {
            config.BannedConnections.Remove(connectionBan);
            Plugin.Instance.UpdateConfiguration(config);

            var netshArgs = $"advfirewall firewall delete rule name=\"{connectionBan.RuleName}\" remoteip=\"{connectionBan.Ip}\"";
            var result = WindowsCmd.GetCommandOutput("netsh.exe", netshArgs);
            return result;
        }
    }
}
