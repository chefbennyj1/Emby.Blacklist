using System.Threading.Tasks;
using MediaBrowser.Common.Net;
using MediaBrowser.Model.Logging;
using MediaBrowser.Model.Serialization;

namespace Blacklist.Api.ReverseLookup
{
    public class HackerTarget
    {
        private IHttpClient HttpClient         { get; set; }
        private ILogger Log                    { get; set; }
        private IJsonSerializer JsonSerializer { get; set; }

        public HackerTarget(IHttpClient client, ILogManager logMan, IJsonSerializer json)
        {
            HttpClient = client;
            Log = logMan.GetLogger(Plugin.Instance.Name);
            JsonSerializer = json;
        }
        public async Task<ReverseLookupData> GetLocation(string ip)
        {
            
            var config = Plugin.Instance.Configuration;
            if (!(config.ipStackAccessToken is null))
            {
                var locationData = await HttpClient.Get(new HttpRequestOptions()
                {
                    Url = $"http://api.ipstack.com/{ip}?access_key={config.ipStackAccessToken} "
                });

                return JsonSerializer.DeserializeFromStream<ReverseLookupData>(locationData);
            }

            return null;
        }
    }
}
