using Newtonsoft.Json;
using System.Collections.Generic;

namespace DallEX.io.API.WalletTools
{
    public class DepositWithdrawalList : IDepositWithdrawalList
    {
        [JsonProperty("deposits")]
        public IList<Deposit> Deposits { get; private set; }

        [JsonProperty("withdrawals")]
        public IList<Withdrawal> Withdrawals { get; private set; }
    }
}
