using IBApi;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TwsSharpApp
{
    public partial class TwsWrapper : EWrapper
    {
        private Dictionary<int, List<ContractDetails>> contractsList = new Dictionary<int, List<ContractDetails>>();

        //
        // Request Contract Details about a stock symbol:
        //
        public void RequestContractDetails_Stock(string symbol)
        {
            ClientSocket.reqContractDetails(nextValidId(), new Contract() {Symbol = symbol, SecType  = "STK" });
        }

        public event EventHandler<ContractDetailsRecv_EventArgs> ContractDetailsEndReceived_Event;
        public event EventHandler<ContractDetailsRecv_EventArgs> ContractDetailsReceived_Event;

        public virtual void contractDetails(int reqId, ContractDetails contractDetails)
        {
            //Debug.WriteLine("ContractDetails begin. ReqId: " + reqId);
            //printContractMsg(contractDetails.Contract);
            //printContractDetailsMsg(contractDetails);
            //Debug.WriteLine("ContractDetails end. ReqId: " + reqId);

            if(contractsList.ContainsKey(reqId) == false)
            {
                contractsList.Add(reqId, new List<ContractDetails>());
            }

            ContractDetailsReceived_Event?.Invoke(this, new ContractDetailsRecv_EventArgs(reqId, contractDetails));
            contractsList[reqId].Add(contractDetails);
        }

        public virtual void contractDetailsEnd(int reqId)
        {
            Debug.WriteLine("ContractDetailsEnd. " + reqId + "\n");
            ContractDetailsEndReceived_Event?.Invoke(this, new ContractDetailsRecv_EventArgs(reqId, contractsList[reqId][0]));

            contractsList.Remove(reqId);
        }

        public void printContractMsg(Contract contract)
        {
            Debug.WriteLine("\tConId: " + contract.ConId);
            Debug.WriteLine("\tSymbol: " + contract.Symbol);
            Debug.WriteLine("\tSecType: " + contract.SecType);
            Debug.WriteLine("\tLastTradeDateOrContractMonth: " + contract.LastTradeDateOrContractMonth);
            Debug.WriteLine("\tStrike: " + contract.Strike);
            Debug.WriteLine("\tRight: " + contract.Right);
            Debug.WriteLine("\tMultiplier: " + contract.Multiplier);
            Debug.WriteLine("\tExchange: " + contract.Exchange);
            Debug.WriteLine("\tPrimaryExchange: " + contract.PrimaryExch);
            Debug.WriteLine("\tCurrency: " + contract.Currency);
            Debug.WriteLine("\tLocalSymbol: " + contract.LocalSymbol);
            Debug.WriteLine("\tTradingClass: " + contract.TradingClass);
        }

        public void printContractDetailsMsg(ContractDetails contractDetails)
        {
            Debug.WriteLine("\tMarketName: " + contractDetails.MarketName);
            Debug.WriteLine("\tMinTick: " + contractDetails.MinTick);
            Debug.WriteLine("\tPriceMagnifier: " + contractDetails.PriceMagnifier);
            Debug.WriteLine("\tOrderTypes: " + contractDetails.OrderTypes);
            Debug.WriteLine("\tValidExchanges: " + contractDetails.ValidExchanges);
            Debug.WriteLine("\tUnderConId: " + contractDetails.UnderConId);
            Debug.WriteLine("\tLongName: " + contractDetails.LongName);
            Debug.WriteLine("\tContractMonth: " + contractDetails.ContractMonth);
            Debug.WriteLine("\tIndystry: " + contractDetails.Industry);
            Debug.WriteLine("\tCategory: " + contractDetails.Category);
            Debug.WriteLine("\tSubCategory: " + contractDetails.Subcategory);
            Debug.WriteLine("\tTimeZoneId: " + contractDetails.TimeZoneId);
            Debug.WriteLine("\tTradingHours: " + contractDetails.TradingHours);
            Debug.WriteLine("\tLiquidHours: " + contractDetails.LiquidHours);
            Debug.WriteLine("\tEvRule: " + contractDetails.EvRule);
            Debug.WriteLine("\tEvMultiplier: " + contractDetails.EvMultiplier);
            Debug.WriteLine("\tMdSizeMultiplier: " + contractDetails.MdSizeMultiplier);
            Debug.WriteLine("\tAggGroup: " + contractDetails.AggGroup);
            Debug.WriteLine("\tUnderSymbol: " + contractDetails.UnderSymbol);
            Debug.WriteLine("\tUnderSecType: " + contractDetails.UnderSecType);
            Debug.WriteLine("\tMarketRuleIds: " + contractDetails.MarketRuleIds);
            Debug.WriteLine("\tRealExpirationDate: " + contractDetails.RealExpirationDate);
            Debug.WriteLine("\tLastTradeTime: " + contractDetails.LastTradeTime);
            printContractDetailsSecIdList(contractDetails.SecIdList);
        }

        public void printContractDetailsSecIdList(List<TagValue> secIdList)
        {
            if (secIdList != null && secIdList.Count > 0)
            {
                Debug.Write("\tSecIdList: {");
                foreach (TagValue tagValue in secIdList)
                {
                    Debug.Write(tagValue.Tag + "=" + tagValue.Value + ";");
                }
                Debug.WriteLine("}");
            }
        }

        public void printBondContractDetailsMsg(ContractDetails contractDetails)
        {
            Debug.WriteLine("\tSymbol: " + contractDetails.Contract.Symbol);
            Debug.WriteLine("\tSecType: " + contractDetails.Contract.SecType);
            Debug.WriteLine("\tCusip: " + contractDetails.Cusip);
            Debug.WriteLine("\tCoupon: " + contractDetails.Coupon);
            Debug.WriteLine("\tMaturity: " + contractDetails.Maturity);
            Debug.WriteLine("\tIssueDate: " + contractDetails.IssueDate);
            Debug.WriteLine("\tRatings: " + contractDetails.Ratings);
            Debug.WriteLine("\tBondType: " + contractDetails.BondType);
            Debug.WriteLine("\tCouponType: " + contractDetails.CouponType);
            Debug.WriteLine("\tConvertible: " + contractDetails.Convertible);
            Debug.WriteLine("\tCallable: " + contractDetails.Callable);
            Debug.WriteLine("\tPutable: " + contractDetails.Putable);
            Debug.WriteLine("\tDescAppend: " + contractDetails.DescAppend);
            Debug.WriteLine("\tExchange: " + contractDetails.Contract.Exchange);
            Debug.WriteLine("\tCurrency: " + contractDetails.Contract.Currency);
            Debug.WriteLine("\tMarketName: " + contractDetails.MarketName);
            Debug.WriteLine("\tTradingClass: " + contractDetails.Contract.TradingClass);
            Debug.WriteLine("\tConId: " + contractDetails.Contract.ConId);
            Debug.WriteLine("\tMinTick: " + contractDetails.MinTick);
            Debug.WriteLine("\tMdSizeMultiplier: " + contractDetails.MdSizeMultiplier);
            Debug.WriteLine("\tOrderTypes: " + contractDetails.OrderTypes);
            Debug.WriteLine("\tValidExchanges: " + contractDetails.ValidExchanges);
            Debug.WriteLine("\tNextOptionDate: " + contractDetails.NextOptionDate);
            Debug.WriteLine("\tNextOptionType: " + contractDetails.NextOptionType);
            Debug.WriteLine("\tNextOptionPartial: " + contractDetails.NextOptionPartial);
            Debug.WriteLine("\tNotes: " + contractDetails.Notes);
            Debug.WriteLine("\tLong Name: " + contractDetails.LongName);
            Debug.WriteLine("\tEvRule: " + contractDetails.EvRule);
            Debug.WriteLine("\tEvMultiplier: " + contractDetails.EvMultiplier);
            Debug.WriteLine("\tAggGroup: " + contractDetails.AggGroup);
            Debug.WriteLine("\tMarketRuleIds: " + contractDetails.MarketRuleIds);
            Debug.WriteLine("\tLastTradeTime: " + contractDetails.LastTradeTime);
            Debug.WriteLine("\tTimeZoneId: " + contractDetails.TimeZoneId);
            printContractDetailsSecIdList(contractDetails.SecIdList);
        }

        public virtual void bondContractDetails(int requestId, ContractDetails contractDetails)
        {
            Debug.WriteLine("BondContractDetails begin. ReqId: " + requestId);
            printBondContractDetailsMsg(contractDetails);
            Debug.WriteLine("BondContractDetails end. ReqId: " + requestId);
        }
    }
}
