using DallEX.io.API;
using DallEX.io.API.MarketTools;
using DallEX.io.View.Service;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Windows.Media;
using System.Reflection;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Threading;
using System.Windows.Input;
using DallEX.io.View.Library;
using Hardcodet.Wpf.TaskbarNotification;

namespace DallEX.io.View
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public sealed partial class MainWindow : Window, IDisposable
    {
        private PoloniexClient PoloniexClient;

        private SemaphoreSlim semaphoreSlim;

        FachadaWSSGS.FachadaWSSGSClient FachadaWSSGS;

        private Timer updateTimer;

        private LendingPage lendingPage;
        private ExchangePage exchangeBTCPage;
        private ExchangePage exchangeETHPage;
        private ExchangePage exchangeXMRPage;
        private ExchangePage exchangeUSDTPage;
        private BotTrader botTrader;
        private AccountPage accountPage;

        private TabItem exchangeBTCTab;
        private TabItem exchangeETHTab;
        private TabItem exchangeXMRTab;
        private TabItem exchangeUSDTTab;
        private TabItem accountTab;
        private TabItem lendingTab;
        private TabItem botTab;

        private int updateTimeMiliseconds = 5000;

        public MainWindow()
        {
            InitializeComponent();

            BuildTabs();

            txtTrollbox.Document.Background = Brushes.Black;
            txtTrollbox.BorderThickness = new Thickness(0);
            txtTrollbox.Margin = new Thickness(2);

            // Set icon from the assembly
            this.Icon = System.Drawing.Icon.ExtractAssociatedIcon(Assembly.GetExecutingAssembly().Location).ToImageSource();

            if (!int.TryParse(ConfigurationManager.AppSettings.Get("headerUpdateTimeMiliseconds"), out updateTimeMiliseconds))
                MessageBox.Show("O parametro do App.Config lendingUpdateTimeMiliseconds está setado com valor inválido, foi aplicado o valor padrão (" + updateTimeMiliseconds + ")!");

            PoloniexClient = PoloniexClient.Instance(ApiKeys.PublicKey, ApiKeys.PrivateKey);
            PoloniexClient.Live.OnTrollboxMessage += Live_OnTrollboxMessage;

            FachadaWSSGS = Singleton<FachadaWSSGS.FachadaWSSGSClient>.Instance;

            LiveStart();

            semaphoreSlim = new SemaphoreSlim(1);

            updateTimer = new Timer(UpdateView, null, 0, updateTimeMiliseconds);           

            disposedValue = false;

            myNotifyIcon.Icon = System.Drawing.Icon.ExtractAssociatedIcon(Assembly.GetExecutingAssembly().Location);
            myNotifyIcon.ToolTipText = this.Title;
            myNotifyIcon.MenuActivation = Hardcodet.Wpf.TaskbarNotification.PopupActivationMode.LeftClick;
            myNotifyIcon.PopupActivation = Hardcodet.Wpf.TaskbarNotification.PopupActivationMode.DoubleClick;

            txtTitleNotify.Text = this.Title;

            var contextMenu = new ContextMenu();
            myNotifyIcon.ContextMenu = contextMenu;

            myNotifyIcon.ShowBalloonTip(this.Title, "DallEX is started!", BalloonIcon.Info);           
        }

        public void SendBaloonMessage(string message, BalloonIcon icon)
        {
            myNotifyIcon.ShowBalloonTip(message, message, icon);
        }

        private void BuildTabs()
        {
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
            exchangeETHPage = new ExchangePage("ETH");
            exchangeETHTab = new TabItem();
            exchangeETHTab.Header = "Exchange ETH";
            exchangeETHTab.Background = System.Windows.Media.Brushes.Yellow;
            TabMain.Items.Add(exchangeETHTab);

            //3
            exchangeXMRPage = new ExchangePage("XMR");
            exchangeXMRTab = new TabItem();
            exchangeXMRTab.Header = "Exchange XMR";
            exchangeXMRTab.Background = System.Windows.Media.Brushes.IndianRed;
            TabMain.Items.Add(exchangeXMRTab);

            //4
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

            //4
            botTrader = new BotTrader();
            botTab = new TabItem();
            botTab.Header = "Bot Trader";
            botTab.Background = System.Windows.Media.Brushes.Salmon;
            TabMain.Items.Add(botTab);

        }

        private async void LiveStart()
        {
            PoloniexClient.Live.Start();
            await PoloniexClient.Live.SubscribeToTrollboxAsync();          
        }

        private void Live_OnTrollboxMessage(object sender, TrollboxMessageEventArgs e)
        {
            txtTrollbox.Dispatcher.Invoke(DispatcherPriority.Background, (ThreadStart)delegate
            {
                var paragraph = txtTrollbox.Document.Blocks.FirstBlock as Paragraph;
                paragraph.Margin = new Thickness(0,10,0,0);

                paragraph.Inlines.Add(new Bold(new Run(e.SenderName + ": "))
                {
                    Foreground = Brushes.Orange,
                    FontSize = 10
                    
                });

                paragraph.Inlines.Add(new Run(e.MessageText + ": ")
                {
                    Foreground = Brushes.LightGray,
                    FontSize = 11
                });

                paragraph.Inlines.Add(new LineBreak());

                paragraph.Inlines.Add(new Run(" "){ FontSize = 5 });
                paragraph.Inlines.Add(new LineBreak());

                txtTrollbox.ScrollToEnd();
            });              
        }

        private void TabMain_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            exchangeBTCTab.Content = null;
            exchangeETHTab.Content = null;
            exchangeXMRTab.Content = null;
            exchangeUSDTTab.Content = null;
            accountTab.Content = null;
            lendingTab.Content = null;
            botTab.Content = null;

            exchangeBTCPage.IsEnabled = false;
            exchangeETHPage.IsEnabled = false;
            exchangeXMRPage.IsEnabled = false;
            exchangeUSDTPage.IsEnabled = false;
            lendingPage.IsEnabled = false;
            accountPage.IsEnabled = false;
            botTrader.IsEnabled = false;

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

                case 2:  //Exchange ETH
                    exchangeETHPage.IsEnabled = true;
                    exchangeETHTab.Content = exchangeETHPage.Content;
                    break;

                case 3:  //Exchange XMR
                    exchangeXMRPage.IsEnabled = true;
                    exchangeXMRTab.Content = exchangeXMRPage.Content;
                    break;

                case 4:  //Exchange USDT
                    exchangeUSDTPage.IsEnabled = true;
                    exchangeUSDTTab.Content = exchangeUSDTPage.Content;
                    break;

                case 5: //Account
                    accountPage.IsEnabled = true;
                    accountTab.Content = accountPage.Content;
                    break;

                case 6: //Bot
                    botTrader.IsEnabled = true;
                    botTab.Content = botTrader.Content;
                    break;
            }
        }

        private async void UpdateView(object state)
        {
            if(PoloniexClient != null)
                if (semaphoreSlim != null)
                {
                    await semaphoreSlim.WaitAsync();

                    try
                    {
                        MarketService.Instance().MarketList = await PoloniexClient.Markets.GetSummaryAsync();
                        WalletService.Instance().WalletList = await PoloniexClient.Wallet.GetBalancesAsync();

                        try
                        {
                            FachadaWSSGSService.Instance().getUltimoValorVOResponse = await FachadaWSSGS.getUltimoValorVOAsync(10813);
                        }
                        catch
                        {
                        }

                        await ucHeader.LoadLoanOffersAsync(PoloniexClient);
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
                btnChat.ToolTip = "Open Trollbox";
                PoloniexClient.Live.Stop();
            }
            else {
                chatColumn.Width = new GridLength(200);
                btnChat.Content = ">>";
                btnChat.ToolTip = "Close Trollbox";
                LiveStart();
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
                        if (PoloniexClient != null)
                            if (PoloniexClient.Live != null)
                                PoloniexClient.Live.Stop();
                                               
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


                        if (FachadaWSSGS != null)
                            FachadaWSSGS.Close();

                    }
                }
            }
            finally
            {
                MarketService.Instance().MarketList = null;
                WalletService.Instance().WalletList = null;
                TabMain = null;

                exchangeBTCTab = null;
                exchangeXMRTab = null;
                exchangeUSDTTab = null;
                accountTab = null;
                lendingTab = null;

                FachadaWSSGS = null;

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