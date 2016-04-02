using DallEX.io.API;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace DallEX.io.View.Library
{
    public abstract class PageBase : Page
    {
        //public PoloniexClient PoloniexClient { get; set; }

        public PageBase() : base()
        {
            //PoloniexClient = PoloniexClient.Instance(ApiKeys.PublicKey, ApiKeys.PrivateKey);
        }
    }
}
