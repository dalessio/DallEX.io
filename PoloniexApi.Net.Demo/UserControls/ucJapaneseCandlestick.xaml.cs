using DallEX.io.API;
using DallEX.io.API.MarketTools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using ZedGraph;

namespace DallEX.io.View.UserControls
{
    /// <summary>
    /// Interaction logic for UserControl1.xaml
    /// </summary>
    public partial class ucJapaneseCandlestick : UserControl
    {
        public ucJapaneseCandlestick()
        {
            InitializeComponent();
        }
        public async Task LoadGraphAsync(CurrencyPair currencyPair, IList<IMarketChartData> chartData)
        {
            await this.Dispatcher.Invoke(async () =>
            {
                await Task.Run(() =>
                {
                    LoadGraph(currencyPair, chartData);
                });
            });
        }

        public bool IsGraphLoaded = false;
        public void LoadGraph(CurrencyPair currencyPair, IList<IMarketChartData> chartData)
        {
            GraphPane myPane = zgc1.GraphPane;

            myPane.Title.Text = "Candlestick Chart " + currencyPair.ToString();
            myPane.XAxis.Title.Text = "Date";
            myPane.YAxis.Title.Text = "Price " + currencyPair.BaseCurrency;

            StockPointList spl = new StockPointList();
            Random rand = new Random();

            foreach (var data in chartData)
            {
                var xDate = new XDate(data.Time.Year, data.Time.Month, data.Time.Day, data.Time.Hour, data.Time.Minute, data.Time.Second);
                double x = xDate;
                double close = data.Close;
                double hi = data.High;
                double low = data.Low;
                double open = data.Open;

                StockPt pt = new StockPt(x, hi, low, open, close, 100000);
                spl.Add(pt);
            }

            JapaneseCandleStickItem myCurve = myPane.AddJapaneseCandleStick("trades", spl);
            myCurve.Stick.IsAutoSize = true;
            myCurve.Stick.Color = System.Drawing.Color.Red;

            // Use DateAsOrdinal to skip weekend gaps
            myPane.XAxis.Type = AxisType.Date;

            // pretty it up a little
            myPane.Chart.Fill = new Fill(System.Drawing.Color.WhiteSmoke, System.Drawing.Color.Gray, 45.0f);
            myPane.Fill = new Fill(System.Drawing.Color.WhiteSmoke, System.Drawing.Color.DarkOrange);


            zgc1.AxisChange();

            zgc1.Refresh();
            zgc1.Update();

            this.UpdateLayout();

            IsGraphLoaded = true;
        }
    }
}
