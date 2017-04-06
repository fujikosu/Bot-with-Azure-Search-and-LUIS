using System.Net.Http;
using System.Web.Configuration;
using System.Threading.Tasks;
using AzureSearchBot.Model;
using Newtonsoft.Json;
using System;

namespace CustomerReplyBot.Services
{
    [Serializable]
    public class AzureSearchService
    {
        private static readonly string QueryString = $"https://{WebConfigurationManager.AppSettings["SearchName"]}.search.windows.net/indexes/{WebConfigurationManager.AppSettings["IndexName"]}/docs?api-key={WebConfigurationManager.AppSettings["SearchKey"]}&api-version=2016-09-01&";

        public async Task<SearchResult> SearchByName(string name, int num)
        {
            using (var httpClient = new HttpClient())
            {
                string query = $"{QueryString}search={name}&$top={num}";
                string response = await httpClient.GetStringAsync(query);
                return JsonConvert.DeserializeObject<SearchResult>(response);
            }
        }
    }
}