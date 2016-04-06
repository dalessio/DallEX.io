using DallEX.io.API;
using DallEX.io.API.MarketTools;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;

namespace DallEX.io.View
{
    public sealed partial class ExchangePage : Page, IDisposable
    {

        private PoloniexClient PoloniexClient;
        private Timer updateTimer;

        private double exchangeBTCVolumeMinimun = 8.1;
        private int updateTimeMiliseconds = 15000;

        private IList<string> currencyItems = null;
        private string selectedCurrency;
        private string currentExchangeCoin;

        private TradeHistory TradeHistoryWindow = null;

        private readonly Window MainWindow = Application.Current.MainWindow;

        public ExchangePage(string _currentExchangeCoin = "BTC")
        {
            InitializeComponent();
            currentExchangeCoin = _currentExchangeCoin;
        }

        private async Task LoadMarketSummaryAsync()
        {
            await Task.Run(async () =>
            {
                IDictionary<CurrencyPair, IMarketData> markets;

                try
                {
                    if (PoloniexClient.Markets != null)
                    {
                        markets = await PoloniexClient.Markets.GetSummaryAsync();
                        if (markets != null)
                            if (markets.Any())
                            {
                                this.Dispatcher.Invoke(DispatcherPriority.Render, (ThreadStart)delegate
                                {
                                    if (cbCurrency.SelectedValue != null)
                                        selectedCurrency = string.Concat(string.Concat(currentExchangeCoin, "_"), cbCurrency.SelectedValue.ToString());

                                    dtgExchange.Items.Clear();
                                    foreach (var market in markets.Where(x => x.Key.ToString().Contains(string.Concat(currentExchangeCoin, "_")) && x.Value.Volume24HourBase > exchangeBTCVolumeMinimun).OrderByDescending(x => x.Value.Volume24HourBase))
                                    {
                                        if (currencyItems != null)
                                            if (!currencyItems.Any(x => x.Equals(market.Key.ToString().Replace(string.Concat(currentExchangeCoin, "_"), ""))))
                                                currencyItems.Add(market.Key.ToString().Replace(string.Concat(currentExchangeCoin, "_"), ""));

                                        market.Value.indiceMaluco = (market.Value.OrderSpreadPercentage * market.Value.Volume24HourBase) / 100;

                                        market.Value.isPositiveChange = (market.Value.PriceChangePercentage > 0);

                                        dtgExchange.Items.Add(market);
                                    }

                                    if (currencyItems != null)
                                        if (currencyItems.Count() != cbCurrency.Items.Count)
                                        {
                                            cbCurrency.ItemsSource = currencyItems.OrderBy(x => x);
                                            cbCurrency.SelectedItem = selectedCurrency.Replace(string.Concat(currentExchangeCoin, "_"), "");
                                        }
                                });
                                await OpenThreadTradeHistory().ConfigureAwait(false);
                            }
                    }
                }
                finally
                {
                    markets = null;
                }
            });
        }

        private async Task OpenThreadTradeHistory()
        {
            await Task.Run(async () =>
            {

                IList<DallEX.io.API.MarketTools.ITrade> tradesHistory = null;
                ITrade highOrderRate = null;
                ITrade lowOrderRate = null;
                ITrade lastMarketOffer = null;

                try
                {
                    DateTime startTime = DateTime.Now;

                    int tempoMinutoPeriodo;

                    txtMinutos.Dispatcher.Invoke(DispatcherPriority.Render, (ThreadStart)delegate
                    {
                        if (!int.TryParse(txtMinutos.Text, out tempoMinutoPeriodo))
                            tempoMinutoPeriodo = 20;

                        startTime = DateTime.Now.AddMinutes(-tempoMinutoPeriodo);
                    });

                    var endTime = DateTime.Now;

                    if (PoloniexClient != null)
                        tradesHistory = await PoloniexClient.Markets.GetTradesAsync(CurrencyPair.Parse(selectedCurrency), startTime, endTime);

                    if (tradesHistory != null)
                    {
                        if (tradesHistory.Any())
                        {
                            highOrderRate = tradesHistory.OrderByDescending(o => o.PricePerCoin).First();
                            lowOrderRate = tradesHistory.OrderBy(o => o.PricePerCoin).First();
                            var averageOrderRate = tradesHistory.Average(x => x.PricePerCoin);
                            lastMarketOffer = tradesHistory.OrderByDescending(x => x.Time).First();

                            this.Dispatcher.Invoke(DispatcherPriority.Render, (ThreadStart)delegate
                            {
                                if (tradesHistory != null)
                                    if (tradesHistory.Any())
                                    {
                                        if (highOrderRate != null)
                                            txtHighPrice.Text = highOrderRate.PricePerCoin.ToString("0.00000000");

                                        if (lowOrderRate != null)
                                            txtLowPrice.Text = string.Concat(lowOrderRate.PricePerCoin.ToString("0.00000000"), " ", "(", lowOrderRate.Time.ToShortTimeString(), ")");

                                        txtPriceAverage.Text = averageOrderRate.ToString("0.00000000");

                                        txtLastPrice.Text = tradesHistory.OrderByDescending(x => x.Time).First().PricePerCoin.ToString("0.00000000");
                                        txtDataRegistro.Text = highOrderRate.Time.ToString();

                                        txtTotalBuy.Text = tradesHistory.Count(x => x.Type.Equals(OrderType.Buy)).ToString() + " buys.";
                                        txtTotalSell.Text = tradesHistory.Count(x => x.Type.Equals(OrderType.Sell)).ToString() + " sells.";

                                        var FirstBid = tradesHistory.Where(x => x.Type == OrderType.Buy).First().Time;
                                        var FirstAsk = tradesHistory.Where(x => x.Type == OrderType.Sell).First().Time;

                                        lblFirstBid.Content = "1st. Bid: " + FirstBid;
                                        lblFirstAsk.Content = "1st. Ask: " + FirstAsk;
                                        lblGapSenconds.Content = "Trade Gap: " + (FirstAsk - FirstBid).TotalSeconds + "s.";

                                        tradesHistory.Clear();
                                    }
                            });


                        }
                    }
                }
                catch
                {
                    ResetDetailsFields();
                }
                finally
                {
                    highOrderRate = null;
                    lowOrderRate = null;
                    lastMarketOffer = null;
                    tradesHistory = null;
                }
            });
        }

        private void ResetDetailsFields()
        {
            this.Dispatcher.Invoke(DispatcherPriority.Normal, (ThreadStart)delegate
            {
                txtHighPrice.Text = 0.ToString("0.00000000");
                txtLowPrice.Text = 0.ToString("0.00000000");
                txtPriceAverage.Text = 0.ToString("0.00000000");
                txtPriceAverage.Text = 0.ToString("0.00000000");
                txtLastPrice.Text = 0.ToString("0.00000000");
                txtDataRegistro.Text = 0.ToString("0.00000000");
                lblFirstBid.Content = "1st. Bid: " + 0.ToString("0.00000000");
                lblFirstAsk.Content = "1st. Ask: " + 0.ToString("0.00000000");

                txtTotalBuy.Text = 0 + " buys.";
                txtTotalSell.Text = 0 + " sells.";

                lblGapSenconds.Content = "Trade Gap: " + 0 + "s.";
            });
        }

        private async void UpdateGrid(object state)
        {
            await LoadMarketSummaryAsync();
        }


        private void txtMinutes_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            if (string.IsNullOrEmpty(e.Text))
                txtMinutos.Text = "0";

            if (!char.IsDigit(e.Text, e.Text.Length - 1))
                e.Handled = true;
        }

        private void Grid_Loaded(object sender, RoutedEventArgs e)
        {
            if (!int.TryParse(ConfigurationManager.AppSettings.Get("exchangeUpdateTimeMiliseconds"), out updateTimeMiliseconds))
                MessageBox.Show("O parametro do App.Config lendingUpdateTimeMiliseconds está setado com valor inválido, foi aplicado o valor padrão (" + updateTimeMiliseconds + ")!");

            switch (currentExchangeCoin)
            {
                case "XMR":
                    selectedCurrency = "XMR_LTC";
                    exchangeBTCVolumeMinimun = 0;
                    break;

                case "USDT":
                    selectedCurrency = "USDT_BTC";
                    exchangeBTCVolumeMinimun = 0;
                    break;

                default:
                    selectedCurrency = "BTC_ETH";
                    if (!double.TryParse(ConfigurationManager.AppSettings.Get("exchangeBTCVolumeMinimun"), out exchangeBTCVolumeMinimun))
                    {
                        MessageBox.Show("O parametro do App.Config exchangeBTCVolumeMinimun está setado com valor inválido, foi aplicado o valor padrão (8.1)!");
                        exchangeBTCVolumeMinimun = 8.1;
                    }
                    break;
            }

            PoloniexClient = PoloniexClient.Instance(ApiKeys.PublicKey, ApiKeys.PrivateKey);

            updateTimer = new Timer(UpdateGrid, null, 0, updateTimeMiliseconds);

            if (currencyItems == null)
                currencyItems = new List<string>();

            disposedValue = false;
        }

        private void Grid_Unloaded(object sender, RoutedEventArgs e)
        {
            CancelTimer();
        }

        private void cbCurrency_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ResetDetailsFields();
        }

        #region Trade History
        private void btnTradeHistory_Click(object sender, RoutedEventArgs e)
        {
            if (TradeHistoryWindow == null)
            {
                TradeHistoryWindow = new TradeHistory(CurrencyPair.Parse(string.Concat(currentExchangeCoin, "_", cbCurrency.SelectedItem.ToString())));

                TradeHistoryWindow.Top = MainWindow.Top;
                TradeHistoryWindow.Left = MainWindow.Left + MainWindow.Width;
                TradeHistoryWindow.Height = MainWindow.Height;
                TradeHistoryWindow.Width = 420;

                MainWindow.LocationChanged += EventHandler;
                MainWindow.SizeChanged += EventHandler;
                MainWindow.StateChanged += MainWindow_StateChanged;

                TradeHistoryWindow.Show();
                TradeHistoryWindow.Closed += TradeHistoryWindow_Closed;
            }
        }

        private void MainWindow_StateChanged(object sender, EventArgs e)
        {
            if (MainWindow.WindowState == WindowState.Maximized)
            {
                if (TradeHistoryWindow != null)
                {
                    MainWindow.WindowState = WindowState.Normal;
                    MainWindow.Top = 0;
                    MainWindow.Left = 0;
                    TradeHistoryWindow.Width = 420;
                    MainWindow.Width = System.Windows.SystemParameters.WorkArea.Width - TradeHistoryWindow.Width;
                    MainWindow.Height = System.Windows.SystemParameters.WorkArea.Height;
                    TradeHistoryWindow.Height = MainWindow.Height;                   
                }
            }
        }

        private void EventHandler(object sender, EventArgs e)
        {
            if (TradeHistoryWindow != null)
            {
                TradeHistoryWindow.Top = MainWindow.Top;
                TradeHistoryWindow.Left = MainWindow.Left + MainWindow.Width;
            }
        }

        private void TradeHistoryWindow_Closed(object sender, EventArgs e)
        {
            TradeHistoryWindow = null;
        }
        #endregion

        #region IDisposable Support
        private bool disposedValue = false;

        void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    try
                    {
                        CancelTimer();

                        if (currencyItems != null)
                            currencyItems.Clear();

                        if (TradeHistoryWindow != null)
                        {
                            TradeHistoryWindow.Close();
                            TradeHistoryWindow.Dispose();
                        }
                    }
                    finally
                    {
                        updateTimer = null;
                        PoloniexClient = null;
                        currencyItems = null;
                        TradeHistoryWindow = null;

                        disposedValue = true;
                    }
                }
            }
        }

        private void CancelTimer()
        {
            if (updateTimer != null)
                updateTimer.Dispose();

            updateTimer = null;
        }

        public void Dispose()
        {
            Dispose(true);
        }
        #endregion

    }
}