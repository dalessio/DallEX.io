using DallEX.io.API;
using DallEX.io.API.MarketTools;
using DallEX.io.API.WalletTools;
using DallEX.io.View.Library;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.ComponentModel;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Threading;
using System.Configuration;

namespace DallEX.io.View
{

    /// <summary>
    /// Interaction logic for TradeHistory.xaml
    /// </summary>
    public partial class TradeHistory : Window, IDisposable
    {
        private PoloniexClient PoloniexClient;
        private CurrencyPair CurrencyPair;

        private Timer updateTimer;

        private int updateTimeMiliseconds = 3000;

        private static int selectedIndex;


        public TradeHistory(CurrencyPair currencyPair)
        {
            InitializeComponent();

            if (!int.TryParse(ConfigurationManager.AppSettings.Get("exchangeUpdateTimeMiliseconds"), out updateTimeMiliseconds))
                MessageBox.Show("O parametro do App.Config lendingUpdateTimeMiliseconds está setado com valor inválido, foi aplicado o valor padrão (" + updateTimeMiliseconds + ")!");

            PoloniexClient = PoloniexClient.Instance(ApiKeys.PublicKey, ApiKeys.PrivateKey);

            updateTimer = new Timer(UpdateGrid, null, 0, updateTimeMiliseconds);

            CurrencyPair = currencyPair;
        }

        private void tabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            selectedIndex = tabControl.SelectedIndex;
        }

        private void ClearGrids()
        {
            dtgYourTradeHistory.Items.Clear();
            dtgTradeHistory.Items.Clear();
        }

        private async void UpdateGrid(object state)
        {
            await LoadMarketSummaryAsync();
        }

        private async Task LoadMarketSummaryAsync()
        {
            await Task.Run(async () =>
            {
                try
                {
                    if (PoloniexClient != null)
                    {
                        await Dispatcher.Invoke(async() =>
                        { 
                        switch (selectedIndex)
                        {
                            case 0: //All
                                var TradeHistory = await PoloniexClient.Markets.GetTradesAsync(CurrencyPair);
                                    if (TradeHistory != null)
                                    {
                                        ClearGrids();
                                        foreach (var trade in TradeHistory)
                                            dtgTradeHistory.Items.Add(trade);

                                        TradeHistory.Clear();
                                    }
                                TradeHistory = null;
                                break;

                            case 1:  //Your  
                                var YourTradeHistory = await PoloniexClient.Wallet.GetTradesHistoryAsync(CurrencyPair);
                                    if (YourTradeHistory != null)
                                    {
                                        ClearGrids();
                                        foreach (var trade in YourTradeHistory)
                                            dtgYourTradeHistory.Items.Add(trade);

                                        YourTradeHistory.Clear();                                      
                                    }
                                YourTradeHistory = null;
                                break;
                            }
                        });
                    }
                }
                finally
                {

                }
            });
        }

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    if (updateTimer != null)
                        updateTimer.Dispose();

                    updateTimer = null;
                    
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
