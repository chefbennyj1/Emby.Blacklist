using System.Runtime.InteropServices;
using Blacklist.Api.Linux;
using Blacklist.Api.Windows;
using Blacklist.Configuration;

namespace Blacklist.Api
{
    public class FirewallController
    {
        public static string AddFirewallRule(Connection connection, PluginConfiguration config)
        {
            var result = string.Empty;

            switch (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                case true:
                    result = WindowsFirewall.BlockIpConnection(connection);
                    break;

                case false:
                    result = LinuxFirewall.BlockIpConnection(connection, config);
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
                    result = WindowsFirewall.AllowIpConnection(connection, config);
                    break;
                case false:
                    result = LinuxFirewall.AllowIpConnection(connection, config);
                    break;
            }

            return result;
        }
    }
}
