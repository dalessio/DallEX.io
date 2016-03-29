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

namespace DallEX.io.View
{
    /// <summary>
    /// Interaction logic for Account.xaml
    /// </summary>
    public partial class AccountWindow : Window
    {
        private PoloniexClient PoloniexClient { get; set; }
        private BackgroundWorker worker;
        private Timer updateTimer;
        private FachadaWSSGS.FachadaWSSGSClient FachadaWSSGS;

        private int updateTimeMiliseconds = 5000;

        private IWallet walletClient;

        public AccountWindow()
        {
            InitializeComponent();

            if (!int.TryParse(ConfigurationManager.AppSettings.Get("walletUpdateTimeMiliseconds"), out updateTimeMiliseconds))
                MessageBox.Show("O parametro do App.Config lendingUpdateTimeMiliseconds está setado com valor inválido, foi aplicado o valor padrão (" + updateTimeMiliseconds + ")!");

            // Set icon from the assembly
            Icon = System.Drawing.Icon.ExtractAssociatedIcon(Assembly.GetExecutingAssembly().Location).ToImageSource();

            PoloniexClient = PoloniexClient.Instance(ApiKeys.PublicKey, ApiKeys.PrivateKey);
            walletClient = PoloniexClient.Wallet;

            worker = new BackgroundWorker();
            worker.DoWork += worker_DoWork;

            updateTimer = new Timer(UpdateGrid, null, 0, updateTimeMiliseconds);

            FachadaWSSGS = new FachadaWSSGS.FachadaWSSGSClient();

        }

        private async void LoadSummaryAsync()
        {          
            var balances = await walletClient.GetBalancesAsync();

            double totalBTC = 0.0;

            dtgAccount.Items.Clear();

            foreach (var balance in balances)
            {
                totalBTC = totalBTC + balance.Value.btcValue;

                if (balance.Value.btcValue > 0)
                    dtgAccount.Items.Add(balance);
            }

            var markets = await PoloniexClient.Markets.GetSummaryAsync();
            var btcTheterPriceLast = markets.Where(x => x.Key.ToString().ToUpper().Equals("USDT_BTC")).OrderBy(x => x.Value.PriceLast).First().Value.PriceLast;

            txtTotalBTC.Text = totalBTC.ToString("0.00000000");

            var totalUSD = Math.Round((btcTheterPriceLast * totalBTC), 2);

            txtTotalUSD.Text = totalUSD.ToString("000.00000000");

            var bcAsync = await FachadaWSSGS.getUltimoValorVOAsync(10813);

            var valorDolarCompraBC = double.Parse(bcAsync.getUltimoValorVOReturn.ultimoValor.svalor.Replace(".",","));

            var totalBRL = Math.Round(totalUSD * valorDolarCompraBC, 2);

            txtTotalBRL.Text = Math.Round(totalBRL, 2).ToString("C2").Replace("R$ ","");
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

        private void worker_DoWork(object sender, DoWorkEventArgs e)
        {
            this.Dispatcher.Invoke(delegate
            {
                LoadSummaryAsync();
            });
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            updateTimer.Dispose();
            updateTimer = null;

            worker.Dispose();
            worker = null;

            walletClient = null;

            FachadaWSSGS = null;
        }
    }
}
