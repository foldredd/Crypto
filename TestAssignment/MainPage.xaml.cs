using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using TestAssignment.Data;
using Windows.Devices.Enumeration;
using System.Security.Cryptography.X509Certificates;

namespace TestAssignment
{
    public sealed partial class MainPage : Page
    {
        private ObservableCollection<Currency> crypts;

        // Зарегистрированная зависимость
        public static readonly DependencyProperty MyCryptsProperty =
            DependencyProperty.Register("CryptodataListProperty", typeof(ObservableCollection<Currency>), typeof(MainPage), new PropertyMetadata(null));
        
      

        // Коллекция объектов Crypt, которая будет зарегистрирована в зависимости
        public ObservableCollection<Currency> CryptodataListProperty
        {
            get { return (ObservableCollection<Currency>)GetValue(MyCryptsProperty); }
            set { SetValue(MyCryptsProperty, value); }
        }

        public MainPage()
        {
            this.InitializeComponent();
            this.TopCrypt();
            this.DataContext = this;
        }

        // За допомогою API визначаємо топ-10 криптовалют
        public async Task AsyncCryptList()
        {
            await Task.Run(async () => {
               
                crypts = new ObservableCollection<Currency>(await CryptoData.GetCryptListAsync());

                await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                {
                    SetValue(MyCryptsProperty, crypts);
                });
            });
        }
        private async Task TopCrypt()
        {
            await AsyncCryptList();
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
        private async void  ListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var listView = sender as ListView;
            var selectedItem = listView.SelectedItem as Currency;
            if (selectedItem != null)
            {
                var Id=selectedItem.Id;
                var marketCapRank = selectedItem.Market_cap_rank;
                var name = selectedItem.Name;
                var symbol = selectedItem.Symbol;
                var currentPrice = selectedItem.Current_price;
                var totalVolume = selectedItem.Total_volume;
                var marketCap = selectedItem.Market_cap;
                var image = selectedItem.Image;
                decimal priceChange =  await CryptoData.GetPriceChange(Id);
              
                PriceData priceData = await CryptoData.GetHistoryPriceForOneDay(Id);
                Crypt crypt = new Crypt(Id, marketCapRank, name, symbol, currentPrice, totalVolume, marketCap, image,priceChange,priceData);
               
             
                Frame.Navigate(typeof(СurrencyPage), crypt);
            }
        }



    }
}
