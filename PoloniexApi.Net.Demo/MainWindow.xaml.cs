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

        private LendingWindow lendingWindow;
        private ExchangeWindow exchangeWindow;
        private AccountWindow accountWindow;

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

            lendingWindow = new LendingWindow();
            LendingTab = new TabItem();
            LendingTab.Loaded += LendingTab_Initialized;
            LendingTab.Unloaded += LendingTab_Unloaded;
            LendingTab.Header = "Lending";
            LendingTab.Background = System.Windows.Media.Brushes.Red;
            TabMain.Items.Add(LendingTab);

            exchangeWindow = new ExchangeWindow();
            ExchangeTab = new TabItem();
            ExchangeTab.Loaded += ExchangeTab_Initialized;
            ExchangeTab.Unloaded += ExchangeTab_Unloaded;
            ExchangeTab.Header = "Exchange";
            ExchangeTab.Background = System.Windows.Media.Brushes.Yellow;
            TabMain.Items.Add(ExchangeTab);

            accountWindow = new AccountWindow();
            AccountTab = new TabItem();
            AccountTab.Loaded += AccountTab_Initialized;
            AccountTab.Unloaded += AccountTab_Unloaded;
            AccountTab.Header = "Account";
            AccountTab.Background = System.Windows.Media.Brushes.Green;
            TabMain.Items.Add(AccountTab);
        }

        void AccountTab_Unloaded(object sender, RoutedEventArgs e)
        {
            if (AccountTab != null)
                AccountTab.Content = null;

            if (accountWindow != null)
                accountWindow.Close();
        }

        void AccountTab_Initialized(object sender, EventArgs e)
        {
            AccountTab.Content = accountWindow.Content;
        }

        void LendingTab_Unloaded(object sender, RoutedEventArgs e)
        {
            if (LendingTab != null)
                LendingTab.Content = null;

            if (lendingWindow != null)
                lendingWindow.Close();

        }

        void LendingTab_Initialized(object sender, EventArgs e)
        {
            LendingTab.Content = lendingWindow.Content;
        }

        void ExchangeTab_Unloaded(object sender, RoutedEventArgs e)
        {
            if (ExchangeTab != null)
            ExchangeTab.Content = null;

            if (exchangeWindow != null)
                exchangeWindow.Close(); 
        }

        void ExchangeTab_Initialized(object sender, EventArgs e)
        {
            ExchangeTab.Content = exchangeWindow.Content;
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

            if (lendingWindow.IsLoaded)
                lendingWindow.Close();
            lendingWindow = null;

            if (exchangeWindow.IsLoaded)
                exchangeWindow.Close();
            exchangeWindow = null;

            if (accountWindow.IsLoaded)
                accountWindow.Close();
            accountWindow = null;

            TabMain.Items.Clear();
            TabMain = null;

            ExchangeTab = null;
            AccountTab = null;
            LendingTab = null;
        }

    }
}
