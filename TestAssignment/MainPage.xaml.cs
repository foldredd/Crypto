using System;
using System.Collections.ObjectModel;
using System.Threading;
using System.Threading.Tasks;
using TestAssignment.Data;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

namespace TestAssignment
{
    public sealed partial class MainPage : Page
    {
        private CancellationTokenSource cancellation;
        private bool openingDetails;

        public ObservableCollection<Currency> CryptodataListProperty { get; private set; } = new ObservableCollection<Currency>();

        public MainPage()
        {
            InitializeComponent();
            DataContext = this;
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            await LoadCurrenciesAsync();
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            cancellation?.Cancel();
            base.OnNavigatedFrom(e);
        }

        private async Task LoadCurrenciesAsync()
        {
            cancellation?.Cancel();
            cancellation = new CancellationTokenSource();
            LoadingPanel.Visibility = Visibility.Visible;
            ErrorPanel.Visibility = Visibility.Collapsed;
            CurrencyList.Visibility = Visibility.Collapsed;

            try
            {
                ObservableCollection<Currency> items = await CryptoData.GetCryptListAsync(cancellation.Token);
                CryptodataListProperty.Clear();
                foreach (Currency item in items) CryptodataListProperty.Add(item);
                CurrencyList.Visibility = Visibility.Visible;
            }
            catch (OperationCanceledException) { }
            catch (Exception ex)
            {
                ErrorText.Text = FriendlyError(ex);
                ErrorPanel.Visibility = Visibility.Visible;
            }
            finally
            {
                LoadingPanel.Visibility = Visibility.Collapsed;
            }
        }

        private async void RefreshClick(object sender, RoutedEventArgs e) => await LoadCurrenciesAsync();
        private void GoConvert(object sender, RoutedEventArgs e) => Frame.Navigate(typeof(Convert));
        private void GoSearch(object sender, RoutedEventArgs e) => Frame.Navigate(typeof(SearchPage));

        private async void CurrencySelected(object sender, SelectionChangedEventArgs e)
        {
            var selected = CurrencyList.SelectedItem as Currency;
            if (selected == null || openingDetails) return;
            openingDetails = true;
            CurrencyList.IsEnabled = false;

            try
            {
                var changeTask = CryptoData.GetPriceChangeAsync(selected.Id, cancellation.Token);
                var historyTask = CryptoData.GetHistoryPriceForOneDayAsync(selected.Id, cancellation.Token);
                await Task.WhenAll(changeTask, historyTask);
                var details = new Crypt(selected.Id, selected.Market_cap_rank, selected.Name, selected.Symbol,
                    selected.Current_price, selected.Total_volume, selected.Market_cap, selected.Image,
                    changeTask.Result, historyTask.Result);
                Frame.Navigate(typeof(СurrencyPage), details);
            }
            catch (OperationCanceledException) { }
            catch (Exception ex)
            {
                ErrorText.Text = FriendlyError(ex);
                ErrorPanel.Visibility = Visibility.Visible;
                CurrencyList.Visibility = Visibility.Collapsed;
            }
            finally
            {
                CurrencyList.SelectedItem = null;
                CurrencyList.IsEnabled = true;
                openingDetails = false;
            }
        }

        internal static string FriendlyError(Exception ex)
        {
            if (ex is TaskCanceledException) return "Сервис не ответил вовремя. Проверьте подключение и попробуйте снова.";
            return "Проверьте подключение к интернету и повторите попытку. " + ex.Message;
        }
    }
}
