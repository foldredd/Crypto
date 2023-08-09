using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using TestAssignment.Data;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using WinRTXamlToolkit.Controls.DataVisualization.Charting;



namespace TestAssignment
{
   
    public sealed partial class СurrencyPage : Page
    {
        Crypt currency { get; set; }

        /*public static readonly DependencyProperty MyCryptsProperty =
            DependencyProperty.Register("Crypt", typeof(Crypt), typeof(СurrencyPage), new PropertyMetadata(null));*/
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            // Вилучення переданого об'єкта Currency з параметрів навігації
            currency = e.Parameter as Crypt;
            this.DataContext = currency;
        }

        public СurrencyPage()
        {
            this.InitializeComponent();
            
        }
        private void button_Click(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(MainPage));
        }

        private void go_Convert(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(Convert));
        }
        private void go_Search(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(SearchPage));
        }
        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            // ...

            List<List<object>> last15Prices = currency.priceData.Prices.Skip(Math.Max(0, currency.priceData.Prices.Count - 15)).ToList();

           // PriceChangeList priceChangeList = new PriceChangeList();
           //priceChangeList.unitList = new List<PriceChangeUnit>();
            List<PriceChangeUnit> priceChangeList = new List<PriceChangeUnit>();
            foreach (var price in last15Prices)
            {
                long unixTime = System.Convert.ToInt64(price[0]);
                DateTime dateTime = DateTimeOffset.FromUnixTimeMilliseconds(unixTime).DateTime;
                PriceChangeUnit priceChangeUnit = new PriceChangeUnit();
                priceChangeUnit.date = dateTime;
                priceChangeUnit.price = (double)price[1];

                priceChangeList.Add(priceChangeUnit);
            }

            LineSeries lineSeries = new LineSeries();
            lineSeries.ItemsSource = priceChangeList;
            lineSeries.IndependentValuePath = "date";
            lineSeries.DependentValuePath = "price";
            lineSeries.Title = "price chart for the last 3 hours";
            chart.Series.Add(lineSeries);


        }





    }
}
