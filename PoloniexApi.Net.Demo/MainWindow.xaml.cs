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
    public sealed partial class MainWindow : Window, IDisposable
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
            worker.WorkerSupportsCancellation = true;

            worker.DoWork += worker_DoWork;
            updateTimer = new Timer(UpdateView, null, 0, 16000);

            lendingPage = new LendingPage();
            LendingTab = new TabItem();
            LendingTab.Header = "Lending";
            LendingTab.Background = System.Windows.Media.Brushes.Red;
            TabMain.Items.Add(LendingTab);

            exchangePage = new ExchangePage();
            ExchangeTab = new TabItem();
            ExchangeTab.Header = "Exchange";
            ExchangeTab.Background = System.Windows.Media.Brushes.Yellow;
            TabMain.Items.Add(ExchangeTab);

            accountPage = new AccountPage();
            AccountTab = new TabItem();
            AccountTab.Header = "Account";
            AccountTab.Background = System.Windows.Media.Brushes.Green;
            TabMain.Items.Add(AccountTab);

            disposedValue = false;
    }

        private void UpdateView(object state)
        {
            if (!worker.IsBusy)
                worker.RunWorkerAsync();
        }

        private void worker_DoWork(object sender, DoWorkEventArgs e)
        {
            ucHeader.LoadLoanOffersAsync(PoloniexClient.Instance(ApiKeys.PublicKey, ApiKeys.PrivateKey));
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            Dispose();
        }

        private void TabMain_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ExchangeTab.Content = null;
            AccountTab.Content = null;
            LendingTab.Content = null;
            
            switch(TabMain.SelectedIndex){
                    case 0: //Lending
                        LendingTab.Content = lendingPage.Content;
                        break;

                    case 1:  //Exchange
                        ExchangeTab.Content = exchangePage.Content;
                        break;

                    case 2: //Account
                        AccountTab.Content = accountPage.Content;
                        break;
            }
        }

        #region IDisposable Support
        private bool disposedValue = false;

        void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    if (worker != null)
                    {
                        if (worker.IsBusy)
                            worker.CancelAsync();

                        worker.Dispose();
                    }

                    worker = null;

                    if (updateTimer != null)
                        updateTimer.Dispose();

                    updateTimer = null;

                    if(lendingPage != null)
                        lendingPage.Dispose();
                    lendingPage = null;

                    if (exchangePage != null)
                        exchangePage.Dispose();
                    exchangePage = null;

                    if (accountPage != null)
                        accountPage.Dispose();
                    accountPage = null;

                    if (TabMain != null)
                        if (TabMain.Items != null)
                            TabMain.Items.Clear();

                    TabMain = null;

                    ExchangeTab.Content = null;
                    AccountTab.Content = null;
                    LendingTab.Content = null;

                    ExchangeTab = null;
                    AccountTab = null;
                    LendingTab = null;

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