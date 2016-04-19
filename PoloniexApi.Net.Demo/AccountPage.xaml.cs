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

        private Timer updateTimer;

        private int updateTimeMiliseconds = 5000;

        public AccountPage() : base()
        {
            InitializeComponent();

            if (!int.TryParse(ConfigurationManager.AppSettings.Get("walletUpdateTimeMiliseconds"), out updateTimeMiliseconds))
                MessageBox.Show("O parametro do App.Config walletUpdateTimeMiliseconds está setado com valor inválido, foi aplicado o valor padrão (" + updateTimeMiliseconds + ")!");

            PoloniexClient = PoloniexClient.Instance(ApiKeys.PublicKey, ApiKeys.PrivateKey);
        }

        private void LoadSummary()
        {
            double btcTheterPriceLast = 0;

            try
            {
                if (WalletService.Instance().WalletAsync != null)
                    if (WalletService.Instance().WalletAsync.Any())
                    {
                        double totalBTC = 0.0;

                        btcTheterPriceLast = MarketService.Instance().MarketAsync.First(x => x.Key.ToString().ToUpper().Equals("USDT_BTC")).Value.PriceLast;

                        double valorDolarCompraBC = double.Parse(FachadaWSSGSService.Instance().getUltimoValorVOResponseAsync.getUltimoValorVOReturn.ultimoValor.svalor.Replace(".", ","));

                        CurrencyPair CurrencyPair = CurrencyPair.Parse(string.Concat("BTC_ETH"));

                        this.Dispatcher.Invoke(DispatcherPriority.Render, (ThreadStart)delegate
                        {
                            if (WalletService.Instance().WalletAsync != null)
                            {
                                dtgAccount.Items.Clear();
                                foreach (var balance in WalletService.Instance().WalletAsync.OrderBy(x => x.Key).OrderByDescending(x => x.Value.btcValue))
                                {
                                    totalBTC = totalBTC + balance.Value.btcValue;

                                    if (balance.Value.btcValue > 0 && btcTheterPriceLast > 0 && valorDolarCompraBC > 0)
                                    {
                                        balance.Value.brzValue = Math.Round(Math.Round((btcTheterPriceLast * balance.Value.btcValue), 2) * valorDolarCompraBC, 2);

                                        balance.Value.marketValue = 0;

                                        CurrencyPair = CurrencyPair.Parse(string.Concat("BTC_", balance.Key));

                                        if (balance.Key.Equals("IFC"))
                                            CurrencyPair = CurrencyPair.Parse(string.Concat("XMR_", balance.Key));

                                        if (balance.Key.Equals("BTC"))
                                            CurrencyPair = CurrencyPair.Parse(string.Concat("USDT_BTC"));

                                                var marketValue = MarketService.Instance().MarketAsync.First(x => x.Key.Equals(CurrencyPair)).Value.PriceLast;

                                                if (balance.Key.Equals("IFC"))
                                                {
                                                    var xmrBtcValue = MarketService.Instance().MarketAsync.First(x => x.Key.Equals(CurrencyPair.Parse("BTC_XMR"))).Value.PriceLast;

                                                    balance.Value.marketValue = marketValue * xmrBtcValue;
                                                }
                                                else
                                                    balance.Value.marketValue = marketValue;
                                        dtgAccount.Items.Add(balance);
                                    }
                                }
                            }

                            var totalUSD = Math.Round((btcTheterPriceLast * totalBTC), 2);

                            txtTotalBTC.Text = totalBTC.ToString("0.00000000");
                            txtTotalUSD.Text = totalUSD.ToString("000.00000000");
                            txtTotalBRL.Text = Math.Round(Math.Round(totalUSD * valorDolarCompraBC, 2), 2).ToString("C2").Replace("R$ ", "");

                        });

                    }
            }
            finally
            {
                
            }

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
