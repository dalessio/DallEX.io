using DallEX.io.API.MarketTools;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DallEX.io.API.LendingTools
{
    public interface ILendings
    {
        /// <summary>Gets a data summary of the markets available.</summary>
        Task<PublicLoanOffersData> GetLoanOffersAsync(string currency);
    }
}
