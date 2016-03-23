using System;
namespace Jojatekok.PoloniexAPI.LendingTools
{
    public class LendingDemand
    {
        public int id { get; set; }

        public double rate { get; set; }
        public double amount { get; set; }
        public int rangeMin { get; set; }
        public int rangeMax { get; set; }

        public DateTime dataRegistro { get; set; }

        internal LendingDemand()
        {
            this.dataRegistro = DateTime.Now;
        }
    }
}