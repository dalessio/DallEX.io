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
using System.Data.Entity;
using DallEX.io.API;

namespace DallEX.io.View
{
    /// <summary>
    /// Interaction logic for PublicLendingOffers.xaml
    /// </summary>
    public partial class LendingWindow : Window
    {
        private PoloniexClient PoloniexClient { get; set; }
        private readonly BackgroundWorker worker = new BackgroundWorker();
        private Timer updateTimer;

        private readonly LoanContext context = new LoanContext();

        private int updateTimeMiliseconds = 3000;
        private int lendingPeriodMinute = 60;

        private DbSet<DallEX.io.API.LendingTools.LendingOffer> contextOffers = null;

        public LendingWindow()
        {
            if(!int.TryParse(ConfigurationManager.AppSettings.Get("lendingUpdateTimeMiliseconds"), out updateTimeMiliseconds))
                MessageBox.Show("O parametro do App.Config lendingUpdateTimeMiliseconds está setado com valor inválido, foi aplicado o valor padrão (" + updateTimeMiliseconds + ")!");

            if (!int.TryParse(ConfigurationManager.AppSettings.Get("lendingPeriodMinute"), out lendingPeriodMinute))
                MessageBox.Show("O parametro do App.Config lendingUpdateTimeMiliseconds está setado com valor inválido, foi aplicado o valor padrão (" + lendingPeriodMinute + ")!");

            // Set icon from the assembly
            Icon = System.Drawing.Icon.ExtractAssociatedIcon(Assembly.GetExecutingAssembly().Location).ToImageSource();

            InitializeComponent();

            PoloniexClient = PoloniexClient.Instance(ApiKeys.PublicKey, ApiKeys.PrivateKey);

            worker.DoWork += worker_DoWork;
            worker.RunWorkerCompleted += worker_RunWorkerCompleted;

            updateTimer = new Timer(UpdateLoans, null, 0, updateTimeMiliseconds);

            txtMinutos.Text = lendingPeriodMinute.ToString();

            contextOffers = context.LendingOffers;
        }

        private async void LoadLoanOffersAsync()
        {
            var markets = await PoloniexClient.Markets.GetSummaryAsync();

            DataGrid1.Dispatcher.Invoke(delegate
            {
                var currency = cbCurrency.SelectedValue.ToString().Split(':')[1].ToString();

                var lendings = PoloniexClient.Lendings.GetLoanOffersAsync(currency.Trim()).Result;

                var firstLoanOffer = lendings.offers.OrderBy(x => x.rate).First();

                if (!contextOffers.Any(x => x.currency.Equals(currency) && x.amount.Equals(firstLoanOffer.amount) && x.rate.Equals(firstLoanOffer.rate) && x.rangeMin.Equals(firstLoanOffer.rangeMin) && x.rangeMax.Equals(firstLoanOffer.rangeMax)))
                {
                    firstLoanOffer.currency = currency;
                    context.LendingOffers.Add(firstLoanOffer);
                    context.SaveChanges();

                    var horaInicio = DateTime.Now.AddMinutes(-int.Parse(txtMinutos.Text));
                    var horaFim = DateTime.Now;

                    var periodOffers = contextOffers.Where(o => (o.currency.Equals(currency) && (o.dataRegistro >= horaInicio && o.dataRegistro <= horaFim)));
                    var highLoanRate = periodOffers.OrderByDescending(o => o.rate).First();
                    var lowLoanRate = periodOffers.OrderBy(o => o.rate).First();
                    var averageLoanRate = periodOffers.Average(x => x.rate);

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

                }
            });
        }

        private void UpdateLoans(object state)
        {
            if (!worker.IsBusy)
                worker.RunWorkerAsync();
        }

        private void worker_DoWork(object sender, DoWorkEventArgs e)
        {
            try
            {
                LoadLoanOffersAsync();
            }
            catch(Exception ex)
            {                
            }

            finally
            {                
            }
        }

        private void worker_RunWorkerCompleted(object sender,
                                       RunWorkerCompletedEventArgs e)
        {
            //update ui once worker complete his work
        }

        private void menuItemLending_Click(object sender, RoutedEventArgs e)
        {
            //PublicLendingOffers window = new PublicLendingOffers();
            //window.Show();
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

            context.Dispose();
            
            PoloniexClient = null;           
        }

        private void ucHeader_Unloaded(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Unload Lending UC");
        }
    }
}
