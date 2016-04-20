using DallEX.io.API;
using DallEX.io.API.LendingTools;
using DallEX.io.API.MarketTools;
using DallEX.io.View.Service;
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
    /// Interaction logic for ucTradeSell.xaml
    /// </summary>
    public sealed partial class ucTradeSell : UserControl
    {
        private CurrencyPair _currencyPair;

        public ucTradeSell()
        {
            InitializeComponent();
        }

        private void lblYouHaveValue_MouseDown(object sender, MouseButtonEventArgs e)
        {
            txtAmount.Text = tbYouHaveValue.Text;
            txtPrice.Text = tbHighestBidValue.Text;
            txtTotal.Text = (double.Parse(txtAmount.Text) * double.Parse(txtPrice.Text)).ToString("0.00000000");
        }

        private void lblHighestBidValue_MouseDown(object sender, MouseButtonEventArgs e)
        {

            double amount = 0;
            if (!double.TryParse(txtAmount.Text, out amount))
            {
                txtAmount.Text = (double.Parse(txtTotal.Text) / double.Parse(txtPrice.Text)).ToString("0.00000000");
            }

            txtPrice.Text = tbHighestBidValue.Text;
            txtTotal.Text = (double.Parse(txtAmount.Text) * double.Parse(txtPrice.Text)).ToString("0.00000000");
        }

        private void btnSell_Click(object sender, RoutedEventArgs e)
        {

        }

        public void SetCurrency(CurrencyPair currencyPair)
        {
            _currencyPair = currencyPair;

            lblTitle.Content = "Sell " + currencyPair.QuoteCurrency;
            lblQuoteCoinYouHave.Content = currencyPair.QuoteCurrency;
            lblBaseCoinHighestBid.Content = currencyPair.QuoteCurrency;


            tbYouHaveValue.Text = Service.WalletService.Instance().WalletAsync.First(x => x.Key.Equals(currencyPair.QuoteCurrency)).Value.available.ToString("0.00000000");
            tbHighestBidValue.Text = Service.MarketService.Instance().MarketAsync.First(x => x.Key.Equals(currencyPair)).Value.OrderTopBuy.ToString("0.00000000");
            txtPrice.Text = double.Parse(tbHighestBidValue.Text).ToString("0.00000000");
        }

    }
}
