namespace DallEX.io.API.MarketTools
{
    public interface IOrder
    {
        double PricePerCoin { get; }

        double AmountQuote { get; }
        double AmountBase { get; }
    }
}
