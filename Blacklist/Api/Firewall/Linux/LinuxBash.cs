using System.Diagnostics;

namespace Blacklist.Api.Firewall.Linux
{
    public class LinuxBash
    {
        public static string GetCommandOutput(string args)
        {
            var escapedArgs = args.Replace("\"", "\\\"");

            ProcessStartInfo procStartInfo = new ProcessStartInfo("/bin/bash", $"sudo -c \"{escapedArgs}\"")
            {
                RedirectStandardOutput = true,
                UseShellExecute        = false,
                CreateNoWindow         = true
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