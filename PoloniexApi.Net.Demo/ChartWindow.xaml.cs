using DallEX.io.API;
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
    /// Interaction logic for ChartWindow.xaml
    /// </summary>
    public partial class ChartWindow : Window
    {
        private PoloniexClient PoloniexClient;

        public CurrencyPair CurrencyPair;

        private static int selectedIndex = 0;

        public int Minutos = 20;

        public ChartWindow(CurrencyPair currencyPair)
        {
            InitializeComponent();

            // Set icon from the assembly
            Icon = System.Drawing.Icon.ExtractAssociatedIcon(Assembly.GetExecutingAssembly().Location).ToImageSource();

            PoloniexClient = PoloniexClient.Instance(ApiKeys.PublicKey, ApiKeys.PrivateKey);

            CurrencyPair = currencyPair;

            Title = string.Concat("History", "(", CurrencyPair.ToString(), ")");

            LoadChart();

        }

        private void tabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            selectedIndex = tabControl.SelectedIndex;
            LoadChart();
        }

        private void ClearGrids()
        {
            dtgTradeHistory.Items.Clear();
        }


        public async void LoadChart()
        {
            try
            {
                if (PoloniexClient != null)
                {
                    var chartData = await PoloniexClient.Markets.GetChartDataAsync(CurrencyPair, MarketPeriod.Day);
                        if (chartData != null)
                            switch (selectedIndex)
                            {
                                case 1: //History
                                        chartData = null;
                                    break;

                                default:  //Chart  
                                        Dispatcher.Invoke(DispatcherPriority.Background, (ThreadStart)delegate
                                        {
                                            if (!ucCandlestick.IsGraphLoaded)
                                                ucCandlestick.LoadGraph(CurrencyPair, chartData);

                                            this.UpdateLayout();
                                        });
                                    break;
                            }

                }
            }
            catch { }

        }
    }
}
