﻿using System.Collections.Generic;

namespace DallEX.io.API.WalletTools
{
    public interface IDepositWithdrawalList
    {
        IList<Deposit> Deposits { get; }

        IList<Withdrawal> Withdrawals { get; }
    }
}
