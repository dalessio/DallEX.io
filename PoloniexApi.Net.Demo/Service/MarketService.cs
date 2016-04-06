using DallEX.io.API;
using DallEX.io.API.MarketTools;
using System;
using System.Collections.Generic;

namespace DallEX.io.View
{
    public class MarketService
    {
        private static readonly Lazy<MarketService> lazy = new Lazy<MarketService>(() => new MarketService());    

        public static MarketService Instance()
        {
            return lazy.Value;
        }

        public MarketService()
        {
            MarketAsync = null;
        }

        public IDictionary<CurrencyPair, IMarketData> MarketAsync;

    }
}