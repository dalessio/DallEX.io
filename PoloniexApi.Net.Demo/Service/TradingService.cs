using System;
using System.Collections.Generic;

namespace DallEX.io.View.Service
{
    public class TradingService
    {
        private static readonly Lazy<TradingService> lazy = new Lazy<TradingService>(() => new TradingService());    

        public static TradingService Instance()
        {
            return lazy.Value;
        }

        public TradingService()
        {
            BtcOpenOrders = null;
            XmrOpenOrders = null;
        }

        public IList<DallEX.io.API.TradingTools.Order> BtcOpenOrders;
        public IList<DallEX.io.API.TradingTools.Order> XmrOpenOrders;
    }
}