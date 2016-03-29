using System;

namespace DallEX.io.API.TradingTools
{
    public interface ITrade : IOrder
    {
        DateTime Time { get; }
    }
}
