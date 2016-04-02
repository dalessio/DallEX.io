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
using DallEX.io.API.MarketTools;
using System.Collections;
using System.Data.Entity;
using DallEX.io.API;

using DallEX.io.View.Library;
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
        }

        private async Task LoadSummaryAsync()
        {
            try
            {
                if (PoloniexClient != null)
                {
                    var balances = await PoloniexClient.Wallet.GetBalancesAsync();

                    double totalBTC = 0.0;

                    this.Dispatcher.Invoke(DispatcherPriority.Render, (ThreadStart)delegate
                    {
                        dtgAccount.Items.Clear();
                        if (balances != null)
                            foreach (var balance in balances)
                            {
                                totalBTC = totalBTC + balance.Value.btcValue;

                                if (balance.Value.btcValue > 0)
                                    dtgAccount.Items.Add(balance);
                            }
                    });

                    if (PoloniexClient != null)
                    {
                        double btcTheterPriceLast = 0;

                        var markets = await PoloniexClient.Markets.GetSummaryAsync();
                        if (markets != null)
                        {
                            btcTheterPriceLast = markets.Where(x => x.Key.ToString().ToUpper().Equals("USDT_BTC")).OrderBy(x => x.Value.PriceLast).First().Value.PriceLast;

                            var totalUSD = Math.Round((btcTheterPriceLast * totalBTC), 2);
                            if (FachadaWSSGS != null)
                            {
                                var bcAsync = await FachadaWSSGS.getUltimoValorVOAsync(10813);

                                if (bcAsync != null)
                                {
                                    var valorDolarCompraBC = double.Parse(bcAsync.getUltimoValorVOReturn.ultimoValor.svalor.Replace(".", ","));

                                    var totalBRL = Math.Round(totalUSD * valorDolarCompraBC, 2);

                                    this.Dispatcher.Invoke(DispatcherPriority.Render, (ThreadStart)delegate
                                    {
                                        txtTotalBTC.Text = totalBTC.ToString("0.00000000");
                                        txtTotalUSD.Text = totalUSD.ToString("000.00000000");
                                        txtTotalBRL.Text = Math.Round(totalBRL, 2).ToString("C2").Replace("R$ ", "");
                                    });

                                    bcAsync = null;
                                }
                            }
                        }
                        balances = null;
                        markets = null;
                    }
                }
            }
#pragma warning disable CS0168 // The variable 'ex' is declared but never used
            catch (Exception ex)
#pragma warning restore CS0168 // The variable 'ex' is declared but never used
            {
                //todo: log
            }
            finally
            {
            }

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

            // Refresh items to display sort
            dtgAccount.Items.Refresh();
        }

        private void UpdateGrid(object state)
        {
            if (!worker.IsBusy)
                worker.RunWorkerAsync();
        }

        private async void worker_DoWork(object sender, DoWorkEventArgs e)
        {
            await LoadSummaryAsync();
        }

        private void Grid_Loaded(object sender, RoutedEventArgs e)
        {
            if (!int.TryParse(ConfigurationManager.AppSettings.Get("walletUpdateTimeMiliseconds"), out updateTimeMiliseconds))
                MessageBox.Show("O parametro do App.Config lendingUpdateTimeMiliseconds está setado com valor inválido, foi aplicado o valor padrão (" + updateTimeMiliseconds + ")!");

            // Set icon from the assembly
            PoloniexClient = PoloniexClient.Instance(ApiKeys.PublicKey, ApiKeys.PrivateKey);

            worker = new BackgroundWorker();
            worker.WorkerSupportsCancellation = true;
            worker.DoWork += worker_DoWork;

            updateTimer = new Timer(UpdateGrid, null, 0, updateTimeMiliseconds);

            FachadaWSSGS = new FachadaWSSGS.FachadaWSSGSClient();

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

                        if (FachadaWSSGS != null)
                            FachadaWSSGS.Close();

                    }
                    catch (Exception ex)
                    {

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
