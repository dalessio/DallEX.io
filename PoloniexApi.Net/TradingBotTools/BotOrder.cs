using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DallEX.io.API.TradingBotTools
{
    public class BotOrder : API.TradingTools.Order
    {
        public BotOrder()
        {
            Time = DateTime.Now;
        }

        public long Id{ get; private set; }

        public long MainIdOrder { get; set; }

        public virtual DateTime Time { get; private set; }

        public double Spread { get; private set; }

        public virtual CurrencyPair CurrencyPair { get; private set; }
    }
}
