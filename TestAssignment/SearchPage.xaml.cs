using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using TestAssignment.Data;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

namespace TestAssignment
{
    public sealed partial class SearchPage : Page
    {
        private ObservableCollection<Currency> allCurrencies = new ObservableCollection<Currency>();
        private CancellationTokenSource cancellation;
        private bool openingDetails;
        public ObservableCollection<Currency> Results { get; } = new ObservableCollection<Currency>();

        public SearchPage()
        {
            InitializeComponent();
            DataContext = this;
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            await LoadAsync();
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            cancellation?.Cancel();
            base.OnNavigatedFrom(e);
        }

        private async Task LoadAsync()
        {
            cancellation?.Cancel();
            cancellation = new CancellationTokenSource();
            LoadingPanel.Visibility = Visibility.Visible;
            ErrorPanel.Visibility = Visibility.Collapsed;
            EmptyPanel.Visibility = Visibility.Collapsed;
            ResultsList.Visibility = Visibility.Collapsed;
            try
            {
                allCurrencies = await CryptoData.GetAllCryptAsync(cancellation.Token);
                ApplyFilter();
                ResultsList.Visibility = Visibility.Visible;
            }
            catch (OperationCanceledException) { }
            catch (Exception ex)
            {
                ErrorText.Text = MainPage.FriendlyError(ex);
                ErrorPanel.Visibility = Visibility.Visible;
            }
            finally { LoadingPanel.Visibility = Visibility.Collapsed; }
        }

        private void SearchChanged(object sender, TextChangedEventArgs e) => ApplyFilter();

        private void ApplyFilter()
        {
            string query = (SearchBox?.Text ?? string.Empty).Trim();
            var filtered = string.IsNullOrEmpty(query)
                ? allCurrencies
                : new ObservableCollection<Currency>(allCurrencies.Where(item =>
                    (item.Name ?? string.Empty).IndexOf(query, StringComparison.CurrentCultureIgnoreCase) >= 0 ||
                    (item.Symbol ?? string.Empty).IndexOf(query, StringComparison.OrdinalIgnoreCase) >= 0));
            Results.Clear();
            foreach (Currency item in filtered) Results.Add(item);
            EmptyPanel.Visibility = Results.Count == 0 ? Visibility.Visible : Visibility.Collapsed;
        }

        private async void CurrencySelected(object sender, SelectionChangedEventArgs e)
        {
            var selected = ResultsList.SelectedItem as Currency;
            if (selected == null || openingDetails) return;
            openingDetails = true;
            ResultsList.IsEnabled = false;
            try
            {
                var changeTask = CryptoData.GetPriceChangeAsync(selected.Id, cancellation.Token);
                var historyTask = CryptoData.GetHistoryPriceForOneDayAsync(selected.Id, cancellation.Token);
                await Task.WhenAll(changeTask, historyTask);
                Frame.Navigate(typeof(СurrencyPage), new Crypt(selected.Id, selected.Market_cap_rank, selected.Name,
                    selected.Symbol, selected.Current_price, selected.Total_volume, selected.Market_cap,
                    selected.Image, changeTask.Result, historyTask.Result));
            }
            catch (OperationCanceledException) { }
            catch (Exception ex)
            {
                ErrorText.Text = MainPage.FriendlyError(ex);
                ErrorPanel.Visibility = Visibility.Visible;
                ResultsList.Visibility = Visibility.Collapsed;
            }
            finally
            {
                ResultsList.SelectedItem = null;
                ResultsList.IsEnabled = true;
                openingDetails = false;
            }
        }

        private async void RetryClick(object sender, RoutedEventArgs e) => await LoadAsync();
        private void GoHome(object sender, RoutedEventArgs e) => Frame.Navigate(typeof(MainPage));
        private void GoConvert(object sender, RoutedEventArgs e) => Frame.Navigate(typeof(Convert));
    }
}
