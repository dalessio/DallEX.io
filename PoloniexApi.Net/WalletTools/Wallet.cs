﻿using DallEX.io.API.MarketTools;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace DallEX.io.API.WalletTools
{
    public class Wallet : IWallet
    {
        private ApiWebClient ApiWebClient { get; set; }

        internal Wallet(ApiWebClient apiWebClient)
        {
            ApiWebClient = apiWebClient;
        }

        private IDictionary<string, Balance> GetBalances()
        {
            var postData = new Dictionary<string, object>();

            var data = PostData<IDictionary<string, Balance>>("returnCompleteBalances", postData);
            return data;
        }

        private IDictionary<string, string> GetDepositAddresses()
        {
            var postData = new Dictionary<string, object>();

            var data = PostData<IDictionary<string, string>>("returnDepositAddresses", postData);
            return data;
        }

        private IDepositWithdrawalList GetDepositsAndWithdrawals(DateTime startTime, DateTime endTime)
        {
            var postData = new Dictionary<string, object> {
                { "start", Helper.DateTimeToUnixTimeStamp(startTime) },
                { "end", Helper.DateTimeToUnixTimeStamp(endTime) }
            };

            var data = PostData<DepositWithdrawalList>("returnDepositsWithdrawals", postData);
            return data;
        }

        private IGeneratedDepositAddress PostGenerateNewDepositAddress(string currency)
        {
            var postData = new Dictionary<string, object> {
                { "currency", currency }
            };

            var data = PostData<IGeneratedDepositAddress>("generateNewAddress", postData);
            return data;
        }

        private void PostWithdrawal(string currency, double amount, string address, string paymentId)
        {
            var postData = new Dictionary<string, object> {
                { "currency", currency },
                { "amount", amount.ToStringNormalized() },
                { "address", address }
            };

            if (paymentId != null) {
                postData.Add("paymentId", paymentId);
            }

            PostData<IGeneratedDepositAddress>("withdraw", postData);
        }

        private IList<ITrade> GetTradesHistory(CurrencyPair currencyPair)
        {
            var postData = new Dictionary<string, object> {
                { "currencyPair", currencyPair }
            };

            var data = PostData<IList<Trade>>("returnTradeHistory", postData);

            return new List<ITrade>(data);
        }


        #region Trade
        private IList<ITrade> GetTradesHistory(CurrencyPair currencyPair, DateTime startTime, DateTime endTime)
        {
            var postData = new Dictionary<string, object> {
                { "currencyPair", currencyPair },
                { "start", Helper.DateTimeToUnixTimeStamp(startTime) },
                { "end", Helper.DateTimeToUnixTimeStamp(endTime) }
            };

            var data = PostData<IList<Trade>>("returnTradeHistory" , postData);

            return new List<ITrade>(data);
        }
        public Task<IList<ITrade>> GetTradesHistoryAsync(CurrencyPair currencyPair)
        {
            return Task.Run(() => GetTradesHistory(currencyPair));
        }

        public Task<IList<ITrade>> GetTradesHistoryAsync(CurrencyPair currencyPair, DateTime startTime, DateTime endTime)
        {
            return Task.Run(() => GetTradesHistory(currencyPair, startTime, endTime));
        }


        #endregion


        public Task<IDictionary<string, Balance>> GetBalancesAsync()
        {
            return Task.Run(() => GetBalances());
        }

        public Task<IDictionary<string, string>> GetDepositAddressesAsync()
        {
            return Task.Run(() => GetDepositAddresses());
        }

        public Task<IDepositWithdrawalList> GetDepositsAndWithdrawalsAsync(DateTime startTime, DateTime endTime)
        {
            return Task.Run(() => GetDepositsAndWithdrawals(startTime, endTime));
        }

        public Task<IDepositWithdrawalList> GetDepositsAndWithdrawalsAsync()
        {
            return Task.Run(() => GetDepositsAndWithdrawals(Helper.DateTimeUnixEpochStart, DateTime.MaxValue));
        }

        public Task<IGeneratedDepositAddress> PostGenerateNewDepositAddressAsync(string currency)
        {
            return Task.Run(() => PostGenerateNewDepositAddress(currency));
        }

        public Task PostWithdrawalAsync(string currency, double amount, string address, string paymentId)
        {
            return Task.Run(() => PostWithdrawal(currency, amount, address, paymentId));
        }

        public Task PostWithdrawalAsync(string currency, double amount, string address)
        {
            return Task.Run(() => PostWithdrawal(currency, amount, address, null));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private T PostData<T>(string command, Dictionary<string, object> postData)
        {
            return ApiWebClient.PostData<T>(command, postData);
        }
    }
}
