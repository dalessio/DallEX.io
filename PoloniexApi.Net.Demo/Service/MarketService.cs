using DallEX.io.API;
using DallEX.io.API.MarketTools;
using System;
using System.Collections.Generic;

namespace DallEX.io.View.Service
{
    public sealed class MarketService
    {
        private static readonly Lazy<MarketService> lazy = new Lazy<MarketService>(() => new MarketService());    

        public static MarketService Instance()
        {
            return lazy.Value;
        }

        public MarketService()
        {
            MarketList = null;
            TradesHistoryList = null;
        }

        public IDictionary<CurrencyPair, IMarketData> MarketList;
        public IList<ITrade> TradesHistoryList;

    }
}