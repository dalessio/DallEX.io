using DallEX.io.API.LendingTools;
using DallEX.io.API.LiveTools;
using DallEX.io.API.MarketTools;
using DallEX.io.API.TradingTools;
using DallEX.io.API.WalletTools;
using System;

namespace DallEX.io.API
{
    public sealed class PoloniexClient
    {
        private static readonly Lazy<PoloniexClient> lazy =
        new Lazy<PoloniexClient>(() => new PoloniexClient());

        /// <summary>Represents the authenticator object of the client.</summary>
        public IAuthenticator Authenticator { get; private set; }

        /// <summary>A class which contains market tools for the client.</summary>
        public ILendings Lendings { get; private set; }

        /// <summary>A class which contains market tools for the client.</summary>
        public IMarkets Markets { get; private set; }
        /// <summary>A class which contains trading tools for the client.</summary>
        public ITrading Trading { get; private set; }
        /// <summary>A class which contains wallet tools for the client.</summary>
        public IWallet Wallet { get; private set; }
        /// <summary>A class which represents live data fetched automatically from the server.</summary>
        public ILive Live { get; private set; }


        private static string _publicApiKey = "";
        private static string _privateApiKey = "";

        public static PoloniexClient Instance(string publicApiKey, string privateApiKey)
        {
            _publicApiKey = publicApiKey;
            _privateApiKey = privateApiKey;

            return lazy.Value;
        }

        /// <summary>Creates a new instance of Poloniex API .NET's client service.</summary>
        /// <param name="publicApiKey">Your public API key.</param>
        /// <param name="privateApiKey">Your private API key.</param>
        private PoloniexClient()
        {
            var apiWebClient = new ApiWebClient(Helper.ApiUrlHttpsBase);

            Authenticator = new Authenticator(apiWebClient, _publicApiKey, _privateApiKey);

            Lendings = new Lendings(apiWebClient);
            Markets = new Markets(apiWebClient);
            Trading = new Trading(apiWebClient);
            Wallet = new Wallet(apiWebClient);
            Live = new Live();
        }
    }
}
