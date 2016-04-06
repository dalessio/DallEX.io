using DallEX.io.API.LendingTools;
using System;
using System.Data.Entity;

namespace DallEX.io.View
{
    public sealed class LoanContext : DbContext
    {
        private static readonly Lazy<LoanContext> lazy = new Lazy<LoanContext>(() => new LoanContext());

        public static LoanContext Instance()
        {
            return lazy.Value;
        }

        public LoanContext() : base()
        {
        }

        public DbSet<LendingOffer> LendingOffers { get; set; }
    }
}

