using System.Collections.Generic;
namespace Jojatekok.PoloniexAPI.LendingTools
{
    public interface IPublicLoanOffersData
    {
        List<LendingOffer> offers { get; set; }
        List<LendingDemand> demands { get; set; }
    }
}