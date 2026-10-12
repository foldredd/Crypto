using Newtonsoft.Json;
using System.Globalization;

namespace TestAssignment.Data
{
    public class Currency
    {
        public Currency() { }

        public Currency(string id, string symbol, string name)
        {
            Id = id;
            Symbol = symbol;
            Name = name;
        }

        [JsonProperty("id")]
        public string Id { get; set; }
        [JsonProperty("symbol")]
        public string Symbol { get; set; }
        [JsonProperty("name")]
        public string Name { get; set; }
        [JsonProperty("current_price")]
        public decimal Current_price { get; set; }
        [JsonProperty("market_cap_rank")]
        public int Market_cap_rank { get; set; }
        [JsonProperty("market_cap")]
        public decimal Market_cap { get; set; }
        [JsonProperty("total_volume")]
        public decimal Total_volume { get; set; }
        [JsonProperty("image")]
        public string Image { get; set; }
        [JsonProperty("price_change_percentage_24h")]
        public decimal? PriceChangePercentage24h { get; set; }

        [JsonIgnore]
        public string SymbolDisplay => (Symbol ?? string.Empty).ToUpperInvariant();
        [JsonIgnore]
        public string PriceDisplay => Current_price < 1m
            ? Current_price.ToString("$0.########", CultureInfo.InvariantCulture)
            : Current_price.ToString("$#,##0.00", CultureInfo.InvariantCulture);
        [JsonIgnore]
        public string MarketCapDisplay => FormatCompact(Market_cap);
        [JsonIgnore]
        public string VolumeDisplay => FormatCompact(Total_volume);
        [JsonIgnore]
        public string ChangeDisplay => PriceChangePercentage24h.HasValue
            ? PriceChangePercentage24h.Value.ToString("+0.00;-0.00;0.00", CultureInfo.InvariantCulture) + "%"
            : "—";

        private static string FormatCompact(decimal value)
        {
            if (value >= 1000000000000m) return (value / 1000000000000m).ToString("0.00", CultureInfo.InvariantCulture) + " трлн";
            if (value >= 1000000000m) return (value / 1000000000m).ToString("0.00", CultureInfo.InvariantCulture) + " млрд";
            if (value >= 1000000m) return (value / 1000000m).ToString("0.00", CultureInfo.InvariantCulture) + " млн";
            return value.ToString("#,##0", CultureInfo.InvariantCulture);
        }
    }
}
