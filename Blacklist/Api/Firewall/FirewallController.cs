using System.Runtime.InteropServices;
using Blacklist.Api.Firewall.Linux;
using Blacklist.Api.Firewall.Windows;
using Blacklist.Configuration;

namespace Blacklist.Api.Firewall
{
    public class FirewallController
    {
        public static string AddFirewallRule(ConnectionData connectionData)
        {
            var result = string.Empty;

            switch (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                case true:
                    result = WindowsFirewall.BlockIpConnection(connectionData);
                    break;

                case false:
                    result = LinuxFirewall.BlockIpConnection(connectionData);
                    break;
            }

            return result;
        }

        public static string RemoveFirewallRule(ConnectionData connectionData, PluginConfiguration config)
        {
            var result = string.Empty;

            switch (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                case true:
                    result = WindowsFirewall.AllowIpConnection(connectionData);
                    config.BannedConnections.Remove(connectionData);
                    Plugin.Instance.UpdateConfiguration(config);
                    break;
                case false:
                    result = LinuxFirewall.AllowIpConnection(connectionData);
                    config.BannedConnections.Remove(connectionData);
                    Plugin.Instance.UpdateConfiguration(config);
                    break;
            }

            return result;
        }

        public static bool FirewallConnectionRuleExists(ConnectionData connectionData)
        {
            var result = string.Empty;
            switch (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                case true:
                    result = WindowsCmd.GetCommandOutput("netsh.exe",
                        $" advfirewall firewall show rule name=\"{connectionData.RuleName}\"");
                    return result != "No rules match the specified criteria";
                case false:
                    result = LinuxBash.GetCommandOutput($"iptables -L INPUT -v -n | grep \"{connectionData.Ip}\"");
                    //We need a result example to condition this:
                    //if result is ?? then move on, else add the ip
                    break;
            }

            return false;
        }
    }
}