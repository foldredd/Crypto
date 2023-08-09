using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestAssignment.Data
{
    // Клас, який представляє криптовалюту.
    public class Currency
    {
        // Конструктор класу, який приймає різні параметри та ініціалізує властивості об'єкта Currency.
        public Currency(string Id, string Symbol)
        {
            this.Id = Id;
            this.Symbol = Symbol;
        }

        public string Market_cap_string { get; set; }
        public string Total_volume_string { get; set; }
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
