using Jojatekok.PoloniexAPI.MarketTools;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Jojatekok.PoloniexAPI.LendingTools
{
    public interface ILendings
    {
        /// <summary>Gets a data summary of the markets available.</summary>
        Task<PublicLoanOffersData> GetLoanOffersAsync(string currency);
    }
}
