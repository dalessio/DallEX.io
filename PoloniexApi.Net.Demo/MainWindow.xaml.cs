using DallEX.io.API;
using System;
using System.ComponentModel;
using System.Reflection;
using System.Threading;
using System.Windows;
using System.Windows.Controls;

namespace DallEX.io.View
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private BackgroundWorker worker;
        private Timer updateTimer;

        private LendingPage lendingPage;
        private ExchangePage exchangePage;
        private AccountPage accountPage;

        private TabItem ExchangeTab;
        private TabItem AccountTab;
        private TabItem LendingTab;

        public MainWindow()
        {
            InitializeComponent();

            // Set icon from the assembly
            this.Icon = System.Drawing.Icon.ExtractAssociatedIcon(Assembly.GetExecutingAssembly().Location).ToImageSource();

            worker = new BackgroundWorker();

            worker.DoWork += worker_DoWork;
            updateTimer = new Timer(UpdateView, null, 0, 16000);

            lendingPage = new LendingPage();
            LendingTab = new TabItem();
            LendingTab.Header = "Lending";
            LendingTab.Background = System.Windows.Media.Brushes.Red;
            TabMain.Items.Add(LendingTab);

            exchangePage = new ExchangePage();
            ExchangeTab = new TabItem();
            //ExchangeTab.Content = exchangePage.Content;
            ExchangeTab.Header = "Exchange";
            ExchangeTab.Background = System.Windows.Media.Brushes.Yellow;
            TabMain.Items.Add(ExchangeTab);

            accountPage = new AccountPage();
            AccountTab = new TabItem();
            //AccountTab.Content = accountPage.Content;
            AccountTab.Header = "Account";
            AccountTab.Background = System.Windows.Media.Brushes.Green;
            TabMain.Items.Add(AccountTab);
        }

        private void UpdateView(object state)
        {
            if (!worker.IsBusy)
                worker.RunWorkerAsync();
        }

        private void worker_DoWork(object sender, DoWorkEventArgs e)
        {
            ucHeader.Dispatcher.Invoke(delegate
            {
                ucHeader.LoadLoanOffersAsync(PoloniexClient.Instance(ApiKeys.PublicKey, ApiKeys.PrivateKey));
            });
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            worker.Dispose();
            worker = null;

            updateTimer.Dispose();
            updateTimer = null;

            lendingPage = null;
            exchangePage = null;
            accountPage = null;

            TabMain.Items.Clear();
            TabMain = null;

            ExchangeTab.Content = null;
            AccountTab.Content = null;
            LendingTab.Content = null;

            ExchangeTab = null;
            AccountTab = null;
            LendingTab = null;
        }

        private void TabMain_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ExchangeTab.Content = null;
            AccountTab.Content = null;
            LendingTab.Content = null;
            
            switch(TabMain.SelectedIndex){
                    case 0:
                        LendingTab.Content = lendingPage.Content;
                        break;

                    case 1:
                        ExchangeTab.Content = exchangePage.Content;
                        break;

                    case 2:
                        AccountTab.Content = accountPage.Content;
                        break;
            }
        }

    }
}