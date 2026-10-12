using System;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Threading;
using TestAssignment.Data;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

namespace TestAssignment
{
    public sealed partial class Convert : Page
    {
        private CancellationTokenSource cancellation;
        private readonly ObservableCollection<Currency> fiatCurrencies = new ObservableCollection<Currency>
        {
            new Currency("usd", "USD", "Доллар США"),
            new Currency("eur", "EUR", "Евро"),
            new Currency("gbp", "GBP", "Фунт стерлингов"),
            new Currency("uah", "UAH", "Украинская гривна")
        };

        public Convert()
        {
            InitializeComponent();
            FiatBox.ItemsSource = fiatCurrencies;
            FiatBox.SelectedIndex = 0;
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            cancellation = new CancellationTokenSource();
            try
            {
                CoinBox.ItemsSource = await CryptoData.GetAllCryptAsync(cancellation.Token);
                CoinBox.SelectedIndex = 0;
                LoadingPanel.Visibility = Visibility.Collapsed;
            }
            catch (OperationCanceledException) { }
            catch (Exception ex)
            {
                LoadingPanel.Visibility = Visibility.Collapsed;
                ShowError(MainPage.FriendlyError(ex));
            }
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            cancellation?.Cancel();
            base.OnNavigatedFrom(e);
        }

        private async void ConvertClick(object sender, RoutedEventArgs e)
        {
            ErrorText.Visibility = Visibility.Collapsed;
            decimal amount;
            if (!TryParseAmount(AmountBox.Text, out amount) || amount <= 0m)
            {
                ShowError("Введите положительное число.");
                return;
            }
            var coin = CoinBox.SelectedItem as Currency;
            var fiat = FiatBox.SelectedItem as Currency;
            if (coin == null || fiat == null)
            {
                ShowError("Выберите обе валюты.");
                return;
            }

            ConvertButton.IsEnabled = false;
            BusyRing.Visibility = Visibility.Visible;
            BusyRing.IsActive = true;
            try
            {
                decimal rate = await CryptoData.ConvertCryptAsync(coin.Id, fiat.Id, cancellation.Token);
                decimal result = amount * rate;
                ResultText.Text = result.ToString("#,##0.########", CultureInfo.CurrentCulture) + " " + fiat.SymbolDisplay;
            }
            catch (OperationCanceledException) { }
            catch (Exception ex) { ShowError(MainPage.FriendlyError(ex)); }
            finally
            {
                BusyRing.IsActive = false;
                BusyRing.Visibility = Visibility.Collapsed;
                ConvertButton.IsEnabled = true;
            }
        }

        private static bool TryParseAmount(string text, out decimal value)
        {
            return decimal.TryParse(text, NumberStyles.Number, CultureInfo.CurrentCulture, out value) ||
                   decimal.TryParse(text, NumberStyles.Number, CultureInfo.InvariantCulture, out value);
        }

        private void ShowError(string message)
        {
            ErrorText.Text = message;
            ErrorText.Visibility = Visibility.Visible;
        }

        private void GoHome(object sender, RoutedEventArgs e) => Frame.Navigate(typeof(MainPage));
        private void GoSearch(object sender, RoutedEventArgs e) => Frame.Navigate(typeof(SearchPage));
    }
}
