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
    /// Interaction logic for ucTopDisplay.xaml
    /// </summary>
    public sealed partial class ucTradeBuy : UserControl
    {
        private CurrencyPair _currencyPair;

        public ucTradeBuy()
        {
            InitializeComponent();
        }

        private void lblYouHaveValue_MouseDown(object sender, MouseButtonEventArgs e)
        {
            txtTotal.Text = tbYouHaveValue.Text;
            txtAmount.Text = (double.Parse(txtTotal.Text) / double.Parse(txtPrice.Text)).ToString("0.00000000");
        }

        private void lblLowestAskValue_MouseDown(object sender, MouseButtonEventArgs e)
        {
            double amount = 0;
            if (!double.TryParse(txtAmount.Text, out amount))
            {
                txtAmount.Text = (double.Parse(txtTotal.Text) / double.Parse(txtPrice.Text)).ToString("0.00000000");
            }

            txtPrice.Text = tbYouLowestAskValue.Text;
            txtTotal.Text = (double.Parse(txtAmount.Text) * double.Parse(txtPrice.Text)).ToString("0.00000000");
        }

        private void btnBuy_Click(object sender, RoutedEventArgs e)
        {

        }

        public void SetCurrency(CurrencyPair currencyPair)
        {
            _currencyPair = currencyPair;

            lblTitle.Content = "Buy " + currencyPair.QuoteCurrency;
            lblBaseCoinYouHave.Content = currencyPair.BaseCurrency;
            lblBaseCoinLowestAskValue.Content = currencyPair.BaseCurrency;

            tbYouHaveValue.Text = Service.WalletService.Instance().WalletAsync.First(x => x.Key.Equals(currencyPair.BaseCurrency)).Value.available.ToString("0.00000000"); ;
            tbYouLowestAskValue.Text = Service.MarketService.Instance().MarketAsync.First(x => x.Key.Equals(currencyPair)).Value.OrderTopSell.ToString("0.00000000");
            txtPrice.Text = double.Parse(tbYouLowestAskValue.Text).ToString("0.00000000");
        }
    }
}
