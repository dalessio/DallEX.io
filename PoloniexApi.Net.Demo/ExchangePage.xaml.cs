﻿using DallEX.io.API;
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
            await Task.Run(async () =>
            {
                try
                {
                    if (MarketService.Instance().MarketAsync != null)
                        if (MarketService.Instance().MarketAsync.Any())
                        {
                            await this.Dispatcher.Invoke(async () => {
                                {
                                    if (cbCurrency.SelectedValue != null)
                                        selectedCurrency = string.Concat(string.Concat(currentExchangeCoin, "_"), cbCurrency.SelectedValue.ToString());

                                    dtgExchange.Items.Clear();
                                    foreach (var market in MarketService.Instance().MarketAsync.Where(x => x.Key.ToString().Contains(string.Concat(currentExchangeCoin, "_")) && x.Value.Volume24HourBase > exchangeBTCVolumeMinimun).OrderByDescending(x => x.Value.Volume24HourBase))
                                    {
                                        if (currencyItems != null)
                                            if (!currencyItems.Any(x => x.Equals(market.Key.ToString().Replace(string.Concat(currentExchangeCoin, "_"), ""))))
                                                currencyItems.Add(market.Key.ToString().Replace(string.Concat(currentExchangeCoin, "_"), ""));


                                        market.Value.indiceMaluco = (market.Value.OrderSpreadPercentage * market.Value.Volume24HourBase) / 100;

                                        market.Value.isHave = Service.WalletService.Instance().WalletAsync.Any(x => x.Key.Equals(market.Key.ToString().Replace(string.Concat(currentExchangeCoin, "_"), "")) && x.Value.btcValue > 0);
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
                                        if(OpenOrdersAsync.SellOrders != null){
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
                catch{}
            });
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
                        TradeHistoryService.Instance().TradesHistoryListAsync = await PoloniexClient.Markets.GetTradesAsync(CurrencyPair.Parse(selectedCurrency), startTime, endTime);

                    if (TradeHistoryService.Instance().TradesHistoryListAsync != null)
                    {
                        if (TradeHistoryService.Instance().TradesHistoryListAsync.Any())
                        {
                            if (TradeHistoryWindow != null)
                                await TradeHistoryWindow.Dispatcher.Invoke(async () =>
                                {
                                    await TradeHistoryWindow.LoadTradeSummaryAsync();
                                });

                            highOrderRate = TradeHistoryService.Instance().TradesHistoryListAsync.OrderByDescending(o => o.PricePerCoin).First();
                            lowOrderRate = TradeHistoryService.Instance().TradesHistoryListAsync.OrderBy(o => o.PricePerCoin).First();
                            var averageOrderRate = TradeHistoryService.Instance().TradesHistoryListAsync.Average(x => x.PricePerCoin);

                            this.Dispatcher.Invoke(DispatcherPriority.Background, (ThreadStart)delegate
                            {
                                if (TradeHistoryService.Instance().TradesHistoryListAsync != null)
                                    if (TradeHistoryService.Instance().TradesHistoryListAsync.Any())
                                    {
                                        if (highOrderRate != null)
                                            txtHighPrice.Text = highOrderRate.PricePerCoin.ToString("0.00000000");

                                        if (lowOrderRate != null)
                                            txtLowPrice.Text = string.Concat(lowOrderRate.PricePerCoin.ToString("0.00000000"), " ", "(", lowOrderRate.Time.ToShortTimeString(), ")");

                                        txtPriceAverage.Text = averageOrderRate.ToString("0.00000000");

                                        txtLastPrice.Text = TradeHistoryService.Instance().TradesHistoryListAsync.OrderByDescending(x => x.Time).First().PricePerCoin.ToString("0.00000000");
                                        txtDataRegistro.Text = highOrderRate.Time.ToString();

                                        var buys = TradeHistoryService.Instance().TradesHistoryListAsync.Where(x => x.Type == OrderType.Buy);
                                        var sells = TradeHistoryService.Instance().TradesHistoryListAsync.Where(x => x.Type == OrderType.Sell);

                                        lblOrdersTotal.Content = "Orders Total (" + (buys.Count() + sells.Count()) + ") :";
                                        txtTotalBuy.Text = buys.Count().ToString() + " buys.";
                                        txtTotalSell.Text = sells.Count().ToString() + " sells.";

                                        if (buys.Any())
                                            lblFirstBid.Content = "1st. Bid: " + buys.First().Time;

                                        if (sells.Any())
                                            lblFirstAsk.Content = "1st. Ask: " + sells.First().Time;

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
            if (semaphoreSlim != null)
            {
                await semaphoreSlim.WaitAsync();

                try
                {
                    await LoadMarketSummaryAsync();
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
        }

        private void cbCurrency_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ResetDetailsFields();
        }

        #region Trade History
        private async void btnTradeHistory_Click(object sender, RoutedEventArgs e)
        {
            await OpenTradeHistory();
        }

        private async Task OpenTradeHistory()
        {
            await Task.Run(() =>
            {
                this.Dispatcher.Invoke(DispatcherPriority.Background, (ThreadStart)delegate
                {
                    if (TradeHistoryWindow == null)
                    {
                        TradeHistoryWindow = new TradeHistory(CurrencyPair.Parse(selectedCurrency));

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
                    MainWindow.WindowState = (sender as Window).WindowState;
                    TradeHistoryWindow.WindowState = (sender as Window).WindowState;

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

                        if (TradeHistoryService.Instance().TradesHistoryListAsync != null)
                            TradeHistoryService.Instance().TradesHistoryListAsync.Clear();

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

                        TradeHistoryService.Instance().TradesHistoryListAsync = null;

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