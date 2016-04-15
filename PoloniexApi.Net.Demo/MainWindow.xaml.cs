using DallEX.io.API;
using DallEX.io.View.Service;
using System;
using System.ComponentModel;
using System.Configuration;
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
        private PoloniexClient PoloniexClient;

        private SemaphoreSlim semaphoreSlim;

        private Timer updateTimer;

        private LendingPage lendingPage;
        private ExchangePage exchangeBTCPage;
        private ExchangePage exchangeXMRPage;
        private ExchangePage exchangeUSDTPage;
        private AccountPage accountPage;

        private TabItem exchangeBTCTab;
        private TabItem exchangeXMRTab;
        private TabItem exchangeUSDTTab;
        private TabItem accountTab;
        private TabItem lendingTab;

        private int updateTimeMiliseconds = 5000;

        public MainWindow()
        {
            InitializeComponent();

            if (!int.TryParse(ConfigurationManager.AppSettings.Get("headerUpdateTimeMiliseconds"), out updateTimeMiliseconds))
                MessageBox.Show("O parametro do App.Config lendingUpdateTimeMiliseconds está setado com valor inválido, foi aplicado o valor padrão (" + updateTimeMiliseconds + ")!");

            // Set icon from the assembly
            this.Icon = System.Drawing.Icon.ExtractAssociatedIcon(Assembly.GetExecutingAssembly().Location).ToImageSource();

            PoloniexClient = PoloniexClient.Instance(ApiKeys.PublicKey, ApiKeys.PrivateKey);

            semaphoreSlim = new SemaphoreSlim(1);

            updateTimer = new Timer(UpdateView, null, 0, updateTimeMiliseconds);

            //0
            lendingPage = new LendingPage();
            lendingTab = new TabItem();
            lendingTab.Header = "Lending";
            lendingTab.Background = System.Windows.Media.Brushes.SteelBlue;
            TabMain.Items.Add(lendingTab);

            //1
            exchangeBTCPage = new ExchangePage("BTC");
            exchangeBTCTab = new TabItem();
            exchangeBTCTab.Header = "Exchange BTC";
            exchangeBTCTab.Background = System.Windows.Media.Brushes.LightSteelBlue;
            TabMain.Items.Add(exchangeBTCTab);

            //2
            exchangeXMRPage = new ExchangePage("XMR");
            exchangeXMRTab = new TabItem();
            exchangeXMRTab.Header = "Exchange XMR";
            exchangeXMRTab.Background = System.Windows.Media.Brushes.IndianRed;
            TabMain.Items.Add(exchangeXMRTab);

            //3
            exchangeUSDTPage = new ExchangePage("USDT");
            exchangeUSDTTab = new TabItem();
            exchangeUSDTTab.Header = "Exchange USDT";
            exchangeUSDTTab.Background = System.Windows.Media.Brushes.LightGoldenrodYellow;
            TabMain.Items.Add(exchangeUSDTTab);

            //4
            accountPage = new AccountPage();
            accountTab = new TabItem();
            accountTab.Header = "Account";
            accountTab.Background = System.Windows.Media.Brushes.GreenYellow;
            TabMain.Items.Add(accountTab);

            disposedValue = false;
        }

        private void TabMain_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            exchangeBTCTab.Content = null;
            exchangeXMRTab.Content = null;
            exchangeUSDTTab.Content = null;
            accountTab.Content = null;
            lendingTab.Content = null;

            exchangeBTCPage.IsEnabled = false;
            exchangeXMRPage.IsEnabled = false;
            exchangeUSDTPage.IsEnabled = false;
            lendingPage.IsEnabled = false;
            accountPage.IsEnabled = false;

            switch (TabMain.SelectedIndex)
            {
                case 0: //Lending
                    lendingPage.IsEnabled = true;
                    lendingTab.Content = lendingPage.Content;
                    break;

                case 1:  //Exchange BTC
                    exchangeBTCPage.IsEnabled = true;
                    exchangeBTCTab.Content = exchangeBTCPage.Content;
                    break;

                case 2:  //Exchange XMR
                    exchangeXMRPage.IsEnabled = true;
                    exchangeXMRTab.Content = exchangeXMRPage.Content;
                    break;

                case 3:  //Exchange USDT
                    exchangeUSDTPage.IsEnabled = true;
                    exchangeUSDTTab.Content = exchangeUSDTPage.Content;
                    break;

                case 4: //Account
                    accountPage.IsEnabled = true;
                    accountTab.Content = accountPage.Content;
                    break;
            }
        }

        private async void UpdateView(object state)
        {
            if (semaphoreSlim != null)
            {
                await semaphoreSlim.WaitAsync();
                try
                {
                    MarketService.Instance().MarketAsync = await PoloniexClient.Markets.GetSummaryAsync();
                    await ucHeader.LoadLoanOffersAsync(PoloniexClient.Instance(ApiKeys.PublicKey, ApiKeys.PrivateKey));
                    WalletService.Instance().WalletAsync = await PoloniexClient.Wallet.GetBalancesAsync();
                }
                finally
                {
                    if (semaphoreSlim != null)
                        semaphoreSlim.Release();
                }
            }

        }

        private void button_Click(object sender, RoutedEventArgs e)
        {
            if (!chatColumn.Width.Equals(new GridLength(0)))
            {
                chatColumn.Width = new GridLength(0);
                btnChat.Content = "<<";
                Grid.SetColumn(btnChat, 0);
            }
            else {
                chatColumn.Width = new GridLength(200);
                btnChat.Content = ">>";
                Grid.SetColumn(btnChat, 1);
            }
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            Dispose();
        }

        #region IDisposable Support
        private bool disposedValue = false;

        void Dispose(bool disposing)
        {
            try
            {
                if (!disposedValue)
                {
                    if (disposing)
                    {
                        if (updateTimer != null)
                            updateTimer.Dispose();

                        updateTimer = null;

                        if (semaphoreSlim != null)
                            semaphoreSlim.Dispose();
                        semaphoreSlim = null;


                        if (lendingPage != null)
                            lendingPage.Dispose();
                        lendingPage = null;

                        if (exchangeBTCPage != null)
                            exchangeBTCPage.Dispose();
                        exchangeBTCPage = null;


                        if (exchangeXMRPage != null)
                            exchangeXMRPage.Dispose();
                        exchangeXMRPage = null;


                        if (exchangeUSDTPage != null)
                            exchangeUSDTPage.Dispose();
                        exchangeUSDTPage = null;

                        if (accountPage != null)
                            accountPage.Dispose();
                        accountPage = null;

                        if (TabMain != null)
                            if (TabMain.Items != null)
                                TabMain.Items.Clear();

                        LoanContext.Instance().Dispose();

                    }                    
                }
            }
            finally
            {
                MarketService.Instance().MarketAsync = null;
                WalletService.Instance().WalletAsync = null;
                TabMain = null;

                exchangeBTCTab = null;
                exchangeXMRTab = null;
                exchangeUSDTTab = null;
                accountTab = null;
                lendingTab = null;

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