using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
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

// Документацию по шаблону элемента "Пустая страница" см. по адресу https://go.microsoft.com/fwlink/?LinkId=234238

namespace TestAssignment
{
    /// <summary>
    /// Пустая страница, которую можно использовать саму по себе или для перехода внутри фрейма.
    /// </summary>
    public sealed partial class SearchPage : Page
    {
        private ObservableCollection<Currency> crypts { get; set; }
        public ObservableCollection<Currency> CryptodataListProperty
        {
            get { return (ObservableCollection<Currency>)GetValue(MyCryptsProperty); }
            set { SetValue(MyCryptsProperty, value); }
        }

        public static readonly DependencyProperty MyCryptsProperty =
           DependencyProperty.Register("CryptodataListProperty", typeof(ObservableCollection<Currency>), typeof(SearchPage), new PropertyMetadata(null));
        public SearchPage(){
            this.InitializeComponent();
            this.AllCrypt();
            this.DataContext = this;
        }
        private async Task AllCrypt()
        {
            await AsyncCryptList();
        }
        public async Task AsyncCryptList()
        {
            await Task.Run(async () => {
                crypts = new ObservableCollection<Currency>(await CryptoData.GetAllCrypt());

                await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                {
                    SetValue(MyCryptsProperty, crypts);
                });
            });
        }
        private void button_Click(object sender, RoutedEventArgs e){
            Frame.Navigate(typeof(MainPage));
        }
       
        private void go_Convert(object sender, RoutedEventArgs e){
            Frame.Navigate(typeof(Convert));
        }
        private void SearchTextBox_TextChanged(object sender, TextChangedEventArgs e){
            string searchText = SearchTextBox.Text.Trim().ToLower();

            if (!string.IsNullOrEmpty(searchText))
            {
                
                var filteredList = crypts.Where(currency => currency.Name.ToLower().Contains(searchText)).ToList();

               
                CryptodataListProperty = new ObservableCollection<Currency>(filteredList);
            }
            else
            {
                
                CryptodataListProperty = new ObservableCollection<Currency>(crypts);
            }
        }
        private async void ListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var listView = sender as ListView;
            var selectedItem = listView.SelectedItem as Currency;
            if (selectedItem != null)
            {
                var Id = selectedItem.Id;
                var marketCapRank = selectedItem.Market_cap_rank;
                var name = selectedItem.Name;
                var symbol = selectedItem.Symbol;
                var currentPrice = selectedItem.Current_price;
                var totalVolume = selectedItem.Total_volume;
                var marketCap = selectedItem.Market_cap;
                var image = selectedItem.Image;
                decimal priceChange = await CryptoData.GetPriceChange(Id);
                PriceData priceData = new PriceData();
                priceData = await CryptoData.GetHistoryPriceForOneDay(Id);
                Crypt crypt = new Crypt(Id, marketCapRank, name, symbol, currentPrice, totalVolume, marketCap, image,priceChange, priceData);


                Frame.Navigate(typeof(СurrencyPage), crypt);
            }
        }
    }
}
