using DallEX.io.API.LendingTools;
using System;
using System.Data.Entity;

namespace DallEX.io.View
{
    class LoanContext : DbContext
    {
        public DbSet<LendingOffer> LendingOffers { get; set; }
    }
}

