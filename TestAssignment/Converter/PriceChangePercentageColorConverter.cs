using System;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Media;

namespace TestAssignment.Converter
{
    public class PriceChangePercentageColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is decimal change)
            {
                return change >= 0m
                    ? new SolidColorBrush(Windows.UI.Color.FromArgb(255, 67, 211, 158))
                    : new SolidColorBrush(Windows.UI.Color.FromArgb(255, 255, 122, 144));
            }
            return new SolidColorBrush(Windows.UI.Color.FromArgb(255, 145, 160, 184));
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotSupportedException();
        }
    }
}
