using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestAssignment.Data
{
    public class Crypt
    {
        // Клас Crypt представляє криптовалюту і містить дані про неї.

        // Конструктор класу, який приймає різні параметри та ініціалізує властивості об'єкта Crypt.
        public Crypt(string id, int marketCapRank, string name, string symbol, decimal currentPrice, decimal totalVolume, decimal marketCap, string image,decimal price_change_percentage_24h,PriceData priceData)
        {
            this.Id = id;
            this.Symbol = symbol;
            this.Name = name;
            this.Current_price = currentPrice;
            this.Market_cap_rank = marketCapRank;
            this.Market_cap = marketCap;
            this.Total_volume = totalVolume;
            this.Image = image;
            this.price_change_percentage_24h = price_change_percentage_24h;
            Market_cap_string = marketCap.ToString("#,#", CultureInfo.InvariantCulture);
            Total_volume_string = Total_volume.ToString("#,#", CultureInfo.InvariantCulture);
            this.priceData = priceData;
        }
        public PriceData priceData {  get; set; }
        public string Market_cap_string { get; set; }
        public string Total_volume_string { get; set; }

        public decimal price_change_percentage_24h { get; set; }
        // Властивість Id представляє унікальний ідентифікатор криптовалюти.
        public string Id { get; set; }

        // Властивість Symbol представляє символьний код криптовалюти.
        public string Symbol { get; set; }

        // Властивість Name представляє назву криптовалюти.
        public string Name { get; set; }

        // Властивість Current_price представляє поточну ціну криптовалюти.
        public decimal Current_price { get; set; }

        // Властивість Market_cap_rank представляє рейтинг криптовалюти за розміром ринкової капіталізації.
        public int Market_cap_rank { get; set; }

        // Властивість Market_cap представляє ринкову капіталізацію криптовалюти.
        public decimal Market_cap { get; set; }

        // Властивість Total_volume представляє загальний обсяг торгів криптовалюти.
        public decimal Total_volume { get; set; }

        // Властивість Image представляє URL-адресу зображення криптовалюти.
        public string Image { get; set; }
    }
}
