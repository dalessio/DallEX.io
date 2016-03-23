using Jojatekok.PoloniexAPI.LendingTools;
using Jojatekok.PoloniexAPI.MarketTools;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Jojatekok.PoloniexAPI.Demo
{
    class LoanContext : DbContext
    {
        public DbSet<LendingOffer> LendingOffers { get; set; }
        //public DbSet<MarketData> MarketData { get; set; }
    }
}

