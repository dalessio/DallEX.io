using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DallEX.io.API.TradingBotTools
{
    public class BotParameters
    {
        public BotParameters()
        {
            Time = DateTime.Now;
        }

        public int Id{ get; private set; }

        public virtual DateTime Time { get; private set; }

        public double UnitValueAboveTopAsk { get; private set; }
        public double UnitValueBelowTopBid { get; private set; }

        public virtual CurrencyPair CurrencyPair { get; private set; }
    }
}
