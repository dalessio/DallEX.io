using DallEX.io.API;
using DallEX.io.API.LendingTools;
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

namespace DallEX.io.View
{
    /// <summary>
    /// Interaction logic for PublicLendingOffers.xaml
    /// </summary>
    public sealed partial class LendingPage : Page, IDisposable
    {
        private PoloniexClient PoloniexClient;
        private BackgroundWorker worker;
        private Timer updateTimer;

        private LoanContext context;

        private int updateTimeMiliseconds = 3000;
        private int lendingPeriodMinute = 60;

        public LendingPage()
        {
            InitializeComponent();
        }

        private void LoadLoanOffersAsync()
        {
            if (PoloniexClient != null)
                if (context != null)
                    if (context.LendingOffers != null)
                    {
                        try
                        {
                            string currency = "BTC";

                            IDictionary<CurrencyPair, IMarketData> markets = null;

                                if (PoloniexClient != null)
                                    markets = PoloniexClient.Markets.GetSummaryAsync().Result;

                                cbCurrency.Dispatcher.Invoke(DispatcherPriority.Render, (ThreadStart)delegate
                                {
                                    currency = cbCurrency.SelectedValue.ToString().Split(':')[1].ToString();
                                });

                                PublicLoanOffersData lendings = null;

                                if (PoloniexClient != null)
                                    lendings = PoloniexClient.Lendings.GetLoanOffersAsync(currency.Trim()).Result;

                                LendingOffer firstLoanOffer = null;

                                if (lendings != null)
                                    firstLoanOffer = lendings.offers.OrderBy(x => x.rate).First();

                                if (firstLoanOffer != null)
                                    if (context != null)
                                        if (context.Database != null)
                                            if (!context.LendingOffers.Any(x => x.currency.Equals(currency) && x.amount.Equals(firstLoanOffer.amount) && x.rate.Equals(firstLoanOffer.rate) && x.rangeMin.Equals(firstLoanOffer.rangeMin) && x.rangeMax.Equals(firstLoanOffer.rangeMax)))
                                            {
                                                firstLoanOffer.currency = currency;
                                                context.LendingOffers.Add(firstLoanOffer);
                                                context.SaveChanges();

                                                DateTime horaInicio = DateTime.Now;
                                                txtMinutos.Dispatcher.Invoke(DispatcherPriority.Render, (ThreadStart)delegate
                                                {
                                                    horaInicio = DateTime.Now.AddMinutes(-int.Parse(txtMinutos.Text));
                                                });

                                                var horaFim = DateTime.Now;

                                                var periodOffers = context.LendingOffers.Where(o => (o.currency.Equals(currency) && (o.dataRegistro >= horaInicio && o.dataRegistro <= horaFim)));
                                                var highLoanRate = periodOffers.OrderByDescending(o => o.rate).First();
                                                var lowLoanRate = periodOffers.OrderBy(o => o.rate).First();

                                                var averageLoanRate = periodOffers.Average(x => x.rate);

                                                this.Dispatcher.Invoke(DispatcherPriority.Render, (ThreadStart)delegate
                                                {
                                                    txtMaiorLoanRate.Text = highLoanRate.rate.ToString("0.0000%");
                                                    txtMenorLoanRate.Text = string.Concat(lowLoanRate.rate.ToString("0.0000%"), " ", "(", lowLoanRate.dataRegistro.ToShortTimeString(), ")");
                                                    txtRateAverage.Text = averageLoanRate.ToString("0.0000%");
                                                    txtBtcEth.Text = highLoanRate.ethExchangeValue.ToString("0.00000000");
                                                    txtUsdtBtc.Text = highLoanRate.btcExchangeValue.ToString("0.00000000");
                                                    txtDataRegistro.Text = highLoanRate.dataRegistro.ToString();
                                                    txtCountLoanOffers.Text = periodOffers.Count() + " in " + txtMinutos.Text + " mins.";

                                                    DataGrid1.Items.Clear();
                                                    foreach (var offer in lendings.offers)
                                                    {
                                                        DataGrid1.Items.Add(offer);
                                                    }
                                                    DataGrid1.Items.Refresh();
                                                });

                                                periodOffers = null;
                                                highLoanRate = null;
                                                lowLoanRate = null;
                                            }

                                firstLoanOffer = null;
                                lendings = null;
                                markets = null;

                        }

                        catch (Exception ex)

                        {
                            //TODO: Log
                        }
                        finally { }

                    }
        }

        private void UpdateLoans(object state)
        {
            if (!worker.IsBusy)
                worker.RunWorkerAsync();
        }

        private void worker_DoWork(object sender, DoWorkEventArgs e)
        {
            LoadLoanOffersAsync();
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
            if (!int.TryParse(ConfigurationManager.AppSettings.Get("lendingUpdateTimeMiliseconds"), out updateTimeMiliseconds))
                MessageBox.Show("O parametro do App.Config lendingUpdateTimeMiliseconds está setado com valor inválido, foi aplicado o valor padrão (" + updateTimeMiliseconds + ")!");

            if (!int.TryParse(ConfigurationManager.AppSettings.Get("lendingPeriodMinute"), out lendingPeriodMinute))
                MessageBox.Show("O parametro do App.Config lendingUpdateTimeMiliseconds está setado com valor inválido, foi aplicado o valor padrão (" + lendingPeriodMinute + ")!");

            PoloniexClient = PoloniexClient.Instance(ApiKeys.PublicKey, ApiKeys.PrivateKey);
            
            context = Singleton<LoanContext>.Instance;

            if (worker == null)
            {
                worker = new BackgroundWorker();
                worker.DoWork += worker_DoWork;
                worker.WorkerSupportsCancellation = true;
            }

            updateTimer = new Timer(UpdateLoans, null, 0, updateTimeMiliseconds);

            txtMinutos.Text = lendingPeriodMinute.ToString();

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

                        if (context != null)
                            context.Dispose();
                    }
                    catch (Exception ex)
                    { }
                    finally
                    {
                        updateTimer = null;
                        worker = null;
                        PoloniexClient = null;
                        context = null;
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
