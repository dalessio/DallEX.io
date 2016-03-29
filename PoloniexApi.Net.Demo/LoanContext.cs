using DallEX.io.API.LendingTools;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DallEX.io.View
{
    class LoanContext : DbContext
    {
        public DbSet<LendingOffer> LendingOffers { get; set; }
    }
}

