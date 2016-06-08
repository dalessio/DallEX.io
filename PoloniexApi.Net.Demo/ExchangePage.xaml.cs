using DallEX.io.API;
using DallEX.io.API.MarketTools;
using DallEX.io.View.Library;
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
using DallEX.io.View.Service;

namespace DallEX.io.View
{
    public sealed partial class ExchangePage : PageBase, IDisposable
    {
        private PoloniexClient PoloniexClient;

        private SemaphoreSlim semaphoreSlim;

        private Timer updateTimer;

        private TradeHistory TradeHistoryWindow = null;
        private ChartWindow chartWindow = null;
        private TradeWindow tradeWindow = null;

        private double exchangeBTCVolumeMinimun = 3.1;
        private int updateTimeMiliseconds = 15000;

        private IList<string> currencyItems = null;
        private string selectedCurrency;
        private string currentExchangeCoin;

        private static readonly Window MainWindow = Application.Current.MainWindow;

        public ExchangePage(string _currentExchangeCoin = "BTC") : base()
        {
            InitializeComponent();
            currentExchangeCoin = _currentExchangeCoin;
        }

        private async Task LoadMarketSummaryAsync()
        {
            try
            {
                if (MarketService.Instance().MarketList != null)
                    if (MarketService.Instance().MarketList.Any())
                    {
                        await this.Dispatcher.Invoke(async () =>
                        {
                            {
                                if (cbCurrency.SelectedValue != null)
                                    selectedCurrency = string.Concat(string.Concat(currentExchangeCoin, "_"), cbCurrency.SelectedValue.ToString());

                                dtgExchange.Items.Clear();
                                foreach (var market in MarketService.Instance().MarketList.Where(x => x.Key.ToString().Contains(string.Concat(currentExchangeCoin, "_")) && x.Value.Volume24HourBase > exchangeBTCVolumeMinimun).OrderByDescending(x => x.Value.Volume24HourBase))
                                {
                                    if (currencyItems != null)
                                        if (!currencyItems.Any(x => x.Equals(market.Key.ToString().Replace(string.Concat(currentExchangeCoin, "_"), ""))))
                                            currencyItems.Add(market.Key.ToString().Replace(string.Concat(currentExchangeCoin, "_"), ""));

                                    double spreadRateBase = market.Value.OrderSpread / 2;
                                    double volumeRateBase = market.Value.Volume24HourBase * 3;
                                    double changeRateBase = market.Value.PriceChangePercentage * 2;
                                    market.Value.indiceMaluco = Math.Round((spreadRateBase * volumeRateBase * changeRateBase * 100) / 3, 5).Normalize();

                                    market.Value.isHave = false;
                                    if(Service.WalletService.Instance().WalletList != null)
                                        market.Value.isHave = Service.WalletService.Instance().WalletList.Any(x => x.Key.Equals(market.Key.ToString().Replace(string.Concat(currentExchangeCoin, "_"), "")) && x.Value.btcValue > 0);
                                    
                                    market.Value.isPositiveChange = (market.Value.PriceChangePercentage > 0);

                                    dtgExchange.Items.Add(market);
                                }

                                if (currencyItems != null)
                                    if (currencyItems.Count() != cbCurrency.Items.Count)
                                    {
                                        cbCurrency.ItemsSource = currencyItems.OrderBy(x => x);
                                        cbCurrency.SelectedItem = selectedCurrency.Replace(string.Concat(currentExchangeCoin, "_"), "");
                                    }


                                var OpenOrdersAsync = await PoloniexClient.Markets.GetOpenOrdersAsync(CurrencyPair.Parse(selectedCurrency), 100);

                                if (OpenOrdersAsync != null)
                                {
                                    if (OpenOrdersAsync.SellOrders != null)
                                    {
                                        dtgTradeHistorySells.Items.Clear();
                                        foreach (var sell in OpenOrdersAsync.SellOrders)
                                        {
                                            dtgTradeHistorySells.Items.Add(sell);
                                        }
                                    }

                                    if (OpenOrdersAsync.BuyOrders != null)
                                    {
                                        dtgTradeHistoryBuys.Items.Clear();
                                        foreach (var buy in OpenOrdersAsync.BuyOrders)
                                        {
                                            dtgTradeHistoryBuys.Items.Add(buy);
                                        }
                                    }
                                }
                            }
                        });
                        await OpenThreadTradeHistory();
                    }

            }
            catch { }
        }

        private async Task OpenThreadTradeHistory()
        {
            await Task.Run(async () =>
            {
                ITrade highOrderRate = null;
                ITrade lowOrderRate = null;

                try
                {
                    DateTime startTime = DateTime.Now.AddHours(3);

                    int tempoMinutoPeriodo;

                    this.Dispatcher.Invoke(DispatcherPriority.Background, (ThreadStart)delegate
                     {
                         if (!int.TryParse(txtMinutos.Text, out tempoMinutoPeriodo))
                             tempoMinutoPeriodo = 20;

                         startTime = startTime.AddMinutes(-tempoMinutoPeriodo);

                         if (TradeHistoryWindow != null)
                         {
                             TradeHistoryWindow.Minutos = tempoMinutoPeriodo;
                             TradeHistoryWindow.CurrencyPair = CurrencyPair.Parse(selectedCurrency);
                         }

                     });

                    var endTime = DateTime.Now.AddHours(3);

                    if (PoloniexClient != null)
                        MarketService.Instance().TradesHistoryList = await PoloniexClient.Markets.GetTradesAsync(CurrencyPair.Parse(selectedCurrency), startTime, endTime);

                    if (MarketService.Instance().TradesHistoryList != null)
                    {
                        if (TradeHistoryWindow != null)
                            await TradeHistoryWindow.Dispatcher.Invoke(async () =>
                            {
                                await TradeHistoryWindow.LoadTradeSummaryAsync();
                            });

                        if (MarketService.Instance().TradesHistoryList.Any())
                        {
                            highOrderRate = MarketService.Instance().TradesHistoryList.OrderByDescending(o => o.PricePerCoin).First();
                            lowOrderRate = MarketService.Instance().TradesHistoryList.OrderBy(o => o.PricePerCoin).First();
                            var averageOrderRate = MarketService.Instance().TradesHistoryList.Average(x => x.PricePerCoin);

                            this.Dispatcher.Invoke(DispatcherPriority.Background, (ThreadStart)delegate
                            {
                                if (MarketService.Instance().TradesHistoryList != null)
                                    if (MarketService.Instance().TradesHistoryList.Any())
                                    {
                                        if (highOrderRate != null)
                                        {
                                            lblHighPrice.Content = string.Format("High Price ({0}):", highOrderRate.Time.ToShortTimeString());
                                            txtHighPrice.Text = highOrderRate.PricePerCoin.ToString("0.00000000") + " (" + highOrderRate.Type + ")";                                           
                                        }

                                        if (lowOrderRate != null)
                                        {
                                            lblLowPrice.Content = string.Format("Low Price ({0}):", lowOrderRate.Time.ToShortTimeString());
                                            txtLowPrice.Text = string.Format("{0} ({1})", lowOrderRate.PricePerCoin.ToString("0.00000000"), lowOrderRate.Type);
                                        }

                                        txtPriceAverage.Text = averageOrderRate.ToString("0.00000000");


                                        var lastPrice = MarketService.Instance().TradesHistoryList.OrderByDescending(x => x.Time).First();
                                        if (lastPrice != null)
                                        {
                                            lblLastPrice.Content = string.Format("Last Trade ({0}):", lastPrice.Time.ToShortTimeString());
                                            txtLastPrice.Text = lastPrice.PricePerCoin.ToString("0.00000000") + " (" + lastPrice.Type + ")";
                                        }

                                        var buys = MarketService.Instance().TradesHistoryList.Where(x => x.Type == OrderType.Buy);
                                        var sells = MarketService.Instance().TradesHistoryList.Where(x => x.Type == OrderType.Sell);

                                        lblOrdersTotal.Content = "Total Trades (" + (buys.Count() + sells.Count()) + ") :";
                                        txtTotalBuy.Text = buys.Count().ToString() + " buys.";
                                        txtTotalSell.Text = sells.Count().ToString() + " sells.";

                                        if (buys.Any())
                                            lblFirstBid.Content = "1st. Buy: " + buys.First().Time;

                                        if (sells.Any())
                                            lblFirstAsk.Content = "1st. Sell: " + sells.First().Time;

                                        if (buys.Any() && sells.Any())
                                            lblGapSenconds.Content = "Trade Gap: " + (sells.First().Time - buys.First().Time).TotalSeconds + "s.";

                                    }
                            });


                        }
                        else
                            ResetDetailsFields();
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
                lblFirstBid.Content = "1st. Bid: " + 0.ToString("0.00000000");
                lblFirstAsk.Content = "1st. Ask: " + 0.ToString("0.00000000");

                txtTotalBuy.Text = 0 + " buys.";
                txtTotalSell.Text = 0 + " sells.";

                lblGapSenconds.Content = "Trade Gap: " + 0 + "s.";
            });
        }

        private async void UpdateGrid(object state)
        {
            if (semaphoreSlim != null)
            {
                await semaphoreSlim.WaitAsync();

                try
                {
                    await LoadMarketSummaryAsync();

                    this.Dispatcher.Invoke(DispatcherPriority.Render, (ThreadStart)delegate
                    { 
                        if (tradeWindow != null)
                            tradeWindow.FillValues(CurrencyPair.Parse(selectedCurrency));
                    });

                }
                finally
                {
                    if (semaphoreSlim != null)
                        semaphoreSlim.Release();
                }
            }
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
                MessageBox.Show("O parametro do App.Config exchangeUpdateTimeMiliseconds está setado com valor inválido, foi aplicado o valor padrão (" + updateTimeMiliseconds + ")!");

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
                        MessageBox.Show("O parametro do App.Config exchangeBTCVolumeMinimun está setado com valor inválido, foi aplicado o valor padrão (3.1)!");
                        exchangeBTCVolumeMinimun = 3.1;
                    }
                    break;
            }
            PoloniexClient = PoloniexClient.Instance(ApiKeys.PublicKey, ApiKeys.PrivateKey);

            semaphoreSlim = new SemaphoreSlim(1);

            updateTimer = new Timer(UpdateGrid, null, 0, updateTimeMiliseconds);

            if (currencyItems == null)
                currencyItems = new List<string>();

            disposedValue = false;
        }

        private void Grid_Unloaded(object sender, RoutedEventArgs e)
        {
            CancelTimer();

            if (TradeHistoryWindow != null)
                TradeHistoryWindow.Close();

            if (chartWindow != null)
                chartWindow.Close();
        }

        private void cbCurrency_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ResetDetailsFields();
        }

        #region Trade Window
        private async void btnTrade_Click(object sender, RoutedEventArgs e)
        {
            await OpenTradeWindow();
        }

        private async Task OpenTradeWindow()
        {
            await Task.Run(() =>
            {
                this.Dispatcher.Invoke(DispatcherPriority.Background, (ThreadStart)delegate
                {
                    if (tradeWindow == null)
                    {
                        tradeWindow = new TradeWindow(CurrencyPair.Parse(selectedCurrency));
                        tradeWindow.Owner = MainWindow;

                        tradeWindow.WindowStartupLocation = WindowStartupLocation.CenterOwner;

                        tradeWindow.Show();
                        tradeWindow.Closed += TradeWindow_Closed;
                    }
                });
            });
        }

        private void TradeWindow_Closed(object sender, EventArgs e)
        {
            this.Dispatcher.Invoke(DispatcherPriority.Background, (ThreadStart)delegate
            {
                tradeWindow = null;
            });

        }
        #endregion

        #region ChartCandlestick
        private async void btnChartHistory_Click(object sender, RoutedEventArgs e)
        {
            await OpenChartHistory();
        }

        private async Task OpenChartHistory()
        {
            await Task.Run(() =>
            {
                this.Dispatcher.Invoke(DispatcherPriority.Background, (ThreadStart)delegate
                {
                    if (chartWindow == null)
                    {
                        chartWindow = new ChartWindow(CurrencyPair.Parse(selectedCurrency));
                        chartWindow.Owner = MainWindow;

                        chartWindow.WindowStartupLocation = WindowStartupLocation.CenterOwner;
                        chartWindow.Width = MainWindow.Width / 1.2;
                        chartWindow.Height = MainWindow.Height / 1.2;

                        chartWindow.Show();
                        chartWindow.Closed += ChartWindow_Closed;
                    }
                });
            });
        }
        
        private void ChartWindow_Closed(object sender, EventArgs e)
        {
            this.Dispatcher.Invoke(DispatcherPriority.Background, (ThreadStart)delegate
            {
                chartWindow = null;
            });

        }
        #endregion

        #region Trade History
        private async void btnOpenOrders_Click(object sender, RoutedEventArgs e)
        {
            await OpenTradeHistory(2);
        }

        private async void btnTradeHistory_Click(object sender, RoutedEventArgs e)
        {
            await OpenTradeHistory();
        }

        private async Task OpenTradeHistory(int selectedIndex = 0)
        {
            await Task.Run(() =>
            {
                this.Dispatcher.Invoke(DispatcherPriority.Background, (ThreadStart)delegate
                {
                    if (TradeHistoryWindow == null)
                    {
                        TradeHistoryWindow = new TradeHistory(CurrencyPair.Parse(selectedCurrency), selectedIndex);

                        TradeHistoryWindow.Top = MainWindow.Top;
                        TradeHistoryWindow.Left = MainWindow.Left + MainWindow.Width;
                        TradeHistoryWindow.Height = MainWindow.Height;
                        TradeHistoryWindow.Width = 430;

                        MainWindow.LocationChanged += EventHandler;
                        MainWindow.SizeChanged += EventHandler;
                        MainWindow.StateChanged += MainWindow_StateChanged;
                        TradeHistoryWindow.StateChanged += MainWindow_StateChanged;

                        TradeHistoryWindow.Show();
                        TradeHistoryWindow.Closed += TradeHistoryWindow_Closed;

                        MainWindow.WindowState = WindowState.Maximized;
                    }
                });
            });
        }

        private void MainWindow_StateChanged(object sender, EventArgs e)
        {

            if ((sender as Window).WindowState == WindowState.Maximized)
            {
                if (TradeHistoryWindow != null)
                {
                    MainWindow.WindowState = WindowState.Normal;
                    TradeHistoryWindow.WindowState = WindowState.Normal;

                    MainWindow.Top = 0;
                    MainWindow.Left = 0;

                    TradeHistoryWindow.Width = 430;

                    MainWindow.Width = System.Windows.SystemParameters.WorkArea.Width - TradeHistoryWindow.Width;
                    MainWindow.Height = System.Windows.SystemParameters.WorkArea.Height;

                    TradeHistoryWindow.Height = MainWindow.Height;

                    MainWindow.Focus();
                    TradeHistoryWindow.Focus();
                }
            }
            else if ((sender as Window).WindowState == WindowState.Minimized)
            {
                MainWindow.WindowState = (sender as Window).WindowState;

                if (TradeHistoryWindow != null)
                    TradeHistoryWindow.WindowState = (sender as Window).WindowState;
            }

            else if ((sender as Window).WindowState == WindowState.Normal)
            {
                MainWindow.WindowState = WindowState.Normal;
                MainWindow.Focus();

                if (TradeHistoryWindow != null)
                {
                    TradeHistoryWindow.WindowState = WindowState.Normal;

                    TradeHistoryWindow.Width = 430;
                    TradeHistoryWindow.Height = MainWindow.Height;

                    TradeHistoryWindow.Focus();
                }
            }

        }

        private void EventHandler(object sender, EventArgs e)
        {
            if (TradeHistoryWindow != null)
            {
                TradeHistoryWindow.Top = MainWindow.Top;
                TradeHistoryWindow.Left = MainWindow.Left + MainWindow.Width;

                TradeHistoryWindow.Height = MainWindow.Height;
            }
        }

        private void TradeHistoryWindow_Closed(object sender, EventArgs e)
        {
            this.Dispatcher.Invoke(DispatcherPriority.Background, (ThreadStart)delegate
            {
                TradeHistoryWindow = null;
            });
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
                            TradeHistoryWindow.Close();

                        if (chartWindow != null)
                            chartWindow.Close();

                        if (MarketService.Instance().TradesHistoryList != null)
                            MarketService.Instance().TradesHistoryList.Clear();

                        if (semaphoreSlim != null)
                            semaphoreSlim.Dispose();
                    }
                    finally
                    {
                        updateTimer = null;
                        semaphoreSlim = null;
                        PoloniexClient = null;
                        currencyItems = null;
                        TradeHistoryWindow = null;
                        chartWindow = null;

                        MarketService.Instance().TradesHistoryList = null;

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