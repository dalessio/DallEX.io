﻿using System;

namespace DallEX.io.API.MarketTools
{
    public interface IMarketChartData
    {
        DateTime Time { get; }

        double Open { get; }
        double Close { get; }

        double High { get; }
        double Low { get; }

        double VolumeBase { get; }
        double VolumeQuote { get; }

        double WeightedAverage { get; }
    }
}
