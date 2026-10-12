using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace TestAssignment.Data
{
    public static class CryptoData
    {
        private const int MaxResponseCharacters = 2 * 1024 * 1024;
        private static readonly Uri ApiBaseUri = new Uri("https://api.coingecko.com/api/v3/");
        private static readonly Regex CoinIdPattern = new Regex("^[a-z0-9-]{1,100}$", RegexOptions.Compiled);
        private static readonly Regex CurrencyCodePattern = new Regex("^[a-z]{3,5}$", RegexOptions.Compiled);
        private static readonly JsonSerializerSettings JsonSettings = new JsonSerializerSettings
        {
            TypeNameHandling = TypeNameHandling.None,
            MaxDepth = 32,
            DateParseHandling = DateParseHandling.None,
            MissingMemberHandling = MissingMemberHandling.Ignore
        };
        private static readonly HttpClient Client = CreateClient();

        private static HttpClient CreateClient()
        {
            var client = new HttpClient
            {
                BaseAddress = ApiBaseUri,
                Timeout = TimeSpan.FromSeconds(15)
            };
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            client.DefaultRequestHeaders.UserAgent.ParseAdd("CryptoDashboard/1.0");
            return client;
        }

        public static async Task<ObservableCollection<Currency>> GetCryptListAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            var currencies = await GetJsonAsync<List<Currency>>(
                "coins/markets?vs_currency=usd&order=market_cap_desc&per_page=20&page=1&sparkline=false&price_change_percentage=24h",
                cancellationToken);
            return new ObservableCollection<Currency>(currencies ?? new List<Currency>());
        }

        public static async Task<ObservableCollection<Currency>> GetAllCryptAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            var currencies = await GetJsonAsync<List<Currency>>(
                "coins/markets?vs_currency=usd&order=market_cap_desc&per_page=250&page=1&sparkline=false&price_change_percentage=24h",
                cancellationToken);
            return new ObservableCollection<Currency>(currencies ?? new List<Currency>());
        }

        public static async Task<decimal> ConvertCryptAsync(string coinId, string targetCurrency, CancellationToken cancellationToken = default(CancellationToken))
        {
            ValidateCoinId(coinId);
            ValidateCurrencyCode(targetCurrency);
            string path = "simple/price?ids=" + Uri.EscapeDataString(coinId) +
                          "&vs_currencies=" + Uri.EscapeDataString(targetCurrency);
            var result = await GetJsonAsync<Dictionary<string, Dictionary<string, decimal>>>(path, cancellationToken);

            Dictionary<string, decimal> prices;
            decimal price;
            if (result == null || !result.TryGetValue(coinId, out prices) || !prices.TryGetValue(targetCurrency, out price))
            {
                throw new InvalidOperationException("Сервис не вернул курс для выбранной пары.");
            }
            return price;
        }

        public static Task<PriceData> GetHistoryPriceForOneDayAsync(string coinId, CancellationToken cancellationToken = default(CancellationToken))
        {
            ValidateCoinId(coinId);
            string path = "coins/" + Uri.EscapeDataString(coinId) + "/market_chart?vs_currency=usd&days=1&interval=hourly";
            return GetJsonAsync<PriceData>(path, cancellationToken);
        }

        public static async Task<decimal> GetPriceChangeAsync(string coinId, CancellationToken cancellationToken = default(CancellationToken))
        {
            ValidateCoinId(coinId);
            string path = "simple/price?ids=" + Uri.EscapeDataString(coinId) + "&vs_currencies=usd&include_24hr_change=true";
            var result = await GetJsonAsync<Dictionary<string, Dictionary<string, decimal?>>>(path, cancellationToken);

            Dictionary<string, decimal?> values;
            decimal? change;
            if (result != null && result.TryGetValue(coinId, out values) &&
                values.TryGetValue("usd_24h_change", out change) && change.HasValue)
            {
                return change.Value;
            }
            return 0m;
        }

        private static async Task<T> GetJsonAsync<T>(string relativePath, CancellationToken cancellationToken)
        {
            using (var request = new HttpRequestMessage(HttpMethod.Get, relativePath))
            using (HttpResponseMessage response = await Client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken))
            {
                if (!response.IsSuccessStatusCode)
                {
                    throw new HttpRequestException("CoinGecko вернул ошибку " + (int)response.StatusCode + ".");
                }
                if (response.Content.Headers.ContentType == null ||
                    !string.Equals(response.Content.Headers.ContentType.MediaType, "application/json", StringComparison.OrdinalIgnoreCase))
                {
                    throw new InvalidDataException("Сервис вернул ответ в неожиданном формате.");
                }
                if (response.Content.Headers.ContentLength.HasValue && response.Content.Headers.ContentLength.Value > MaxResponseCharacters)
                {
                    throw new InvalidDataException("Ответ сервиса превышает допустимый размер.");
                }

                string json = await ReadLimitedStringAsync(response.Content, cancellationToken);
                T value = JsonConvert.DeserializeObject<T>(json, JsonSettings);
                if (value == null)
                {
                    throw new InvalidDataException("Сервис вернул пустой ответ.");
                }
                return value;
            }
        }

        private static async Task<string> ReadLimitedStringAsync(HttpContent content, CancellationToken cancellationToken)
        {
            using (Stream stream = await content.ReadAsStreamAsync())
            using (var reader = new StreamReader(stream, Encoding.UTF8, true, 4096, false))
            {
                var builder = new StringBuilder();
                var buffer = new char[4096];
                int read;
                while ((read = await reader.ReadAsync(buffer, 0, buffer.Length)) > 0)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    if (builder.Length + read > MaxResponseCharacters)
                    {
                        throw new InvalidDataException("Ответ сервиса превышает допустимый размер.");
                    }
                    builder.Append(buffer, 0, read);
                }
                return builder.ToString();
            }
        }

        private static void ValidateCoinId(string value)
        {
            if (string.IsNullOrWhiteSpace(value) || !CoinIdPattern.IsMatch(value))
            {
                throw new ArgumentException("Некорректный идентификатор криптовалюты.", nameof(value));
            }
        }

        private static void ValidateCurrencyCode(string value)
        {
            if (string.IsNullOrWhiteSpace(value) || !CurrencyCodePattern.IsMatch(value))
            {
                throw new ArgumentException("Некорректный код валюты.", nameof(value));
            }
        }
    }
}
