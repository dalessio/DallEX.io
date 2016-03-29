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

        public MainWindow()
        {
            InitializeComponent();

            // Set icon from the assembly
            this.Icon = System.Drawing.Icon.ExtractAssociatedIcon(Assembly.GetExecutingAssembly().Location).ToImageSource();

            worker = new BackgroundWorker();

            worker.DoWork += worker_DoWork;
            updateTimer = new Timer(UpdateView, null, 0, 16000);          

            var LendingWindow = new LendingWindow();
            TabItem LendingTab = new TabItem();
            LendingTab.Header = "Lending";
            LendingTab.Content = LendingWindow.Content;
            LendingTab.Background = System.Windows.Media.Brushes.Red;
            TabMain.Items.Add(LendingTab);

            var ExchangeWindow = new ExchangeWindow();
            TabItem ExchangeTab = new TabItem();
            ExchangeTab.Header = "Exchange";
            ExchangeTab.Content = ExchangeWindow.Content;
            ExchangeTab.Background = System.Windows.Media.Brushes.Yellow;
            TabMain.Items.Add(ExchangeTab);

            var AccountWindow = new AccountWindow();
            TabItem AccountTab = new TabItem();
            AccountTab.Header = "Account";
            AccountTab.Content = AccountWindow.Content;
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
            
        }

    }
}
