namespace Blacklist.Api.Firewall.Linux
{
    public class LinuxFirewall
    {
        public static string BlockIpConnection(ConnectionData connectionData)
        {
            var ipTablesArgs = $"iptables -A INPUT -s {connectionData.Ip} -j DROP";
            var result = LinuxBash.GetCommandOutput(ipTablesArgs);
            return result.Contains("(policy DROP)") ? "OK" : string.Empty;
        }

        public static string AllowIpConnection(ConnectionData connectionData)
        {
            

            var ipTablesArgs = $"iptables -D INPUT -s {connectionData.Ip} -j DROP";
            var result = LinuxBash.GetCommandOutput(ipTablesArgs);
            return result.Contains("(policy ACCEPT)") ? "OK" : string.Empty;
        }
    }
}
