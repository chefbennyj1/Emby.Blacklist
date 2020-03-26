using System.Diagnostics;

namespace Blacklist.Api.Firewall.Windows
{
    public class WindowsCmd
    {
        public static string GetCommandOutput(string file, string args)
        {
            ProcessStartInfo procStartInfo = new ProcessStartInfo(file, args)
            {
                RedirectStandardOutput = true,
                UseShellExecute        = false,
                CreateNoWindow         = true,
                Verb                   = "runas"
            };

            using (Process process = new Process())
            {
                process.StartInfo = procStartInfo;
                process.Start();

                string result = process.StandardOutput.ReadToEnd();
                return (result);
            }
        }
    }
}