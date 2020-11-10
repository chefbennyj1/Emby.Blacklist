using System.Threading.Tasks;
using MediaBrowser.Common.Net;
using MediaBrowser.Model.Logging;
using MediaBrowser.Model.Serialization;

namespace Blacklist.Api.ReverseLookup
{
    public class HackerTarget
    {
        private IHttpClient HttpClient         { get; }
        private ILogger Log                    { get; }
        private IJsonSerializer JsonSerializer { get; }

        public HackerTarget(IHttpClient client, ILogManager logMan, IJsonSerializer json)
        {
            HttpClient = client;
            Log = logMan.GetLogger(Plugin.Instance.Name);
            JsonSerializer = json;
        }
        public async Task<ReverseLookupData> GetLocation(string ip)
        {
            var locationData = await HttpClient.Get(new HttpRequestOptions()
            {
                Url = $"http://ip-api.com/json/{ip}"
            });

            var data = JsonSerializer.DeserializeFromStream<ReverseLookupData>(locationData);
            data.countryFlag = $"https://www.countryflags.io/{data.countryCode}/shiny/64.png";
            return data;
           
        }
    }
}
