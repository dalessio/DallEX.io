using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using DallEX.io.API.MarketTools;
using Newtonsoft.Json;

namespace DallEX.io.API.LendingTools
{
    public class LendingOffer
    {
        public int id { get; set; }

        public double rate { get; set; }
        public double amount { get; set; }
        public int rangeMin { get; set; }
        public int rangeMax { get; set; }
        
        public double btcExchangeValue { get; set; }
        public double ethExchangeValue { get; set; }

        public string currency { get; set; }

        public DateTime dataRegistro { get; set; }

        internal LendingOffer()
        {
            this.dataRegistro = DateTime.Now;
        }
    }
}
