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
        private BackgroundWorker worker;
        private Timer updateTimer;
        private FachadaWSSGS.FachadaWSSGSClient FachadaWSSGS;

        private int updateTimeMiliseconds = 5000;

        public AccountPage() : base()
        {
            InitializeComponent();

            if (!int.TryParse(ConfigurationManager.AppSettings.Get("walletUpdateTimeMiliseconds"), out updateTimeMiliseconds))
                MessageBox.Show("O parametro do App.Config lendingUpdateTimeMiliseconds está setado com valor inválido, foi aplicado o valor padrão (" + updateTimeMiliseconds + ")!");

            PoloniexClient = PoloniexClient.Instance(ApiKeys.PublicKey, ApiKeys.PrivateKey);

            FachadaWSSGS = Singleton<FachadaWSSGS.FachadaWSSGSClient>.Instance;
        }

        private async Task LoadSummaryAsync()
        {
            await Task.Run(async () =>
            {
                IDictionary<string, Balance> balances = null;
                DallEX.io.View.FachadaWSSGS.getUltimoValorVOResponse bcAsync = null;

                double btcTheterPriceLast = 0;

                try
                {
                    if (PoloniexClient != null)
                        balances = await PoloniexClient.Wallet.GetBalancesAsync();

                    if (balances != null)
                        if (balances.Any())
                        {
                            double totalBTC = 0.0;

                            btcTheterPriceLast = MarketService.Instance().MarketAsync.Where(x => x.Key.ToString().ToUpper().Equals("USDT_BTC")).OrderBy(x => x.Value.PriceLast).First().Value.PriceLast;

                            bcAsync = await FachadaWSSGS.getUltimoValorVOAsync(10813);

                            double valorDolarCompraBC = double.Parse(bcAsync.getUltimoValorVOReturn.ultimoValor.svalor.Replace(".", ","));

                            this.Dispatcher.Invoke(DispatcherPriority.Render, (ThreadStart)delegate
                            {
                                if (balances != null)
                                {
                                    dtgAccount.Items.Clear();
                                    foreach (var balance in balances.OrderBy(x => x.Key))
                                    {
                                        totalBTC = totalBTC + balance.Value.btcValue;

                                        if (balance.Value.btcValue > 0)
                                        {
                                            balance.Value.brzValue = Math.Round(Math.Round((btcTheterPriceLast * balance.Value.btcValue), 2) * valorDolarCompraBC, 2);
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
                    balances = null;
                    bcAsync = null;
                }
            });
        }

        private void dtgAccount_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
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

        private void UpdateGrid(object state)
        {
            if (!worker.IsBusy)
                worker.RunWorkerAsync();
        }

        private async void worker_DoWork(object sender, DoWorkEventArgs e)
        {
            await LoadSummaryAsync().ConfigureAwait(false);
        }

        private void Grid_Loaded(object sender, RoutedEventArgs e)
        {
            worker = new BackgroundWorker();
            worker.WorkerSupportsCancellation = true;
            worker.DoWork += worker_DoWork;

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

            if (worker != null)
            {
                if (worker.IsBusy)
                    worker.CancelAsync();

                worker.Dispose();
            }

            updateTimer = null;
            worker = null;

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
                    }
                    finally
                    {
                        updateTimer = null;
                        worker = null;
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
