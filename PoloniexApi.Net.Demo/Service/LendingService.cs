using DallEX.io.API.LendingTools;
using System;
using System.Collections.Generic;

namespace DallEX.io.View.Service
{
    public class LendingService
    {
        private static readonly Lazy<LendingService> lazy = new Lazy<LendingService>(() => new LendingService());    

        public static LendingService Instance()
        {
            return lazy.Value;
        }

        public LendingService()
        {
            LendingOffersAsync = null;
        }

        public PublicLoanOffersData LendingOffersAsync;
    }
}