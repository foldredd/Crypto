using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Media;

namespace TestAssignment.Converter
{
    internal class PriceChangePercentageColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is decimal priceChangePercentage)
            {
                if(priceChangePercentage > 0)
                {
                    return new SolidColorBrush(Windows.UI.Colors.Green);
                }
                else
                {
                    return new SolidColorBrush(Windows.UI.Colors.Red);
                }
            }
            return new SolidColorBrush(Windows.UI.Colors.White);
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
