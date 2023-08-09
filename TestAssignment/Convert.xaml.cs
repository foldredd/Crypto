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



namespace TestAssignment
{
    public sealed partial class Convert : Page
    {
        private ObservableCollection<Currency> crypts;
        public static readonly DependencyProperty CryptodataListProperty =
     DependencyProperty.Register("CryptodataListProperty", typeof(ObservableCollection<Currency>), typeof(Convert), new PropertyMetadata(null));

        public Convert()
        {

            this.InitializeComponent();
            this.AsyncCryptList();
            this.DataContext = this;
        }
        private void button_Click(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(MainPage));
        }
        public async Task AsyncCryptList()
        {
            await Task.Run(async () => {
                crypts = new ObservableCollection<Currency>(await CryptoData.GetCryptListAsync());

                await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () => {
                    crypts.Add(new Currency("usd", "usd"));
                    crypts.Add(new Currency("eur", "eur"));

                    SetValue(CryptodataListProperty, crypts);
                });
            });
        }
        public void SecondValue_TextChanged(object sender, TextChangedEventArgs e)
        {

        }
        public async void Click_Convert(object sender, RoutedEventArgs e)
        {
            string stringvalue = FirstValue.Text;
            decimal value;
            string currentyFromTransfer;
            string currentyWhichTransfer;
            decimal price = 5;

            try
            {
                value = System.Convert.ToDecimal(stringvalue);
                currentyFromTransfer = (MyComboBox.SelectedItem as Currency)?.Id;
                currentyWhichTransfer = (MyComboBox2.SelectedItem as Currency)?.Symbol;

                if (currentyFromTransfer != null && currentyWhichTransfer != null)
                {

                    price = await CryptoData.ConvertCrypt(currentyFromTransfer, currentyWhichTransfer);

                    value = value * price;
                    SecondValue.Text = value.ToString();
                }
            }
            catch (Exception ex)
            {
                string errorMessage = "Ошибка формата: " + ex.Message;
                ErrorTextBlock.Text = errorMessage;
                ErrorTextBlock.Visibility = Visibility.Visible;
            }
        }
        private void go_Search(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(SearchPage));
        }
    }
}
