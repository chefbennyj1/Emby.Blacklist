using System.Runtime.InteropServices;
using Blacklist.Api.Firewall.Linux;
using Blacklist.Api.Firewall.Windows;
using Blacklist.Configuration;

namespace Blacklist.Api.Firewall
{
    public class FirewallController
    {
        public static string AddFirewallRule(Connection connection)
        {
            var result = string.Empty;

            switch (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                case true:
                    result = WindowsFirewall.BlockIpConnection(connection);
                    break;

                case false:
                    result = LinuxFirewall.BlockIpConnection(connection);
                    break;
            }

            return result;
        }

        public static string RemoveFirewallRule(Connection connection, PluginConfiguration config)
        {
            var result = string.Empty;

            switch (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                case true:
                    result = WindowsFirewall.AllowIpConnection(connection);
                    config.BannedConnections.Remove(connection);
                    Plugin.Instance.UpdateConfiguration(config);
                    break;
                case false:
                    result = LinuxFirewall.AllowIpConnection(connection);
                    config.BannedConnections.Remove(connection);
                    Plugin.Instance.UpdateConfiguration(config);
                    break;
            }

            return result;
        }

        public static bool FirewallConnectionRuleExists(Connection connection)
        {
            var result = string.Empty;
            switch (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                case true:
                    result = WindowsCmd.GetCommandOutput("netsh.exe",
                        $" advfirewall firewall show rule name=\"{connection.RuleName}\"");
                    return result != "No rules match the specified criteria";
                case false:
                    result = LinuxBash.GetCommandOutput($"iptables -L INPUT -v -n | grep \"{connection.Ip}\"");
                    //We need a result example to condition this:
                    //if result is ?? then move on, else add the ip
                    break;
            }

            return false;
        }
    }
}