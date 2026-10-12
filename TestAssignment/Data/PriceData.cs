using Newtonsoft.Json;
using System.Collections.Generic;

namespace TestAssignment.Data
{
    public class PriceData
    {
        [JsonProperty("prices")]
        public List<List<double>> Prices { get; set; } = new List<List<double>>();
    }
}
