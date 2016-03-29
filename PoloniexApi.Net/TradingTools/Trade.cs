using Newtonsoft.Json;
using System;

namespace DallEX.io.API.TradingTools
{
    public class Trade : Order, ITrade
    {
        [JsonProperty("date")]
        private string TimeInternal {
            set { Time = Helper.ParseDateTime(value); }
        }
        public DateTime Time { get; private set; }
    }
}
