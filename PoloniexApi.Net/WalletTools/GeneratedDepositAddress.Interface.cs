namespace DallEX.io.API.WalletTools
{
    public interface IGeneratedDepositAddress
    {
        bool IsGenerationSuccessful { get; }

        string Address { get; }
    }
}
