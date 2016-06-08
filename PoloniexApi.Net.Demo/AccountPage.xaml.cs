using DallEX.io.API;
using DallEX.io.API.MarketTools;
using DallEX.io.API.WalletTools;
using DallEX.io.View.Library;
using DallEX.io.View.Service;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;

namespace DallEX.io.View
{
    /// <summary>
    /// Interaction logic for Account.xaml
    /// </summary>
    public sealed partial class AccountPage : Page, IDisposable
    {
        private PoloniexClient PoloniexClient;
        private SemaphoreSlim semaphoreSlim;
        private SemaphoreSlim semaphoreSlimWallet;

        private Timer updateTimer;

        private int updateTimeMiliseconds = 5000;

        private static double totalOpenOrdersValue;

        private static double xmrBtcValue;

        public AccountPage() : base()
        {
            InitializeComponent();

            if (!int.TryParse(ConfigurationManager.AppSettings.Get("walletUpdateTimeMiliseconds"), out updateTimeMiliseconds))
                MessageBox.Show("O parametro do App.Config walletUpdateTimeMiliseconds está setado com valor inválido, foi aplicado o valor padrão (" + updateTimeMiliseconds + ")!");

            PoloniexClient = PoloniexClient.Instance(ApiKeys.PublicKey, ApiKeys.PrivateKey);
        }

        private void LoadSummary()
        {
            try
            {
                if (WalletService.Instance().WalletList != null)
                    if (WalletService.Instance().WalletList.Any())
                    {
                        double totalBTC = 0.0;

                        double btcTheterPriceLast = MarketService.Instance().MarketList.First(x => x.Key.ToString().ToUpper().Equals("USDT_BTC")).Value.PriceLast;

                        double valorDolarCompraBC = double.Parse(FachadaWSSGSService.Instance().getUltimoValorVOResponse.getUltimoValorVOReturn.ultimoValor.svalor.Replace(".", ","));

                        CurrencyPair currencyPair = CurrencyPair.Parse("BTC_ETH");

                        this.Dispatcher.Invoke(DispatcherPriority.Render, (ThreadStart)async delegate
                        {
                            dtgAccount.Items.Clear();


                            if (WalletService.Instance().WalletList != null)
                            {
                                foreach (var balance in WalletService.Instance().WalletList.Where(x => x.Value.btcValue > 0).ToList().OrderBy(x => x.Key).OrderByDescending(x => x.Value.btcValue))
                                {
                                    totalBTC = totalBTC + balance.Value.btcValue;

                                    if (balance.Value.btcValue > 0 && btcTheterPriceLast > 0 && valorDolarCompraBC > 0)
                                    {
                                        balance.Value.brzValue = Math.Round(Math.Round((btcTheterPriceLast * balance.Value.btcValue), 2) * valorDolarCompraBC, 2);

                                        balance.Value.marketValue = 0;

                                        if (MarketService.Instance().MarketList.Keys.Any(x => x.Equals(CurrencyPair.Parse("BTC_" + balance.Key))))
                                        {
                                            currencyPair = CurrencyPair.Parse(string.Concat("BTC_", balance.Key));

                                            var marketValue = MarketService.Instance().MarketList.First(x => x.Key.Equals(currencyPair)).Value.PriceLast;

                                            balance.Value.marketValue = marketValue;
                                        }

                                        else if (MarketService.Instance().MarketList.Keys.Any(x => x.Equals(CurrencyPair.Parse("XMR_" + balance.Key))))
                                        {
                                            currencyPair = CurrencyPair.Parse(string.Concat("XMR_", balance.Key));

                                            xmrBtcValue = MarketService.Instance().MarketList.First(x => x.Key.Equals(CurrencyPair.Parse("BTC_XMR"))).Value.PriceLast;
                                            var marketValue = MarketService.Instance().MarketList.First(x => x.Key.Equals(currencyPair)).Value.PriceLast;
                                            balance.Value.marketValue = marketValue * xmrBtcValue;
                                        }

                                        //else if (MarketService.Instance().MarketAsync.Keys.Where(x => x.BaseCurrency.Equals("USDT")).Contains(CurrencyPair.Parse("USDT_" + balance.Key)))
                                        else
                                        {
                                            currencyPair = CurrencyPair.Parse(string.Concat("USDT_", balance.Key));

                                            var usdtBtcValue = MarketService.Instance().MarketList.First(x => x.Key.Equals(CurrencyPair.Parse("USDT_BTC"))).Value.PriceLast;
                                            var marketValue = MarketService.Instance().MarketList.First(x => x.Key.Equals(currencyPair)).Value.PriceLast;
                                            balance.Value.marketValue = marketValue * usdtBtcValue;
                                        }

                                        dtgAccount.Items.Add(balance);
                                    }
                                }
                            }

                            var totalUSD = Math.Round((btcTheterPriceLast * totalBTC), 2);
                            txtTotalBTC.Text = totalBTC.ToString("0.00000000");
                            txtTotalUSD.Text = totalUSD.ToString("000.00000000");
                            txtTotalBRL.Text = Math.Round(Math.Round(totalUSD * valorDolarCompraBC, 2), 2).ToString("C2").Replace("R$ ", "");

                            var calcTotalOpenOrders = await CalcTotalOpenOrders();
                            var totalOpenOrdersUSD = Math.Round((btcTheterPriceLast * calcTotalOpenOrders), 2);

                            txtTotalOrders.Text = (calcTotalOpenOrders).ToString("0.00000000");
                            txtTotalOrdersReais.Text = Math.Round(Math.Round(totalOpenOrdersUSD * valorDolarCompraBC, 2), 2).ToString("C2").Replace("R$ ", "");
                        });
                    }
            }
            catch (Exception ex)
            {

            }

        }

        private async Task<double> CalcTotalOpenOrders()
        {
            if (semaphoreSlimWallet != null)
            {
                await semaphoreSlimWallet.WaitAsync();

                try
                {
                    totalOpenOrdersValue = 0.0;

                    CurrencyPair currencyPair = CurrencyPair.Parse("BTC_ETH");

                    foreach (var balance in WalletService.Instance().WalletList.Where(x => x.Value.btcValue > 0).ToList())
                    {
                        if (MarketService.Instance().MarketList.Keys.Any(x => x.Equals(CurrencyPair.Parse("BTC_" + balance.Key))))
                        {
                            currencyPair = CurrencyPair.Parse(string.Concat("BTC_", balance.Key));

                            await CalcBtcOrders(currencyPair);
                        }
                        else if (MarketService.Instance().MarketList.Keys.Any(x => x.Equals(CurrencyPair.Parse("XMR_" + balance.Key))))
                        {
                            if (MarketService.Instance().MarketList.Keys.Any(x => x.Equals(currencyPair)))
                                currencyPair = CurrencyPair.Parse(string.Concat("XMR_", balance.Key));

                            await CalcXmrOrders(currencyPair);

                        }
                    }

                }
                catch (Exception ex)
                {
                }
                finally
                {
                    if (semaphoreSlimWallet != null)
                        semaphoreSlimWallet.Release();
                }
            }

            return totalOpenOrdersValue;
        }

        private async Task CalcXmrOrders(CurrencyPair currencyPair)
        {
            foreach (var trade in await PoloniexClient.Trading.GetOpenOrdersAsync(currencyPair))
                if (xmrBtcValue > 0)
                {
                    double btcTotal = (trade.AmountBase * xmrBtcValue);
                    totalOpenOrdersValue = totalOpenOrdersValue + btcTotal;
                }
        }

        private async Task CalcBtcOrders(CurrencyPair currencyPair)
        {
            foreach (var trade in await PoloniexClient.Trading.GetOpenOrdersAsync(currencyPair))
                totalOpenOrdersValue = totalOpenOrdersValue + trade.AmountBase;
        }

        private async void UpdateGrid(object state)
        {
            if (semaphoreSlim != null)
            {
                await semaphoreSlim.WaitAsync();

                try
                {
                    LoadSummary();
                }
                finally
                {
                    if (semaphoreSlim != null)
                        semaphoreSlim.Release();
                }
            }
        }

        private void Grid_Loaded(object sender, RoutedEventArgs e)
        {
            semaphoreSlim = new SemaphoreSlim(1);
            semaphoreSlimWallet = new SemaphoreSlim(1);
            updateTimer = new Timer(UpdateGrid, null, 0, updateTimeMiliseconds);
        }

        private void Grid_Unloaded(object sender, RoutedEventArgs e)
        {
            CancelTimer();
        }

        private void CancelTimer()
        {
            if (updateTimer != null)
                updateTimer.Dispose();
            updateTimer = null;
        }

        #region IDisposable Support
        private bool disposedValue = false;

        public bool IsDisposed
        {
            get
            {
                return disposedValue;
            }
        }

        void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    try
                    {
                        CancelTimer();
                        if (semaphoreSlim != null)
                            semaphoreSlim.Dispose();
                    }
                    finally
                    {
                        updateTimer = null;
                        semaphoreSlim = null;
                        PoloniexClient = null;
                    }
                }

                disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
        }
        #endregion
    }
}
