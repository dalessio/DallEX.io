namespace DallEX.io.API.WalletTools
{
    public interface IBalance
    {
        double available { get; }
        double onOrders { get; }
        double btcValue { get; }
    }
}
