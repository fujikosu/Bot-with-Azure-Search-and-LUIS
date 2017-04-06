using Newtonsoft.Json;

namespace AzureSearchBot.Model
{
    //Data model for search
    public class SearchResult
    {
        [JsonProperty("@odata.context")]
        public string odataContext { get; set; }
        public Value[] value { get; set; }
    }

    public class Value
    {
        [JsonProperty("@search.score")]
        public double searchScore { get; set; }

        public string id { get; set; }
        public string question { get; set; }
        public string answer { get; set; }
    }
}