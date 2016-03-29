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
    /// <summary>
    /// Interaction logic for Account.xaml
    /// </summary>
    public partial class AccountWindow : Window
    {
        private PoloniexClient PoloniexClient { get; set; }
        private readonly BackgroundWorker worker = new BackgroundWorker();
        private Timer updateTimer;

        private int updateTimeMiliseconds = 5000;

        private IWallet walletClient;
        private readonly LoanContext context = new LoanContext();

        public AccountWindow()
        {
            InitializeComponent();

            if (!int.TryParse(ConfigurationManager.AppSettings.Get("walletUpdateTimeMiliseconds"), out updateTimeMiliseconds))
                MessageBox.Show("O parametro do App.Config lendingUpdateTimeMiliseconds está setado com valor inválido, foi aplicado o valor padrão (" + updateTimeMiliseconds + ")!");

            // Set icon from the assembly
            Icon = System.Drawing.Icon.ExtractAssociatedIcon(Assembly.GetExecutingAssembly().Location).ToImageSource();

            PoloniexClient = new PoloniexClient(ApiKeys.PublicKey, ApiKeys.PrivateKey);
            walletClient = PoloniexClient.Wallet;

            worker = new BackgroundWorker();
            worker.DoWork += worker_DoWork;

            updateTimer = new Timer(UpdateGrid, null, 0, updateTimeMiliseconds);
        }

        private async void LoadSummaryAsync()
        {
            var balances = await walletClient.GetBalancesAsync();

            dtgAccount.Items.Clear();

            foreach (var balance in balances)
            {
                if (balance.Value.btcValue > 0)
                    dtgAccount.Items.Add(balance);
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

        private void worker_DoWork(object sender, DoWorkEventArgs e)
        {
            this.Dispatcher.Invoke(delegate
            {
                LoadSummaryAsync();
            });
        }

    }
}
