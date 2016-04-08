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
    /// Interaction logic for ucTopDisplay.xaml
    /// </summary>
    public sealed partial class ucTopDisplay : UserControl
    {
        public ucTopDisplay()
        {
            InitializeComponent();
        }

        public async Task LoadLoanOffersAsync(PoloniexClient PoloniexClient)
        {
            await Task.Run(async () =>
            {
                PublicLoanOffersData lendings = null;
                LendingOffer firstLoanOffer = null;

                try
                {
                    lendings = await PoloniexClient.Lendings.GetLoanOffersAsync("BTC");
                    firstLoanOffer = lendings.offers.OrderBy(x => x.rate).First();

                    if (MarketService.Instance().MarketAsync != null)
                        if (MarketService.Instance().MarketAsync.Any())
                        {
                            double ethPriceLast = MarketService.Instance().MarketAsync.Where(x => x.Key.ToString().ToUpper().Equals("BTC_ETH")).OrderBy(x => x.Value.PriceLast).First().Value.PriceLast;
                            double btcPriceLast = MarketService.Instance().MarketAsync.Where(x => x.Key.ToString().ToUpper().Equals("USDT_BTC")).OrderBy(x => x.Value.PriceLast).First().Value.PriceLast;

                            firstLoanOffer.ethExchangeValue = ethPriceLast;
                            firstLoanOffer.btcExchangeValue = btcPriceLast;


                            string eth = string.Concat("BTC/ETH: ", firstLoanOffer.ethExchangeValue.ToString("0.00000000"));
                            string btc = string.Concat("USDT/BTC: ", firstLoanOffer.btcExchangeValue.ToString("0.00000000"));
                            string loan = string.Concat("BTC Loan Rate: ", firstLoanOffer.rate.ToString("0.00000%"));

                            txtDisplay.Dispatcher.Invoke(DispatcherPriority.Background, (ThreadStart)delegate
                            {
                                txtDisplay.Text = string.Concat(btc, "          ", eth, "          ", loan);
                            });

                        }
                }
                finally
                {
                    lendings = null;
                    firstLoanOffer = null;
                }
            });
        }
    }
}
