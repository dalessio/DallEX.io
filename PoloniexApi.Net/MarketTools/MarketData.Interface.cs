using System;
namespace Jojatekok.PoloniexAPI.MarketTools
{
    public interface IMarketData
    {
        double PriceLast { get; }
        double PriceChangePercentage { get; }

        double Volume24HourBase { get; }
        double Volume24HourQuote { get; }

        double OrderTopBuy { get; }
        double OrderTopSell { get; }
        double OrderSpread { get; }
        double OrderSpreadPercentage { get; }

        bool IsFrozen { get; }

        DateTime primeiraCompra { get; set; }
        DateTime primeiraVenda { get; set; }
        double gapTradeSeconds { get; set; }
        double indiceMaluco { get; set; }

        double btcExchangeValue { get; set; }
        double ethExchangeValue { get; set; }
        DateTime dataRegistro { get; set; }

    }
}
