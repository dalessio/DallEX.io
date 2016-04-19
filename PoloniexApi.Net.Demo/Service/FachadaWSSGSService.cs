using DallEX.io.API.LendingTools;
using System;
using System.Collections.Generic;

namespace DallEX.io.View.Service
{
    public class FachadaWSSGSService
    {
        private static readonly Lazy<FachadaWSSGSService> lazy = new Lazy<FachadaWSSGSService>(() => new FachadaWSSGSService());    

        public static FachadaWSSGSService Instance()
        {
            return lazy.Value;
        }

        public FachadaWSSGSService()
        {
            getUltimoValorVOResponseAsync = null;
        }

        public FachadaWSSGS.getUltimoValorVOResponse getUltimoValorVOResponseAsync;
    }
}