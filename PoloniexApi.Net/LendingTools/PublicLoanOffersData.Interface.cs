using System.Collections.Generic;
namespace DallEX.io.API.LendingTools
{
    public interface IPublicLoanOffersData
    {
        List<LendingOffer> offers { get; set; }
        List<LendingDemand> demands { get; set; }
    }
}