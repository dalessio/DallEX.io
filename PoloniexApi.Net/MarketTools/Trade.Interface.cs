using System;

namespace DallEX.io.API.MarketTools
{
    public interface ITrade
    {
        DateTime Time { get; }

        OrderType Type { get; }

        double PricePerCoin { get; }

        double AmountQuote { get; }
        double AmountBase { get; }
    }
}
