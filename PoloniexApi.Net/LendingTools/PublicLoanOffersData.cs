using System.Collections.Generic;
namespace Jojatekok.PoloniexAPI.LendingTools
{
    public class PublicLoanOffersData
    {
        public List<LendingOffer> offers { get; set; }
        public List<LendingDemand> demands { get; set; }


        internal PublicLoanOffersData()
        {

        }
    }
}
