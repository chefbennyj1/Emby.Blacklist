namespace Blacklist.Api.Firewall.Windows
{
    public class WindowsFirewall
    {
        public static string BlockIpConnection(Connection connection)
        {
            var netshArgs =
                $"advfirewall firewall add rule name=\"{connection.RuleName}\" dir=in interface=any action=block enable=yes profile=Any remoteip=\"{connection.Ip}\"";
            var result = WindowsCmd.GetCommandOutput("netsh.exe", netshArgs);
            return result; //OK
        }

        public static string AllowIpConnection(Connection connection)
        {
            var netshArgs =
                $"advfirewall firewall delete rule name=\"{connection.RuleName}\" remoteip=\"{connection.Ip}\"";
            var result = WindowsCmd.GetCommandOutput("netsh.exe", netshArgs);
            return result; //OK
        }
    }
}