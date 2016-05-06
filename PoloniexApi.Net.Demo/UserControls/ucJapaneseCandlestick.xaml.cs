using DallEX.io.API;
using DallEX.io.API.LendingTools;
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
        public bool IsGraphLoaded = false;

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

                StockPt pt = new StockPt(x, hi, low, open, close, data.VolumeBase);
                spl.Add(pt);
            }

            JapaneseCandleStickItem myCurve = myPane.AddJapaneseCandleStick("Price", spl);
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
            zgc1.IsShowCursorValues = true;
            this.UpdateLayout();

            IsGraphLoaded = true;
        }

        public void LoadLoanGraph(string currency, IList<LendingOffer> chartData)
        {
            GraphPane myPane = zgc1.GraphPane;

            // Set the title and axis labels
            myPane.Title.Text = "Candlestick Chart " + currency;
            myPane.XAxis.Title.Text = "Date";
            myPane.YAxis.Title.Text = "Rate";

            // Generate some sine-based data values
            PointPairList list = new PointPairList();
            foreach (var data in chartData.GroupBy(x => x.dataRegistro).Distinct().Select(x => new {Time = x.Key, Rate = x.Max(m => m.rate)  }).OrderBy(x => x.Time))
            {
                var xDate = new XDate(data.Time.Year, data.Time.Month, data.Time.Day, data.Time.Hour, data.Time.Minute, data.Time.Second);
                double x = (double)xDate;
                double y = data.Rate;
                list.Add(x, y);
            }

            //GraphDemo1(myPane, list);

            GraphDemo2(myPane, list);

            // Set the XAxis to date type
            myPane.XAxis.Type = AxisType.Date;

            zgc1.AxisChange();

            zgc1.Refresh();
            zgc1.Update();

            zgc1.IsShowCursorValues = true;

            UpdateLayout();

            IsGraphLoaded = true;
        }
        private static void GraphDemo2(GraphPane myPane, PointPairList list)
        {
            // Add a red curve with circle symbols
            LineItem curve = myPane.AddCurve("Rate", list, System.Drawing.Color.Red, SymbolType.Circle);
            curve.Line.Width = 1.5F;
            // Fill the area under the curve
            curve.Line.Fill = new Fill(System.Drawing.Color.White, System.Drawing.Color.FromArgb(60, 190, 50), 90F);
            // Fill the symbols with white to make them opaque
            curve.Symbol.Fill = new Fill(System.Drawing.Color.White);
            curve.Symbol.Size = 5;

            // Set the curve type to forward steps
            curve.Line.StepType = StepType.ForwardStep;
        }
        private static void GraphDemo1(GraphPane myPane, PointPairList list)
        {
            // Generate a red curve with diamond
            // symbols, and "My Curve" in the legend
            LineItem myCurve = myPane.AddCurve("Rate",
                list, System.Drawing.Color.Red, SymbolType.Diamond);
        }
    }
}
