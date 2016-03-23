﻿using Newtonsoft.Json;
using System;

namespace Jojatekok.PoloniexAPI.MarketTools
{
    [JsonObject(MemberSerialization = MemberSerialization.OptIn)]
    public class MarketData : IMarketData
    {
        public int id { get; set; }

        [JsonProperty("last")]
        public double PriceLast { get; internal set; }
        [JsonProperty("percentChange")]
        public double PriceChangePercentage { get; internal set; }

        [JsonProperty("baseVolume")]
        public double Volume24HourBase { get; internal set; }
        [JsonProperty("quoteVolume")]
        public double Volume24HourQuote { get; internal set; }

        [JsonProperty("highestBid")]
        public double OrderTopBuy { get; internal set; }
        [JsonProperty("lowestAsk")]
        public double OrderTopSell { get; internal set; }
        public double OrderSpread {
            get { return (OrderTopSell - OrderTopBuy).Normalize(); }
        }
        public double OrderSpreadPercentage {
            get { return OrderTopSell / OrderTopBuy - 1; }
        }

        [JsonProperty("isFrozen")]
        internal byte IsFrozenInternal {
            set { IsFrozen = value != 0; }
        }
        public bool IsFrozen { get; private set; }

        public DateTime primeiraCompra { get; set; }
        public DateTime primeiraVenda { get; set; }
        public double gapTradeSeconds { get; set; }
        public double indiceMaluco { get; set; }

        public double btcExchangeValue { get; set; }
        public double ethExchangeValue { get; set; }

        public DateTime dataRegistro { get; set; }

        internal MarketData()
        {
            this.dataRegistro = DateTime.Now;
        }

    }
}
