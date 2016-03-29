using System.Collections.Generic;

namespace DallEX.io.API.MarketTools
{
    public interface IOrderBook
    {
        IList<IOrder> BuyOrders { get; }
        IList<IOrder> SellOrders { get; }
    }
}
