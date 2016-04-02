using DallEX.io.API;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace DallEX.io.View.UserControls
{
    /// <summary>
    /// Interaction logic for ucTopDisplay.xaml
    /// </summary>
    public partial class ucTopDisplay : UserControl, IDisposable
    {
        private ThicknessAnimation ThickAnimation = null;
        private ThreadStart threadStart = null;
        private Thread thread = null;

        private static object _syncRoot = new object();

        public ucTopDisplay()
        {
            InitializeComponent();

            ThickAnimation = new ThicknessAnimation();
            ThickAnimation.From = new Thickness(0, 0, 0, 0);
            ThickAnimation.RepeatBehavior = RepeatBehavior.Forever;
            ThickAnimation.FillBehavior = FillBehavior.Stop;
            ThickAnimation.Duration = new Duration(TimeSpan.FromSeconds(6));

            if (threadStart == null)
                threadStart = new ThreadStart(LeftToRightMarqueeOnTextBox);

            disposedValue = false;

        }

        public async void LoadLoanOffersAsync(PoloniexClient PoloniexClient)
        {
            try
            {
                var lendings = await PoloniexClient.Lendings.GetLoanOffersAsync("BTC");
                var firstLoanOffer = lendings.offers.OrderBy(x => x.rate).First();

                var markets = await PoloniexClient.Markets.GetSummaryAsync();
                double ethPriceLast = markets.Where(x => x.Key.ToString().ToUpper().Equals("BTC_ETH")).OrderBy(x => x.Value.PriceLast).First().Value.PriceLast;
                double btcPriceLast = markets.Where(x => x.Key.ToString().ToUpper().Equals("USDT_BTC")).OrderBy(x => x.Value.PriceLast).First().Value.PriceLast;

                firstLoanOffer.ethExchangeValue = ethPriceLast;
                firstLoanOffer.btcExchangeValue = btcPriceLast;


                string eth = string.Concat("BTC/ETH: ", firstLoanOffer.ethExchangeValue.ToString("0.00000000"));
                string btc = string.Concat("USDT/BTC: ", firstLoanOffer.btcExchangeValue.ToString("0.00000000"));
                string loan = string.Concat("BTC Loan Rate: ", firstLoanOffer.rate.ToString("0.00000%"));

                txtDisplay.Dispatcher.Invoke(DispatcherPriority.Render, (ThreadStart)delegate
                {
                    txtDisplay.Text = string.Concat(btc, "          ", eth, "          ", loan);
                });

                LeftToRightMarqueeOnTextBoxThread();

                lendings = null;
                firstLoanOffer = null;
                markets = null;
            }
            catch (Exception ex)
            {
                //todo: log
            }
            finally
            {
            }
        }

        private void LeftToRightMarqueeOnTextBoxThread()
        {
            try
            {
                if (thread == null ||
                    (thread.ThreadState != System.Threading.ThreadState.Running &&
                    thread.ThreadState != System.Threading.ThreadState.WaitSleepJoin)
                )
                {
                    lock (_syncRoot)
                    {
                        thread = new Thread(threadStart);
                        thread.Priority = ThreadPriority.AboveNormal;
                        thread.IsBackground = true;
                        thread.Start();
                    }
                }

            }
            catch (Exception ex)
            { }

        }


        public void LeftToRightMarqueeOnTextBox()
        {
            txtDisplay.Dispatcher.Invoke(DispatcherPriority.Render, (ThreadStart)delegate
            {

                string Copy = string.Concat(" ", " ", " ", " ", " ", " ", " ", " ", " ", " ", " ", " ", " ", " ", " ", " ", " ", " ", " ", " ", " ", " ", " ", " ", " ", " ", " ", " ", " ", " ", " ", " ", " ", " ", " ", " ", " ", " ", " ", " ", " ", " ", " ", " ", " ", " ", " ", " ", " ", " ", " ", " ", " ", " ", " ", " ", " ", " ", " ", " ", " ", " ", " ", " ", " ", " ", " ", " ", " ", " ", " ", " ", " ", " ", " ", " ", " ", " ", " ", " ", " ", " ", " ", " ", " ", " ", " ", " ", " ", " ", " ", " ", " ", " ", " ", " ", " ", " ", " ", " ", " ", " ", " ", " ", " ", " ", " ", " ", " ", " ", " ", " ", " ", " ", " ", " ", " ", " ", " ", " ",
                    txtDisplay.Text);

                double TextGraphicalWidth = new FormattedText(Copy, System.Globalization.CultureInfo.CurrentCulture, System.Windows.FlowDirection.LeftToRight, new Typeface(txtDisplay.FontFamily.Source), txtDisplay.FontSize, txtDisplay.Foreground).WidthIncludingTrailingWhitespace;
                double TextLenghtGraphicalWidth = 0;

                while (TextLenghtGraphicalWidth < txtDisplay.ActualWidth)
                {
                    txtDisplay.Text += Copy;
                    TextLenghtGraphicalWidth = new FormattedText(txtDisplay.Text, System.Globalization.CultureInfo.CurrentCulture, System.Windows.FlowDirection.LeftToRight, new Typeface(txtDisplay.FontFamily.Source), txtDisplay.FontSize, txtDisplay.Foreground).WidthIncludingTrailingWhitespace;
                }
                txtDisplay.Text += " " + txtDisplay.Text;
                ThickAnimation.To = new Thickness(-TextGraphicalWidth, 0, 0, 0);
                txtDisplay.BeginAnimation(TextBox.PaddingProperty, ThickAnimation);
            });
        }

        #region IDisposable Support
        private bool disposedValue = false;

        void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    if (thread != null)
                        thread.Abort();

                    ThickAnimation = null;
                    threadStart = null;
                    thread = null;
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
