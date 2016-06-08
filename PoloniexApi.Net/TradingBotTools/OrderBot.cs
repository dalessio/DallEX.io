using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DallEX.io.API.TradingBotTools
{
    public class OrderBot : API.TradingTools.Order
    {
        public OrderBot()
        {
            Time = DateTime.Now;
        }

        public int Id{ get; private set; }

        public virtual DateTime Time { get; private set; }

        public double Spread { get; private set; }

        public virtual CurrencyPair CurrencyPair { get; private set; }
    }
}
