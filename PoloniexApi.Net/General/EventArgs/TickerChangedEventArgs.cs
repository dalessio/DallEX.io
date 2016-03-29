using DallEX.io.API.MarketTools;
using System;

namespace DallEX.io.API
{
    public class TickerChangedEventArgs : EventArgs
    {
        public CurrencyPair CurrencyPair { get; private set; }
        public MarketData MarketData { get; private set; }

        internal TickerChangedEventArgs(CurrencyPair currencyPair, MarketData marketData)
        {
            CurrencyPair = currencyPair;
            MarketData = marketData;
        }
    }
}
