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

        private SemaphoreSlim semaphoreSlim;

        private Timer updateTimer;

        private ChartLendingWindow chartWindow = null;

        private static readonly MainWindow MainWindow = (MainWindow)Application.Current.MainWindow;

        private int updateTimeMiliseconds = 3000;
        private int lendingPeriodMinute = 60;

        private string selectedCurrency;

        public LendingPage()
        {
            InitializeComponent();
        }

        private async Task LoadLoanOffersAsync()
        {
            await Task.Run(async () =>
            {
                selectedCurrency = "BTC"; //defaultValue

                PublicLoanOffersData lendings = null;
                LendingOffer firstLoanOffer = null;
                IDictionary<CurrencyPair, IMarketData> markets = null;

                try
                {
                    if (PoloniexClient != null)
                    {
                        markets = await PoloniexClient.Markets.GetSummaryAsync();

                        cbCurrency.Dispatcher.Invoke(DispatcherPriority.Render, (ThreadStart)delegate
                        {
                            if (cbCurrency.SelectedValue != null)
                                if (cbCurrency.SelectedValue.ToString().Split(':').Any())
                                    selectedCurrency = cbCurrency.SelectedValue.ToString().Split(':')[1].ToString();
                        });

                        lendings = await PoloniexClient.Lendings.GetLoanOffersAsync(selectedCurrency.Trim());

                        if (markets != null)
                            if (markets.Any())
                                if (lendings != null)
                                {

                                    this.Dispatcher.Invoke(DispatcherPriority.Render, (ThreadStart)delegate
                                    {
                                        DataGrid1.Items.Clear();
                                        foreach (var offer in lendings.offers)
                                        {
                                            DataGrid1.Items.Add(offer);
                                        }
                                    });


                                    firstLoanOffer = lendings.offers.OrderBy(x => x.rate).First();

                                    if (firstLoanOffer != null)
                                        using (var context = new LoanContext())
                                            if (!context.LendingOffers.Any(x => x.currency.Equals(selectedCurrency) &&
                                                                                                            x.amount.Equals(firstLoanOffer.amount) &&
                                                                                                            x.rate.Equals(firstLoanOffer.rate) &&
                                                                                                            x.rangeMin.Equals(firstLoanOffer.rangeMin) &&
                                                                                                            x.rangeMax.Equals(firstLoanOffer.rangeMax)))
                                            {
                                                firstLoanOffer.currency = selectedCurrency;
                                                context.LendingOffers.Add(firstLoanOffer);
                                                await context.SaveChangesAsync();

                                            }

                                    await FillDetails(selectedCurrency);

                                }
                    }


                }
                finally
                {
                    firstLoanOffer = null;
                    lendings = null;
                    markets = null;
                }
            });
        }

        private async Task FillDetails(string currency)
        {
            await Task.Run(() =>
            {
                LendingOffer highLoanRate = null;
                LendingOffer lowLoanRate = null;
                IList<LendingOffer> periodOffers = null;

                try
                {
                    DateTime horaInicio = DateTime.Now;
                    DateTime horaFim = DateTime.Now;

                    txtMinutos.Dispatcher.Invoke(DispatcherPriority.Render, (ThreadStart)delegate
                    {
                        int tempoMinutoPeriodo = 20;

                        if (!int.TryParse(txtMinutos.Text, out tempoMinutoPeriodo))
                            tempoMinutoPeriodo = 20;

                        horaInicio = DateTime.Now.AddMinutes(-tempoMinutoPeriodo);
                    });


                    using (var context = new LoanContext())
                        periodOffers = context.LendingOffers.Where(o => (o.currency.Equals(currency) && (o.dataRegistro >= horaInicio && o.dataRegistro <= horaFim))).ToList();

                    ResetLabels();
                    if (periodOffers.Any())
                    {
                        highLoanRate = periodOffers.OrderByDescending(o => o.rate).First();
                        lowLoanRate = periodOffers.OrderBy(o => o.rate).First();

                        var averageLoanRate = periodOffers.Average(x => x.rate);

                        this.Dispatcher.Invoke(DispatcherPriority.Render, (ThreadStart)delegate
                        {
                            txtMaiorLoanRate.Text = highLoanRate.rate.ToString("0.0000%");
                            txtMenorLoanRate.Text = string.Concat(lowLoanRate.rate.ToString("0.0000%"), " ", "(", lowLoanRate.dataRegistro.ToShortTimeString(), ")");
                            txtRateAverage.Text = averageLoanRate.ToString("0.0000%");
                            txtDataRegistro.Text = highLoanRate.dataRegistro.ToString();
                            txtCountLoanOffers.Text = periodOffers.Count() + " in " + txtMinutos.Text + " mins.";
                        });
                    }
                }
                catch
                {
                    ResetLabels();
                }
                finally
                {
                    highLoanRate = null;
                    lowLoanRate = null;
                    periodOffers = null;
                }
            });
        }

        private void ResetLabels()
        {
            this.Dispatcher.Invoke(DispatcherPriority.Render, (ThreadStart)delegate
            {

                txtMaiorLoanRate.Text = 0.ToString("0.0000%");
                txtMenorLoanRate.Text = string.Concat(0.ToString("0.0000%"), " ", "(", DateTime.Now.ToShortTimeString(), ")");
                txtRateAverage.Text = 0.ToString("0.0000%");
                txtDataRegistro.Text = DateTime.Now.ToString();
                txtCountLoanOffers.Text = 0 + " in " + 0 + " mins.";
            });
        }

        private async void UpdateLoans(object state)
        {
            if (semaphoreSlim != null)
            {
                await semaphoreSlim.WaitAsync();
                try
                {
                    await LoadLoanOffersAsync();
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
            if (!int.TryParse(ConfigurationManager.AppSettings.Get("lendingUpdateTimeMiliseconds"), out updateTimeMiliseconds))
                MessageBox.Show("O parametro do App.Config lendingUpdateTimeMiliseconds está setado com valor inválido, foi aplicado o valor padrão (" + updateTimeMiliseconds + ")!");

            if (!int.TryParse(ConfigurationManager.AppSettings.Get("lendingPeriodMinute"), out lendingPeriodMinute))
                MessageBox.Show("O parametro do App.Config lendingUpdateTimeMiliseconds está setado com valor inválido, foi aplicado o valor padrão (" + lendingPeriodMinute + ")!");

            PoloniexClient = PoloniexClient.Instance(ApiKeys.PublicKey, ApiKeys.PrivateKey);

            semaphoreSlim = new SemaphoreSlim(1);

            updateTimer = new Timer(UpdateLoans, null, 0, updateTimeMiliseconds);

            txtMinutos.Text = lendingPeriodMinute.ToString();

            disposedValue = false;

            MainWindow.SendBaloonMessage("Lending", Hardcodet.Wpf.TaskbarNotification.BalloonIcon.None);
        }

        private void Grid_Unloaded(object sender, RoutedEventArgs e)
        {
            updateTimer = new Timer(UpdateLoans, null, 0, updateTimeMiliseconds*3);
            //CancelTimer();
        }

        #region ChartCandlestick
        private void btnChartHistory_Click(object sender, RoutedEventArgs e)
        {
            OpenChartHistory();
        }

        private void OpenChartHistory()
        {
            this.Dispatcher.Invoke(DispatcherPriority.Background, (ThreadStart)delegate
            {
                if (chartWindow == null)
                {
                    chartWindow = new ChartLendingWindow(selectedCurrency);
                    chartWindow.Owner = MainWindow;

                    chartWindow.WindowStartupLocation = WindowStartupLocation.CenterOwner;
                    chartWindow.Width = MainWindow.Width / 1.2;
                    chartWindow.Height = MainWindow.Height / 1.2;

                    chartWindow.Show();
                    chartWindow.Closed += ChartWindow_Closed;
                }
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

                        if (semaphoreSlim != null)
                            semaphoreSlim.Dispose();
                    }
                    finally
                    {
                        semaphoreSlim = null;

                        updateTimer = null;
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
            updateTimer = null;

        }

        public void Dispose()
        {
            Dispose(true);
        }
        #endregion
    }
}
