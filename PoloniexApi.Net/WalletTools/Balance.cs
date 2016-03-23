using Newtonsoft.Json;

namespace Jojatekok.PoloniexAPI.WalletTools
{
    public class Balance
    {
        public int id { get; set; }
        public double available { get; set; }
        public double onOrders { get; set; }
        public double btcValue { get; set; }

        internal Balance()
        {
            
        }

    }
}
