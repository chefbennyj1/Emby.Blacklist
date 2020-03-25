using System.Linq;
using MediaBrowser.Common.Net;
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

        [Route("/ReverseLookup", "GET", Summary = "Get Geo-location data about the failed login attempt")]
        public class ReverseLookup : IReturn<string>
        {
            [ApiMember(Name = "Ip", Description = "Ip Address to lookup", IsRequired = true, DataType = "string", ParameterType = "query", Verb = "GET")]
            public string Ip { get; set; }
        }

        private IJsonSerializer JsonSerializer { get; }
        private IFileSystem FileSystem         { get; }
        private readonly ILogger logger;
        private IHttpClient HttpClient         { get; set; }

        // ReSharper disable once TooManyDependencies
        public FirewallService(IJsonSerializer json, IFileSystem fS, ILogManager logManager, IHttpClient client)
        {
            JsonSerializer = json;
            FileSystem     = fS;
            logger         = logManager.GetLogger(GetType().Name);
            HttpClient     = client;
        }

        public string Delete(DeleteFirewallRule request)
        {
            var config     = Plugin.Instance.Configuration;
            var connection = config.BannedConnections.FirstOrDefault(con => con.RuleName == request.RuleName);
            var result     = FirewallController.RemoveFirewallRule(connection, config);

            logger.Info($"Firewall Rule {connection?.RuleName} Deleted for Ip {connection?.Ip} - {result}");
            return result;
        }

        public string Get(ReverseLookup request)
        {
            var config = Plugin.Instance.Configuration;
            var json = HttpClient.Get(new HttpRequestOptions()
            {
                AcceptHeader = "application/json",
                Url = $"http://api.ipstack.com/{request.Ip}?access_key={config.IpStackApiKey}"
            });
            return JsonSerializer.SerializeToString(json);
        }
    }
}
