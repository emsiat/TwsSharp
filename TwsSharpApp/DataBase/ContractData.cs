using IBApi;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TwsSharpApp.Data
{
    public class ContractData
    {
        public ContractData ()
        {
        }

        public ContractData (Contract contract)
        {
            Symbol      = contract.Symbol;
            SecType     = contract.SecType;
            Currency    = contract.Currency;
            Exchange    = contract.Exchange;
            PrimaryExch = contract.PrimaryExch;
        }

        public int Id { get; set; }
        public string Symbol      { get; set; }
        public string SecType     { get; set; }
        public string Exchange    { get; set; }
        public string Currency    { get; set; }
        public string PrimaryExch { get; set; }

        public Contract ToContract()
        {
            Contract contr = new Contract();

            contr.Symbol      = Symbol;
            contr.SecType     = SecType;
            contr.Exchange    = Exchange;
            contr.Currency    = Currency;
            contr.PrimaryExch = PrimaryExch;

            return contr;
        }
    }
}
