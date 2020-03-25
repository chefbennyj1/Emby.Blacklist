using Blacklist.Configuration;

namespace Blacklist.Api.Windows
{
    public class WindowsFirewall
    {
        
        public static string BlockIpConnection(ConnectionData connectionData)
        {
            var netshArgs = $"advfirewall firewall add rule name=\"{connectionData.RuleName}\" dir=in interface=any action=block enable=yes profile=Any remoteip=\"{connectionData.Ip}\"";
            var result = WindowsCmd.GetCommandOutput("netsh.exe", netshArgs);
            return result; //OK
        }

        public static string AllowIpConnection(ConnectionData connectionData)
        {
            var netshArgs = $"advfirewall firewall delete rule name=\"{connectionData.RuleName}\" remoteip=\"{connectionData.Ip}\"";
            var result = WindowsCmd.GetCommandOutput("netsh.exe", netshArgs);
            return result; //OK
        }
    }
}
