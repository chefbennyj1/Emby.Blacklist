namespace Blacklist.Api.Firewall.Linux
{
    public class LinuxFirewall
    {
        public static string BlockIpConnection(Connection connection)
        {
            var ipTablesArgs = $"iptables -A INPUT -s {connection.Ip} -j DROP";
            var result = LinuxBash.GetCommandOutput(ipTablesArgs);
            return result.Contains("(policy DROP)") ? "OK" : string.Empty;
        }

        public static string AllowIpConnection(Connection connection)
        {
            var ipTablesArgs = $"iptables -D INPUT -s {connection.Ip} -j DROP";
            var result = LinuxBash.GetCommandOutput(ipTablesArgs);
            return result.Contains("(policy ACCEPT)") ? "OK" : string.Empty;
        }
    }
}