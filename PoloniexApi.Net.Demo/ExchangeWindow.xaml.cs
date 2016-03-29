using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.ComponentModel;
using System.Configuration;
using Jojatekok.PoloniexAPI.MarketTools;
using System.Collections;
using System.Data.Entity;

namespace Jojatekok.PoloniexAPI.Demo
{
    public partial class ExchangeWindow : Window
    {
        private PoloniexClient PoloniexClient { get; set; }
        private BackgroundWorker worker;
        private Timer updateTimer;

        private readonly LoanContext context = new LoanContext();

        private IMarkets marketsClient;
        private static IList<MarketTools.ITrade> tradesHistory;

        private double volumeMinimum = 2.1;
        private int updateTimeMiliseconds = 15000;

        private static IList<string> currencyItems = null;
        private static string currency = "BTC_ETH";


        //private DbSet<MarketTools.MarketData> contextMarketData = null;

        public ExchangeWindow()
        {
            // Set icon from the assembly
            Icon = System.Drawing.Icon.ExtractAssociatedIcon(Assembly.GetExecutingAssembly().Location).ToImageSource();

            InitializeComponent();

            if (!int.TryParse(ConfigurationManager.AppSettings.Get("exchangeUpdateTimeMiliseconds"), out updateTimeMiliseconds))
                MessageBox.Show("O parametro do App.Config lendingUpdateTimeMiliseconds está setado com valor inválido, foi aplicado o valor padrão (" + updateTimeMiliseconds + ")!");

            if (!double.TryParse(ConfigurationManager.AppSettings.Get("exchangeUpdateTimeMiliseconds"), out volumeMinimum))
                MessageBox.Show("O parametro do App.Config exchangeVolumeMinimun está setado com valor inválido, foi aplicado o valor padrão (" + volumeMinimum + ")!");


            PoloniexClient = new PoloniexClient(ApiKeys.PublicKey, ApiKeys.PrivateKey);
            marketsClient = PoloniexClient.Markets;

            worker = new BackgroundWorker();
            worker.DoWork += worker_DoWork;

            updateTimer = new Timer(UpdateGrid, null, 0, updateTimeMiliseconds);

            currencyItems = new List<string>();

            //contextMarketData = context.MarketData;
        }

        private async void LoadMarketSummaryAsync()
        {
            var markets = await marketsClient.GetSummaryAsync();

            var ethPriceLast = markets.Where(x => x.Key.ToString().ToUpper().Equals("BTC_ETH")).OrderBy(x => x.Value.PriceLast).First().Value.PriceLast;
            var btcPriceLast = markets.Where(x => x.Key.ToString().ToUpper().Equals("USDT_BTC")).OrderBy(x => x.Value.PriceLast).First().Value.PriceLast;

            txtEthNow.Text = string.Concat("BTC/ETH: ", ethPriceLast.ToString("0.00000000"));
            txtBtcNow.Text = string.Concat("USDT/BTC: ", btcPriceLast.ToString("0.00000000"));

            dtgExchange.Items.Clear();

            foreach (var market in markets)
            {

                if (market.Key.ToString().Contains("BTC_") && market.Value.Volume24HourBase > 2.1)
                {
                    try
                    {
                        if (!currencyItems.Any(x => x.Equals(market.Key.ToString())))
                            currencyItems.Add(market.Key.ToString());

                        if (!currencyItems.Count().Equals(cbCurrency.Items.Count)){
                            cbCurrency.ItemsSource = currencyItems;                          
                            cbCurrency.Items.Refresh();

                            foreach (ComboBoxItem item in cbCurrency.Items)
                                if (item.Content.ToString().Equals("BTC_ETH")){
                                    item.IsSelected = true;
                                    break;
                                }
                        }

                        
                        if (cbCurrency.SelectedValue != null)
                            currency = cbCurrency.SelectedValue.ToString();


                        if (market.Key.ToString().Contains(currency))
                        {
                            var horaInicio = DateTime.Now.AddMinutes(-int.Parse(txtMinutos.Text));
                            var horaFim = DateTime.Now;

                            var tradesHistory = await marketsClient.GetTradesAsync(CurrencyPair.Parse(market.Key.ToString()));
                            var periodMarket = tradesHistory.Where(o => (o.Time >= horaInicio && o.Time <= horaFim));

                            var highLoanRate = periodMarket.OrderByDescending(o => o.PricePerCoin).First();
                            var lowLoanRate = periodMarket.OrderBy(o => o.PricePerCoin).First();
                            var averageLoanRate = periodMarket.Average(x => x.PricePerCoin);
                            var lastMarketOffer = periodMarket.OrderByDescending(x => x.Time).First();

                            txtHighPrice.Text = highLoanRate.PricePerCoin.ToString("0.00000000");
                            txtLowPrice.Text = string.Concat(lowLoanRate.PricePerCoin.ToString("0.00000000"), " ", "(", lowLoanRate.Time.ToShortTimeString(), ")");
                            txtPriceAverage.Text = averageLoanRate.ToString("0.00000000");
                            txtLastPrice.Text = periodMarket.OrderByDescending(x => x.Time).First().PricePerCoin.ToString("0.00000000");
                            txtDataRegistro.Text = highLoanRate.Time.ToString();

                            txtTotalBuy.Text = periodMarket.Count(x => x.Type.Equals(OrderType.Buy)).ToString() + " buys.";
                            txtTotalSell.Text = periodMarket.Count(x => x.Type.Equals(OrderType.Sell)).ToString() + " sells.";

                            var startOpenThreadTradeHistory = new ParameterizedThreadStart(OpenThreadTradeHistory);
                            var openThreadTradeHistory = new Thread(startOpenThreadTradeHistory);

                            openThreadTradeHistory.Priority = ThreadPriority.AboveNormal;
                            openThreadTradeHistory.IsBackground = true;
                            openThreadTradeHistory.Start(market.Key.ToString());

                        }
                    }
                    catch { }

                    market.Value.indiceMaluco = (market.Value.OrderSpreadPercentage * market.Value.Volume24HourBase) / 100;

                    dtgExchange.Items.Add(market);
                }
            }
            dtgExchange.Items.Refresh();
        }

        private async void OpenThreadTradeHistory(object pair)
        {
            tradesHistory = await marketsClient.GetTradesAsync(CurrencyPair.Parse(pair.ToString()));

            this.Dispatcher.Invoke(delegate
            {
                if (cbCurrency.SelectedValue != null)
                    currency = cbCurrency.SelectedValue.ToString();

                    if (pair.Equals(currency))
                    {
                        var FirstBid = tradesHistory.Where(x => x.Type == OrderType.Buy).First().Time;
                        var FirstAsk = tradesHistory.Where(x => x.Type == OrderType.Sell).First().Time;

                        lblFirstBid.Content = "1st. Bid: " + FirstBid;
                        lblFirstAsk.Content = "1st. Ask: " + FirstAsk;
                        lblGapSenconds.Content = "Trade Gap: " + (FirstAsk - FirstBid).TotalSeconds + "s.";
                    }
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
            dtgExchange.Items.Refresh();
        }

        private void UpdateGrid(object state)
        {
            if (!worker.IsBusy)
                worker.RunWorkerAsync();
        }

        private void worker_DoWork(object sender, DoWorkEventArgs e)
        {
            dtgExchange.Dispatcher.Invoke(delegate
            {
                LoadMarketSummaryAsync();
            });
        }

        private void txtMinutes_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            if (string.IsNullOrEmpty(e.Text))
                txtMinutos.Text = "0";

            if (!char.IsDigit(e.Text, e.Text.Length - 1))
                e.Handled = true;
        }


        private void Window_Closed(object sender, EventArgs e)
        {
            updateTimer.Dispose();
            updateTimer = null;

            worker.Dispose();
            worker = null;

            PoloniexClient = null;
        }
       
    }
}
