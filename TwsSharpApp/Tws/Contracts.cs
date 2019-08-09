using IBApi;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TwsSharpApp
{
    class Contracts
    {
        public static Contract USStock(string symbol)
        {
            Contract contract = new Contract
            {
                Symbol   = symbol,
                SecType  = "STK",
                Currency = "USD",
                Exchange = "ISLAND"
            };

            return contract;
        }
    }
}
