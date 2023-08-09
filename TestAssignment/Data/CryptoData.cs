using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

using Windows.UI.Xaml.Shapes;


namespace TestAssignment.Data
{
    //Клас який використовується для зв'язку з API
    public class CryptoData
    {
        //Посилання на API
        private const string ApiBaseUrl = "https://api.coingecko.com/api/v3";

        /*Цей метод використвується для  десеріалізації відповіді від сайту  coingecko.com
         * на запрос який повинен повертати топ-10 криптовалют  */
        public static async Task<ObservableCollection<Currency>> GetCryptListAsync()
        {
            using (HttpClient client = new HttpClient())
            {
                client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/91.0.4472.124 Safari/537.36");
                //Рядок запросу на топ-10 криптовалют
                 string apiUrl = $"{ApiBaseUrl}/coins/markets?vs_currency=usd&ids=&order=market_cap_desc&per_page=10&page=1&sparkline=false&locale=en";
                
                HttpResponseMessage response = await client.GetAsync(apiUrl);
                //Якщо запрос успішний ми десералізуємо JSON формат у коллекцію об'єктів Crypt
                if (response.IsSuccessStatusCode)
                {
                    string jsonResponse = await response.Content.ReadAsStringAsync();
                    ObservableCollection<Currency> crypts = JsonConvert.DeserializeObject<ObservableCollection<Currency>>(jsonResponse);
                    foreach (Currency currency in crypts)
                    {
                        currency.Market_cap_string=currency.Market_cap.ToString("#,#", CultureInfo.InvariantCulture);
                        currency.Total_volume_string=currency.Total_volume.ToString("#,#", CultureInfo.InvariantCulture);
                    }
                    return crypts;
                }
                //Якщо запрос не був успішний ми виводимо код відповіді HTTP
                else
                {

                    return null;
                }
            }
        }

        /*Цей метод використвується для  конвертації валюти */
        public static async Task<decimal> ConvertCrypt(string currentyFromTransfer, string currentyWhichTransfer){
            using (HttpClient client = new HttpClient()){
                client.DefaultRequestHeaders.Add("User-agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/91.0.4472.124 Safari/537.36");
                //Запрос на виявлення ціни на обрану валюту за обрану валюту
                string apiurl = $"{ApiBaseUrl}/coins/markets?vs_currency={currentyWhichTransfer}&ids={currentyFromTransfer}";

                HttpResponseMessage response = await client.GetAsync(apiurl);
                if (response.IsSuccessStatusCode){
                    //Якщо запрос успішний ми десералізуємо JSON формат у коллекцію об'єктів Crypt
                    string JsonResponse = await response.Content.ReadAsStringAsync();
                    List<TestAssignment.Data.Currency> currencies = JsonConvert.DeserializeObject<List<TestAssignment.Data.Currency>>(JsonResponse);
                    // в цьому List є тільки один елемент
                    Currency result = currencies[0];
                    return result.Current_price;
                }
                else{
                    return 0;
                }
            }
        }
        public static async Task<PriceData> GetHistoryPriceForOneDay(string id)
        {
            using (HttpClient client = new HttpClient())
            {
                client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/91.0.4472.124 Safari/537.36");
                string apiUrl = $"{ApiBaseUrl}/coins/{id}/market_chart?vs_currency=usd&days=1";
                HttpResponseMessage response = await client.GetAsync(apiUrl);

                if (response.IsSuccessStatusCode)
                {
                    string jsonResponse = await response.Content.ReadAsStringAsync();
                    PriceData data = JsonConvert.DeserializeObject<PriceData>(jsonResponse);
                    foreach (List<object> price in data.Prices)
                    {
                        long unixTime = System.Convert.ToInt64(price[0]);
                        decimal currencyPrice = System.Convert.ToDecimal(price[1]);
                        DateTime dateTime = DateTimeOffset.FromUnixTimeMilliseconds(unixTime).DateTime;
                        
                    }
                    return data;
                }
                else
                {
                    return null;
                }
            }
        }
        //Цей метод використвується для отримання усіх криптовалют
        public static async Task<ObservableCollection<Currency>> GetAllCrypt()
        {
            using (HttpClient client = new HttpClient())
            {
                client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/91.0.4472.124 Safari/537.36");
                string apiurl = $"{ApiBaseUrl}/coins/markets?vs_currency=usd&ids=&";
                HttpResponseMessage response = await client.GetAsync(apiurl);
                if (response.IsSuccessStatusCode)
                {
                    string jsonResponse = await response.Content.ReadAsStringAsync();
                    ObservableCollection<Currency> crypts = JsonConvert.DeserializeObject<ObservableCollection<Currency>>(jsonResponse);
                    return crypts;
                }
                else
                {
                    return null;
                }
            }
        }
        public static async Task<decimal> GetPriceChange(string id)
        {
            using (HttpClient client = new HttpClient())
            {
                client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/91.0.4472.124 Safari/537.36");
                string apiUrl = $"{ApiBaseUrl}/coins/{id}?localization=false&tickers=true&market_data=true&community_data=false&developer_data=false&sparkline=false";
                HttpResponseMessage response = await client.GetAsync(apiUrl);
                if (response.IsSuccessStatusCode)
                {
                    string jsonResponse = await response.Content.ReadAsStringAsync();


                    dynamic data = JsonConvert.DeserializeObject(jsonResponse);

                    Console.WriteLine(data.id);
                    decimal priceChangePercentage24h = data.market_data.price_change_percentage_24h;
                    return priceChangePercentage24h;
                }
                else
                {
                    return -999;
                }
            }
        }
    }
}
