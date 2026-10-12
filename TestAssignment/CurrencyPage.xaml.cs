using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using TestAssignment.Data;
using Windows.Foundation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

namespace TestAssignment
{
    public sealed partial class СurrencyPage : Page
    {
        private Crypt currency;

        public СurrencyPage()
        {
            InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            currency = e.Parameter as Crypt;
            if (currency == null)
            {
                Frame.Navigate(typeof(MainPage));
                return;
            }
            DataContext = currency;
            DrawChart();
        }

        private void DrawChart()
        {
            List<List<double>> prices = currency.priceData?.Prices?
                .Where(point => point != null && point.Count >= 2 && !double.IsNaN(point[1]) && !double.IsInfinity(point[1]))
                .ToList() ?? new List<List<double>>();
            if (prices.Count < 2) return;

            double min = prices.Min(point => point[1]);
            double max = prices.Max(point => point[1]);
            double range = Math.Max(max - min, Math.Max(max * 0.01, 0.00000001));
            var points = new PointCollection();
            for (int index = 0; index < prices.Count; index++)
            {
                double x = 680d * index / (prices.Count - 1);
                double y = 245d - ((prices[index][1] - min) / range * 230d);
                points.Add(new Point(x, y));
            }
            PriceLine.Points = points;

            DateTime start = DateTimeOffset.FromUnixTimeMilliseconds((long)prices.First()[0]).LocalDateTime;
            DateTime end = DateTimeOffset.FromUnixTimeMilliseconds((long)prices.Last()[0]).LocalDateTime;
            StartTimeText.Text = start.ToString("dd MMM, HH:mm", CultureInfo.CurrentCulture);
            EndTimeText.Text = end.ToString("dd MMM, HH:mm", CultureInfo.CurrentCulture);
        }

        private void GoHome(object sender, RoutedEventArgs e) => Frame.Navigate(typeof(MainPage));
        private void GoConvert(object sender, RoutedEventArgs e) => Frame.Navigate(typeof(Convert));
        private void GoSearch(object sender, RoutedEventArgs e) => Frame.Navigate(typeof(SearchPage));
    }
}
