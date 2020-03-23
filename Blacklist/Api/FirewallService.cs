using System.Linq;
using MediaBrowser.Model.IO;
using MediaBrowser.Model.Logging;
using MediaBrowser.Model.Serialization;
using MediaBrowser.Model.Services;

namespace Blacklist.Api
{
    public class FirewallService : IService
    {



        [Route("/DeleteFirewallRule", "DELETE", Summary = "Delete firewall rule created by failed login attempts")]
        public class DeleteFirewallRule : IReturn<string>
        {
            [ApiMember(Name = "RuleName", Description = "Firewall Rule Name", IsRequired = true, DataType = "string", ParameterType = "query", Verb = "DELETE")]
            public string RuleName { get; set; }
            [ApiMember(Name = "Ip", Description = "Firewall Rule Ip", IsRequired = true, DataType = "string", ParameterType = "query", Verb = "DELETE")]
            public string Ip { get; set; }
        }

        private IJsonSerializer JsonSerializer { get; }
        private IFileSystem FileSystem         { get; }
        private readonly ILogger logger;

        public FirewallService(IJsonSerializer json, IFileSystem fS, ILogManager logManager)
        {
            JsonSerializer = json;
            FileSystem     = fS;
            logger         = logManager.GetLogger(GetType().Name);
        }

        public string Delete(DeleteFirewallRule request)
        {
            var config     = Plugin.Instance.Configuration;
            var connection = config.BannedConnections.FirstOrDefault(con => con.RuleName == request.RuleName);
            var result     = FirewallController.RemoveFirewallRule(connection, config);

            logger.Info($"Firewall Rule {connection?.RuleName} Deleted for Ip {connection?.Ip} - {result}");
            return result;
        }
    }
}
