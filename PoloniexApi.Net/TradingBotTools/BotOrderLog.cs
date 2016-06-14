using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DallEX.io.API.TradingBotTools
{
    public class BotOrderLog : API.TradingTools.Order
    {
        public BotOrderLog()
        {
            Time = DateTime.Now;
        }

        public int Id{ get; private set; }

        public virtual DateTime Time { get; private set; }

        public double Value { get; private set; }

        public virtual CurrencyPair CurrencyPair { get; private set; }
    }
}
