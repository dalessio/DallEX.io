﻿using DallEX.io.API;
using DallEX.io.API.MarketTools;
using DallEX.io.API.WalletTools;
using DallEX.io.View.Library;
using DallEX.io.View.Service;
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
using System.Reflection;

namespace DallEX.io.View
{

    /// <summary>
    /// Interaction logic for TradeHistory.xaml
    /// </summary>
    public partial class TradeHistory : Window
    {
        private PoloniexClient PoloniexClient;

        public CurrencyPair CurrencyPair;

        private static int selectedIndex;

        public int Minutos = 20;

        int controlOpenOrdersUpdate = 0;

        public TradeHistory(CurrencyPair currencyPair, int _selectedIndex = 0)
        {
            InitializeComponent();

            // Set icon from the assembly
            this.Icon = System.Drawing.Icon.ExtractAssociatedIcon(Assembly.GetExecutingAssembly().Location).ToImageSource();

            PoloniexClient = PoloniexClient.Instance(ApiKeys.PublicKey, ApiKeys.PrivateKey);

            CurrencyPair = currencyPair;

            Title = string.Concat("Trade History", "(", CurrencyPair.ToString(), ")");
            tabControl.SelectedIndex = _selectedIndex;
        }

        private void tabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            selectedIndex = tabControl.SelectedIndex;
        }

        private void ClearGrids()
        {
            dtgYourTradeHistory.Items.Clear();
            dtgTradeHistory.Items.Clear();
            dtgOpenOrders.Items.Clear();
        }


        public async Task LoadTradeSummaryAsync()
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
                                    if (MarketService.Instance().TradesHistoryList != null)
                                    {
                                        ClearGrids();
                                        foreach (var trade in MarketService.Instance().TradesHistoryList)
                                            dtgTradeHistory.Items.Add(trade);
                                    }
                                break;

                            case 1:  //Your  
                                var YourTradeHistory = await PoloniexClient.Wallet.GetTradesHistoryAsync(CurrencyPair, DateTime.Now.AddYears(-1), DateTime.Now.AddHours(12));
                                    if (YourTradeHistory != null)
                                    {
                                        ClearGrids();
                                        foreach (var trade in YourTradeHistory)
                                            dtgYourTradeHistory.Items.Add(trade);

                                        YourTradeHistory.Clear();                                      
                                    }
                                YourTradeHistory = null;
                                break;
                            case 2:  //Your Open Orders

                                    if (controlOpenOrdersUpdate.Equals(0))
                                    {
                                        controlOpenOrdersUpdate = 3;

                                        var openOrders = await PoloniexClient.Trading.GetOpenOrdersAsync(CurrencyPair);
                                        if (openOrders != null)
                                        {
                                            ClearGrids();

                                            foreach (var trade in openOrders)
                                                dtgOpenOrders.Items.Add(trade);

                                            openOrders.Clear();
                                        }
                                        openOrders = null;
                                    }
                                    else
                                        controlOpenOrdersUpdate--;

                                break;

                            }
                        });
                    }
                }
                catch{}
            });
        }

        private async void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            ulong idOrder = ulong.Parse(((Button)sender).Tag.ToString());

            if (await PoloniexClient.Trading.DeleteOrderAsync(CurrencyPair, idOrder))
                MessageBox.Show("Canceled");
            else
                MessageBox.Show("Error");
        }
    }
}
