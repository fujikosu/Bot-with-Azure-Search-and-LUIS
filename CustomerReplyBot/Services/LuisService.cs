using System.Net.Http;
using System.Web.Configuration;
using System.Threading.Tasks;
using Luis.Model;
using Newtonsoft.Json;
using System;

namespace CustomerReplyBot.Services
{
    [Serializable]
    public class LuisService
    {
        private static readonly string QueryString = $"https://westus.api.cognitive.microsoft.com/luis/v2.0/apps/your_luis_number?subscription-key={WebConfigurationManager.AppSettings["LuisKey"]}&verbose=true&q=";

        public async Task<LuisResult> GetResponse(string sentence)
        {
            using (var httpClient = new HttpClient())
            {
                string query = $"{QueryString}{sentence}";
                string response = await httpClient.GetStringAsync(query);
                return JsonConvert.DeserializeObject<LuisResult>(response);
            }
        }
    }
}