using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DallEX.io.API.TradingBotTools
{
    public class BotConfig
    {
        public BotConfig()
        {
            Time = DateTime.Now;
        }

        public int Id{ get; set; }

        public virtual DateTime Time { get; set; }

        public string Label { get; set; }

        public virtual CurrencyPair CurrencyPair { get; private set; }

        public double SatoshiBellowTopAsk { get; set; }
        
        public double SatoshiAboveTopAsk { get; set; }

        public double SatoshiBellowTopBid { get; set; }

        public double SatoshiAboveTopBid { get; set; }

        public double StopValueAskCoinBalanceLessThan { get; set; }
        
        public double StopValueBidCoinBalanceLessThan { get; set; }

        public double StopPriceLimitAskCoinBalanceLessThan { get; set; }

        public double StopPriceLimitBidCoinBalanceLessThan { get; set; }

        public double StopWhenSatoshiSpreadLessThan { get; set; }

        public double StopTaskBtcBalanceFallsBellow { get; set; }
        
        public double StopTaskAltcoinBalanceFallsBellow { get; set; }

        public int ThreadsNumber { get; set; }

    }
}
