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
    public partial class ChartLendingWindow : Window
    {
        private PoloniexClient PoloniexClient;

        public string Currency;

        public ChartLendingWindow(string currency)
        {
            InitializeComponent();

            // Set icon from the assembly
            Icon = System.Drawing.Icon.ExtractAssociatedIcon(Assembly.GetExecutingAssembly().Location).ToImageSource();

            PoloniexClient = PoloniexClient.Instance(ApiKeys.PublicKey, ApiKeys.PrivateKey);

            Currency = currency;

            Title = string.Concat("Lending Candlestick ", "(", currency, ")");

            LoadChart();
        }

        public void LoadChart()
        {
            try
            {
                if (PoloniexClient != null)
                {
                    var chartData = LoanContext.Instance().LendingOffers.ToList();

                    if (chartData != null)
                        Dispatcher.Invoke(DispatcherPriority.Background, (ThreadStart)delegate
                        {
                            ucCandlestick.LoadLoanGraph(Currency, chartData);
                            this.UpdateLayout();

                            dtgHistory.ItemsSource = chartData.GroupBy(x => x.dataRegistro).Distinct().Select(x => new { Time = x.Key, Rate = x.Max(m => m.rate) }).OrderByDescending(x=> x.Time);
                        });
                }
            }
            catch { }

        }
    }
}
