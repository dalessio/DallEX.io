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
using System.Windows.Threading;

namespace DallEX.io.View
{
    /// <summary>
    /// Interaction logic for Account.xaml
    /// </summary>
    public sealed partial class AccountPage : PageBase, IDisposable
    {
        private PoloniexClient PoloniexClient;
        private SemaphoreSlim semaphoreSlim;

        private Timer updateTimer;

        private FachadaWSSGS.FachadaWSSGSClient FachadaWSSGS;

        private int updateTimeMiliseconds = 5000;

        public AccountPage() : base()
        {
            InitializeComponent();

            if (!int.TryParse(ConfigurationManager.AppSettings.Get("walletUpdateTimeMiliseconds"), out updateTimeMiliseconds))
                MessageBox.Show("O parametro do App.Config lendingUpdateTimeMiliseconds est� setado com valor inv�lido, foi aplicado o valor padr�o (" + updateTimeMiliseconds + ")!");

            PoloniexClient = PoloniexClient.Instance(ApiKeys.PublicKey, ApiKeys.PrivateKey);

            FachadaWSSGS = Singleton<FachadaWSSGS.FachadaWSSGSClient>.Instance;
        }

        private async Task LoadSummaryAsync()
        {
            await Task.Run(async () =>
            {
                DallEX.io.View.FachadaWSSGS.getUltimoValorVOResponse bcAsync = null;

                double btcTheterPriceLast = 0;

                try
                {
                    if (PoloniexClient != null)
                        WalletService.Instance().WalletAsync = await PoloniexClient.Wallet.GetBalancesAsync();

                    if (WalletService.Instance().WalletAsync != null)
                        if (WalletService.Instance().WalletAsync.Any())
                        {
                            double totalBTC = 0.0;

                            btcTheterPriceLast = MarketService.Instance().MarketAsync.Where(x => x.Key.ToString().ToUpper().Equals("USDT_BTC")).OrderBy(x => x.Value.PriceLast).First().Value.PriceLast;

                            bcAsync = await FachadaWSSGS.getUltimoValorVOAsync(10813);

                            double valorDolarCompraBC = double.Parse(bcAsync.getUltimoValorVOReturn.ultimoValor.svalor.Replace(".", ","));

                            CurrencyPair CurrencyPair = CurrencyPair.Parse(string.Concat("BTC_ETH"));

                            this.Dispatcher.Invoke(DispatcherPriority.Render, (ThreadStart)delegate
                            {
                                if (WalletService.Instance().WalletAsync != null)
                                {
                                    dtgAccount.Items.Clear();
                                    foreach (var balance in WalletService.Instance().WalletAsync.OrderBy(x => x.Key))
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

                                            var marketAsync = MarketService.Instance().MarketAsync.Where(x => x.Key.Equals(CurrencyPair)).OrderBy(x => x.Value.PriceLast);

                                            if (marketAsync != null)
                                                if (marketAsync.Any())
                                                    if (!balance.Key.Equals("IFC"))
                                                        balance.Value.marketValue = marketAsync.First().Value.PriceLast;
                                                    else
                                                    {
                                                        var ifcXmrValue = marketAsync.First().Value.PriceLast;
                                                        var xmrBtcValue = MarketService.Instance().MarketAsync.Where(x => x.Key.Equals(CurrencyPair.Parse("BTC_XMR"))).OrderBy(x => x.Value.PriceLast).First().Value.PriceLast;

                                                        balance.Value.marketValue = ifcXmrValue * xmrBtcValue;
                                                    }


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
                    bcAsync = null;
                }
            });
        }

        private bool dtgAccount_Loaded_fistTime = true;

        private void dtgAccount_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            if (dtgAccount_Loaded_fistTime)
            {
                dtgAccount_Loaded_fistTime = false;

                var column = dtgAccount.Columns[2];

                // Clear current sort descriptions
                dtgAccount.Items.SortDescriptions.Clear();

                // Add the new sort description
                dtgAccount.Items.SortDescriptions.Add(new SortDescription(column.SortMemberPath, ListSortDirection.Descending));

                // Apply sort
                foreach (var col in dtgAccount.Columns)
                {
                    col.SortDirection = null;
                }
                column.SortDirection = ListSortDirection.Descending;
            }
        }

        private async void UpdateGrid(object state)
        {
            if (semaphoreSlim != null)
            {
                await semaphoreSlim.WaitAsync();

                try
                {
                    await LoadSummaryAsync();
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

                        if (FachadaWSSGS != null)
                            FachadaWSSGS.Close();

                        if (semaphoreSlim != null)
                            semaphoreSlim.Dispose();
                    }
                    finally
                    {
                        updateTimer = null;
                        semaphoreSlim = null;
                        FachadaWSSGS = null;
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
