using System;
using System.Collections.Generic;

namespace DallEX.io.View.Service
{
    public class TradeHistoryService
    {
        private static readonly Lazy<TradeHistoryService> lazy = new Lazy<TradeHistoryService>(() => new TradeHistoryService());    

        public static TradeHistoryService Instance()
        {
            return lazy.Value;
        }

        public TradeHistoryService()
        {
            TradesHistoryListAsync = null;
        }

        public IList<DallEX.io.API.MarketTools.ITrade> TradesHistoryListAsync;
    }
}