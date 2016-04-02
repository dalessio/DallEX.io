using DallEX.io.API;
using DallEX.io.API.MarketTools;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;

namespace DallEX.io.View
{
    public sealed partial class ExchangePage : Page, IDisposable
    {

        private PoloniexClient PoloniexClient;
        private BackgroundWorker worker;
        private Timer updateTimer;

        private IList<DallEX.io.API.MarketTools.ITrade> tradesHistory;

        private double volumeMinimum = 8.1;
        private int updateTimeMiliseconds = 15000;

        private IList<string> currencyItems = null;
        private string selectedCurrency;
        private string currentExchangeCoin;

        private ThreadStart startOpenThreadTradeHistory;
        private Thread openThreadTradeHistory;

        private static object _syncRoot = new object();

        public ExchangePage(string _currentExchangeCoin = "BTC")
        {
            InitializeComponent();
            currentExchangeCoin = _currentExchangeCoin;

            switch (currentExchangeCoin)
            {
                case "XMR":
                    selectedCurrency = "XMR_LTC";
                    break;

                case "USDT":
                    selectedCurrency = "USDT_BTC";
                    break;

                default:
                    selectedCurrency = "BTC_ETH";
                    break;
            }
        }

        private void LoadMarketSummaryAsync()
        {
            try
            {          
                if (PoloniexClient.Markets != null)
                {
                    var markets = PoloniexClient.Markets.GetSummaryAsync().Result.Where(x => x.Key.ToString().Contains(string.Concat(currentExchangeCoin, "_")) && x.Value.Volume24HourBase > volumeMinimum).OrderByDescending(x => x.Value.Volume24HourBase);

                    this.Dispatcher.Invoke(DispatcherPriority.Normal, (ThreadStart)delegate
                    {
                        if (cbCurrency.SelectedValue != null)
                            selectedCurrency = string.Concat(string.Concat(currentExchangeCoin, "_"), cbCurrency.SelectedValue.ToString());

                        dtgExchange.Items.Clear();
                        foreach (var market in markets)
                        {
                            if (currencyItems != null)
                                if (!currencyItems.Any(x => x.Equals(market.Key.ToString().Replace(string.Concat(currentExchangeCoin, "_"), ""))))
                                    currencyItems.Add(market.Key.ToString().Replace(string.Concat(currentExchangeCoin, "_"), ""));

                            market.Value.indiceMaluco = (market.Value.OrderSpreadPercentage * market.Value.Volume24HourBase) / 100;

                            dtgExchange.Items.Add(market);
                        }

                        if (currencyItems.Count() != cbCurrency.Items.Count)
                        {
                            cbCurrency.ItemsSource = currencyItems.OrderBy(x => x);
                            cbCurrency.SelectedItem = selectedCurrency.Replace(string.Concat(currentExchangeCoin, "_"), "");
                        }
                    });

                    markets = null;

                    FillDetails();
                }
            }
            catch (Exception ex)
            {
                //TODO: Log
            }
        }

        private void FillDetails()
        {
            try
            {
                if (openThreadTradeHistory == null ||
                    (openThreadTradeHistory.ThreadState != System.Threading.ThreadState.Running &&
                    openThreadTradeHistory.ThreadState != System.Threading.ThreadState.WaitSleepJoin)
                )
                {
                    lock (_syncRoot)
                    {
                        openThreadTradeHistory = new Thread(startOpenThreadTradeHistory);
                        openThreadTradeHistory.Priority = ThreadPriority.AboveNormal;
                        //openThreadTradeHistory.IsBackground = true;
                        openThreadTradeHistory.Start();
                    }
                }

            }
            catch (Exception ex)
            { }
        }

        private async void OpenThreadTradeHistory()
        {
            tradesHistory = await PoloniexClient.Markets.GetTradesAsync(CurrencyPair.Parse(selectedCurrency));

            ResetDetailsFields();

            if (tradesHistory != null)
            {
                DateTime startTime = DateTime.Now;
                txtMinutos.Dispatcher.Invoke(DispatcherPriority.Render, (ThreadStart)delegate
                {
                    startTime = DateTime.Now.AddMinutes(-int.Parse(txtMinutos.Text));
                });

                var endTime = DateTime.Now;

                IEnumerable<ITrade> periodMarket = null;
                if (tradesHistory != null)
                    periodMarket = tradesHistory.Where(o => (o.Time >= startTime && o.Time <= endTime));

                if (periodMarket != null)
                    if (periodMarket.Any())
                    {
                        var highOrderRate = periodMarket.OrderByDescending(o => o.PricePerCoin).First();
                        var lowOrderRate = periodMarket.OrderBy(o => o.PricePerCoin).First();
                        var averageOrderRate = periodMarket.Average(x => x.PricePerCoin);
                        var lastMarketOffer = periodMarket.OrderByDescending(x => x.Time).First();

                        if (periodMarket != null && tradesHistory != null)
                        {
                            this.Dispatcher.Invoke(DispatcherPriority.Normal, (ThreadStart)delegate
                            {
                                if (highOrderRate != null)
                                    txtHighPrice.Text = highOrderRate.PricePerCoin.ToString("0.00000000");

                                if (lowOrderRate != null)
                                    txtLowPrice.Text = string.Concat(lowOrderRate.PricePerCoin.ToString("0.00000000"), " ", "(", lowOrderRate.Time.ToShortTimeString(), ")");

                                txtPriceAverage.Text = averageOrderRate.ToString("0.00000000");

                                txtLastPrice.Text = periodMarket.OrderByDescending(x => x.Time).First().PricePerCoin.ToString("0.00000000");
                                txtDataRegistro.Text = highOrderRate.Time.ToString();

                                txtTotalBuy.Text = periodMarket.Count(x => x.Type.Equals(OrderType.Buy)).ToString() + " buys.";
                                txtTotalSell.Text = periodMarket.Count(x => x.Type.Equals(OrderType.Sell)).ToString() + " sells.";

                                var FirstBid = tradesHistory.Where(x => x.Type == OrderType.Buy).First().Time;
                                var FirstAsk = tradesHistory.Where(x => x.Type == OrderType.Sell).First().Time;

                                lblFirstBid.Content = "1st. Bid: " + FirstBid;
                                lblFirstAsk.Content = "1st. Ask: " + FirstAsk;
                                lblGapSenconds.Content = "Trade Gap: " + (FirstAsk - FirstBid).TotalSeconds + "s.";
                            });
                        }

                        highOrderRate = null;
                        lowOrderRate = null;
                        lastMarketOffer = null;
                    }
            }

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

        private void dtgExchange_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            var column = dtgExchange.Columns[2];

            // Clear current sort descriptions
            dtgExchange.Items.SortDescriptions.Clear();

            // Add the new sort description
            dtgExchange.Items.SortDescriptions.Add(new SortDescription(column.SortMemberPath, ListSortDirection.Descending));

            // Apply sort
            foreach (var col in dtgExchange.Columns)
            {
                col.SortDirection = null;
            }
            column.SortDirection = ListSortDirection.Descending;

            // Refresh items to display sort
            //dtgExchange.Items.Refresh();
        }

        private void UpdateGrid(object state)
        {
            if (!worker.IsBusy)
                worker.RunWorkerAsync();
        }

        private void worker_DoWork(object sender, DoWorkEventArgs e)
        {
            LoadMarketSummaryAsync();
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

            if (!double.TryParse(ConfigurationManager.AppSettings.Get("exchangeVolumeMinimun"), out volumeMinimum))
                MessageBox.Show("O parametro do App.Config exchangeVolumeMinimun está setado com valor inválido, foi aplicado o valor padrão (" + volumeMinimum + ")!");

            PoloniexClient = PoloniexClient.Instance(ApiKeys.PublicKey, ApiKeys.PrivateKey);

            worker = new BackgroundWorker();
            worker.WorkerSupportsCancellation = true;
            worker.DoWork += worker_DoWork;

            updateTimer = new Timer(UpdateGrid, null, 0, updateTimeMiliseconds);

            if (currencyItems == null)
                currencyItems = new List<string>();

            if (startOpenThreadTradeHistory == null)
                startOpenThreadTradeHistory = new ThreadStart(OpenThreadTradeHistory);

            disposedValue = false;
        }

        private void Grid_Unloaded(object sender, RoutedEventArgs e)
        {
            CancelTimer();
        }

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

                        if (openThreadTradeHistory != null)
                            if (openThreadTradeHistory.IsAlive)
                                openThreadTradeHistory.Abort();

                        if (tradesHistory != null)
                            tradesHistory.Clear();

                        if (currencyItems != null)
                            currencyItems.Clear();

                    }
                    catch (Exception ex)
                    { }
                    finally
                    {
                        updateTimer = null;
                        worker = null;
                        openThreadTradeHistory = null;
                        startOpenThreadTradeHistory = null;
                        tradesHistory = null;
                        PoloniexClient = null;

                        currencyItems = null;

                    }
                }

                disposedValue = true;
            }
        }

        private void CancelTimer()
        {
            if (updateTimer != null)
                updateTimer.Dispose();

            if (worker != null)
            {
                if (worker.IsBusy)
                    worker.CancelAsync();

                worker.Dispose();
            }

            updateTimer = null;
            worker = null;

        }

        public void Dispose()
        {
            Dispose(true);
        }
        #endregion
    }
}