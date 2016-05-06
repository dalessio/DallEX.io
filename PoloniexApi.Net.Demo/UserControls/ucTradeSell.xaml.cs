﻿using DallEX.io.API;
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

        #region Calc
        private void lblYouHaveValue_MouseDown(object sender, MouseButtonEventArgs e)
        {
            txtAmount.Text = tbYouHaveValue.Text;
            CalcTotal();
        }

        private void lblHighestBidValue_MouseDown(object sender, MouseButtonEventArgs e)
        {
            txtPrice.Text = tbHighestBidValue.Text;
            CalcTotal();
        }

        private void txtPrice_KeyUp(object sender, KeyEventArgs e)
        {
            CalcTotal();
        }

        private void txtAmount_KeyUp(object sender, KeyEventArgs e)
        {
            CalcTotal();
        }

        private void txtTotal_KeyUp(object sender, KeyEventArgs e)
        {
            CalcAmount();
        }

        private void CalcAmount()
        {
            double total = 0;
            double price = 0;

            txtAmount.Text = string.Empty;

            if (double.TryParse(txtTotal.Text, out total))
                if (double.TryParse(txtPrice.Text, out price))
                    txtAmount.Text = (total / price).ToString("0.00000000");

        }

        private void CalcTotal()
        {
            double amount = 0;
            double price = 0;

            txtTotal.Text = string.Empty;

            if (double.TryParse(txtAmount.Text, out amount))
                if (double.TryParse(txtPrice.Text, out price))
                    txtTotal.Text = (amount * price).ToString("0.00000000");

        }
        #endregion
        private async void btnSell_Click(object sender, RoutedEventArgs e)
        {
            double amount = 0;
            double price = 0;

            if (double.TryParse(txtAmount.Text, out amount))
                if (double.TryParse(txtPrice.Text, out price))
                {
                    await PoloniexClient.Instance(ApiKeys.PublicKey, ApiKeys.PrivateKey).Trading.PostOrderAsync(_currencyPair, OrderType.Sell, price, amount);
                    Fillvalues(_currencyPair);
                }
        }

        public void SetCurrency(CurrencyPair currencyPair)
        {
            _currencyPair = currencyPair;

            lblTitle.Content = "Sell " + currencyPair.QuoteCurrency;
            lblQuoteCoinYouHave.Content = currencyPair.QuoteCurrency;
            lblBaseCoinHighestBid.Content = currencyPair.QuoteCurrency;

            Fillvalues(_currencyPair);
            txtPrice.Text = double.Parse(tbHighestBidValue.Text).ToString("0.00000000");
        }

        public void Fillvalues(CurrencyPair currencyPair)
        {
            this.Dispatcher.Invoke(DispatcherPriority.Render, (ThreadStart)delegate
            {
                tbYouHaveValue.Text = Service.WalletService.Instance().WalletAsync.First(x => x.Key.Equals(currencyPair.QuoteCurrency)).Value.available.ToString("0.00000000");
                tbHighestBidValue.Text = Service.MarketService.Instance().MarketAsync.First(x => x.Key.Equals(currencyPair)).Value.OrderTopBuy.ToString("0.00000000");
            });
        }
    }
}