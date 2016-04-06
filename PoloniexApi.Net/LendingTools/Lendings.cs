using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace DallEX.io.API.LendingTools
{
    public class Lendings : ILendings
    {
        private ApiWebClient ApiWebClient { get; set; }

        internal Lendings(ApiWebClient apiWebClient)
        {
            ApiWebClient = apiWebClient;
        }

        private PublicLoanOffersData GetLoanOffers(string currency)
        {
            var data = GetData<PublicLoanOffersData>("returnLoanOrders", "currency=" + currency);
            return data;
        }
        public Task<PublicLoanOffersData> GetLoanOffersAsync(string currency)
        {
            return Task.Run(() => GetLoanOffers(currency));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private T GetData<T>(string command, params object[] parameters)
        {
            return ApiWebClient.GetData<T>(Helper.ApiUrlHttpsRelativePublic + command, parameters);
        }


    }
}
