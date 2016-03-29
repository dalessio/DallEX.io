using DallEX.io.API;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace DallEX.io.View
{
    public abstract class BaseView : System.Windows.Window
    {
        public PoloniexClient PoloniexClient { get; set; }

        public BaseView()
        {
            PoloniexClient = PoloniexClient.Instance(ApiKeys.PublicKey, ApiKeys.PrivateKey);
        }
    }
}
