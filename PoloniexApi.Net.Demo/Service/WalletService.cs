using DallEX.io.API.WalletTools;
using System;
using System.Collections.Generic;

namespace DallEX.io.View.Service
{
    public sealed class WalletService
    {
        private static readonly Lazy<WalletService> lazy = new Lazy<WalletService>(() => new WalletService());    

        public static WalletService Instance()
        {
            return lazy.Value;
        }

        public WalletService()
        {
            WalletAsync = null;
        }

        public IDictionary<string, Balance> WalletAsync;

    }
}