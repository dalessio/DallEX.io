using Newtonsoft.Json;

namespace DallEX.io.API.WalletTools
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
