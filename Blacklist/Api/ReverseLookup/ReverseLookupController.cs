using MediaBrowser.Common.Net;
using MediaBrowser.Model.Serialization;

namespace Blacklist.Api.ReverseLookup
{
    public class ReverseLookupController
    {
        public static ReverseLookupData GetReverseLookupData(ConnectionData connectionData, IHttpClient httpClient, IJsonSerializer jsonSerializer)
        {
            var config = Plugin.Instance.Configuration;
            var json = httpClient.Get(new HttpRequestOptions()
            {
                AcceptHeader = "application/json",
                Url = $"http://api.ipstack.com/{connectionData.Ip}?access_key={config.IpStackApiKey}"
            }).Result;
            return jsonSerializer.DeserializeFromStream<ReverseLookupData>(json);
        }
    }
}
