using IBApi;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;

namespace TwsSharpApp
{
    // Here are methods defined in EWrapper which we are not using.
    // Methods we use are defined in TwsWrapper.cs 
    // As we use methods from interface, we remove them from here.
    public partial class TwsWrapper : EWrapper
    {
        public string BboExchange { get; private set; }

        public virtual void error(Exception e)
        {
            Debug.WriteLine("Exception thrown: " + e);
            throw e;
        }

        public virtual void connectionClosed()
        {
            Debug.WriteLine("Connection closed.\n");
        }

        public virtual void currentTime(long time)
        {
            Debug.WriteLine("Current Time: " + time + "\n");
        }

        public virtual void tickPrice(int tickerId, int field, double price, TickAttrib attribs)
        {
            Debug.WriteLine("Tick Price. Ticker Id:" + tickerId + ", Field: " + field + ", Price: " + price + ", CanAutoExecute: " + attribs.CanAutoExecute +
                ", PastLimit: " + attribs.PastLimit + ", PreOpen: " + attribs.PreOpen);
        }

        public virtual void tickSize(int tickerId, int field, int size)
        {
            Debug.WriteLine("Tick Size. Ticker Id:" + tickerId + ", Field: " + field + ", Size: " + size);
        }

        public virtual void tickString(int tickerId, int tickType, string value)
        {
            Debug.WriteLine("Tick string. Ticker Id:" + tickerId + ", Type: " + tickType + ", Value: " + value);
        }

        public virtual void tickGeneric(int tickerId, int field, double value)
        {
            Debug.WriteLine("Tick Generic. Ticker Id:" + tickerId + ", Field: " + field + ", Value: " + value);
        }

        public virtual void tickEFP(int tickerId, int tickType, double basisPoints, string formattedBasisPoints, double impliedFuture, int holdDays, string futureLastTradeDate, double dividendImpact, double dividendsToLastTradeDate)
        {
            Debug.WriteLine("TickEFP. " + tickerId + ", Type: " + tickType + ", BasisPoints: " + basisPoints + ", FormattedBasisPoints: " + formattedBasisPoints + ", ImpliedFuture: " + impliedFuture + ", HoldDays: " + holdDays + ", FutureLastTradeDate: " + futureLastTradeDate + ", DividendImpact: " + dividendImpact + ", DividendsToLastTradeDate: " + dividendsToLastTradeDate);
        }

        public virtual void tickSnapshotEnd(int tickerId)
        {
            Debug.WriteLine("TickSnapshotEnd: " + tickerId);
        }

        public virtual void deltaNeutralValidation(int reqId, DeltaNeutralContract deltaNeutralContract)
        {
            Debug.WriteLine("DeltaNeutralValidation. " + reqId + ", ConId: " + deltaNeutralContract.ConId + ", Delta: " + deltaNeutralContract.Delta + ", Price: " + deltaNeutralContract.Price);
        }

        public virtual void managedAccounts(string accountsList)
        {
            Debug.WriteLine("Account list: " + accountsList);
        }

        public virtual void tickOptionComputation(int tickerId, int field, double impliedVolatility, double delta, double optPrice, double pvDividend, double gamma, double vega, double theta, double undPrice)
        {
            Debug.WriteLine("TickOptionComputation. TickerId: " + tickerId + ", field: " + field + ", ImpliedVolatility: " + impliedVolatility + ", Delta: " + delta
                + ", OptionPrice: " + optPrice + ", pvDividend: " + pvDividend + ", Gamma: " + gamma + ", Vega: " + vega + ", Theta: " + theta + ", UnderlyingPrice: " + undPrice);
        }

        public virtual void accountSummary(int reqId, string account, string tag, string value, string currency)
        {
            Debug.WriteLine("Acct Summary. ReqId: " + reqId + ", Acct: " + account + ", Tag: " + tag + ", Value: " + value + ", Currency: " + currency);
        }

        public virtual void accountSummaryEnd(int reqId)
        {
            Debug.WriteLine("AccountSummaryEnd. Req Id: " + reqId + "\n");
        }

        public virtual void updateAccountValue(string key, string value, string currency, string accountName)
        {
            Debug.WriteLine("UpdateAccountValue. Key: " + key + ", Value: " + value + ", Currency: " + currency + ", AccountName: " + accountName);
        }

        public virtual void updatePortfolio(Contract contract, double position, double marketPrice, double marketValue, double averageCost, double unrealizedPNL, double realizedPNL, string accountName)
        {
            Debug.WriteLine("UpdatePortfolio. " + contract.Symbol + ", " + contract.SecType + " @ " + contract.Exchange
                + ": Position: " + position + ", MarketPrice: " + marketPrice + ", MarketValue: " + marketValue + ", AverageCost: " + averageCost
                + ", UnrealizedPNL: " + unrealizedPNL + ", RealizedPNL: " + realizedPNL + ", AccountName: " + accountName);
        }

        public virtual void updateAccountTime(string timestamp)
        {
            Debug.WriteLine("UpdateAccountTime. Time: " + timestamp + "\n");
        }

        public virtual void accountDownloadEnd(string account)
        {
            Debug.WriteLine("Account download finished: " + account + "\n");
        }

        public virtual void orderStatus(int orderId, string status, double filled, double remaining, double avgFillPrice, int permId, int parentId, double lastFillPrice, int clientId, string whyHeld, double mktCapPrice)
        {
            Debug.WriteLine("OrderStatus. Id: " + orderId + ", Status: " + status + ", Filled" + filled + ", Remaining: " + remaining
                + ", AvgFillPrice: " + avgFillPrice + ", PermId: " + permId + ", ParentId: " + parentId + ", LastFillPrice: " + lastFillPrice + ", ClientId: " + clientId + ", WhyHeld: " + whyHeld + ", MktCapPrice: " + mktCapPrice);
        }

        public virtual void openOrder(int orderId, Contract contract, Order order, OrderState orderState)
        {
            Debug.WriteLine("OpenOrder. ID: " + orderId + ", " + contract.Symbol + ", " + contract.SecType + " @ " + contract.Exchange + ": " + order.Action + ", " + order.OrderType + " " + order.TotalQuantity + ", " + orderState.Status);
            if (order.WhatIf)
            {
                Debug.WriteLine("What-If. ID: " + orderId +
                    ", InitMarginBefore: " + Util.formatDoubleString(orderState.InitMarginBefore) + ", MaintMarginBefore: " + Util.formatDoubleString(orderState.MaintMarginBefore) + " EquityWithLoanBefore: " + Util.formatDoubleString(orderState.EquityWithLoanBefore) +
                    ", InitMarginChange: " + Util.formatDoubleString(orderState.InitMarginChange) + ", MaintMarginChange: " + Util.formatDoubleString(orderState.MaintMarginChange) + " EquityWithLoanChange: " + Util.formatDoubleString(orderState.EquityWithLoanChange) +
                    ", InitMarginAfter: " + Util.formatDoubleString(orderState.InitMarginAfter) + ", MaintMarginAfter: " + Util.formatDoubleString(orderState.MaintMarginAfter) + " EquityWithLoanAfter: " + Util.formatDoubleString(orderState.EquityWithLoanAfter));
            }
        }

        public virtual void openOrderEnd()
        {
            Debug.WriteLine("OpenOrderEnd");
        }

        public virtual void execDetails(int reqId, Contract contract, Execution execution)
        {
            Debug.WriteLine("ExecDetails. " + reqId + " - " + contract.Symbol + ", " + contract.SecType + ", " + contract.Currency + " - " + execution.ExecId + ", " + execution.OrderId + ", " + execution.Shares + ", " + execution.LastLiquidity);
        }

        public virtual void execDetailsEnd(int reqId)
        {
            Debug.WriteLine("ExecDetailsEnd. " + reqId + "\n");
        }

        public virtual void commissionReport(CommissionReport commissionReport)
        {
            Debug.WriteLine("CommissionReport. " + commissionReport.ExecId + " - " + commissionReport.Commission + " " + commissionReport.Currency + " RPNL " + commissionReport.RealizedPNL);
        }

        public virtual void fundamentalData(int reqId, string data)
        {
            Debug.WriteLine("FundamentalData. " + reqId + "" + data + "\n");
        }

        public virtual void marketDataType(int reqId, int marketDataType)
        {
            Debug.WriteLine("MarketDataType. " + reqId + ", Type: " + marketDataType + "\n");
        }

        public virtual void updateMktDepth(int tickerId, int position, int operation, int side, double price, int size)
        {
            Debug.WriteLine("UpdateMarketDepth. " + tickerId + " - Position: " + position + ", Operation: " + operation + ", Side: " + side + ", Price: " + price + ", Size: " + size);
        }

        public virtual void updateMktDepthL2(int tickerId, int position, string marketMaker, int operation, int side, double price, int size, bool isSmartDepth)
        {
            Debug.WriteLine("UpdateMarketDepthL2. " + tickerId + " - Position: " + position + ", Operation: " + operation + ", Side: " + side + ", Price: " + price + ", Size: " + size + ", isSmartDepth: " + isSmartDepth);
        }

        public virtual void updateNewsBulletin(int msgId, int msgType, String message, String origExchange)
        {
            Debug.WriteLine("News Bulletins. " + msgId + " - Type: " + msgType + ", Message: " + message + ", Exchange of Origin: " + origExchange + "\n");
        }

        public virtual void position(string account, Contract contract, double pos, double avgCost)
        {
            Debug.WriteLine("Position. " + account + " - Symbol: " + contract.Symbol + ", SecType: " + contract.SecType + ", Currency: " + contract.Currency + ", Position: " + pos + ", Avg cost: " + avgCost);
        }

        public virtual void positionEnd()
        {
            Debug.WriteLine("PositionEnd \n");
        }

        public virtual void scannerParameters(string xml)
        {
            Debug.WriteLine("ScannerParameters. " + xml + "\n");
        }

        public virtual void scannerData(int reqId, int rank, ContractDetails contractDetails, string distance, string benchmark, string projection, string legsStr)
        {
            Debug.WriteLine("ScannerData. " + reqId + " - Rank: " + rank + ", Symbol: " + contractDetails.Contract.Symbol + ", SecType: " + contractDetails.Contract.SecType + ", Currency: " + contractDetails.Contract.Currency
                + ", Distance: " + distance + ", Benchmark: " + benchmark + ", Projection: " + projection + ", Legs String: " + legsStr);
        }

        public virtual void scannerDataEnd(int reqId)
        {
            Debug.WriteLine("ScannerDataEnd. " + reqId);
        }

        public virtual void receiveFA(int faDataType, string faXmlData)
        {
            Debug.WriteLine("Receing FA: " + faDataType + " - " + faXmlData);
        }

        public virtual void verifyMessageAPI(string apiData)
        {
            Debug.WriteLine("verifyMessageAPI: " + apiData);
        }
        public virtual void verifyCompleted(bool isSuccessful, string errorText)
        {
            Debug.WriteLine("verifyCompleted. IsSuccessfule: " + isSuccessful + " - Error: " + errorText);
        }
        public virtual void verifyAndAuthMessageAPI(string apiData, string xyzChallenge)
        {
            Debug.WriteLine("verifyAndAuthMessageAPI: " + apiData + " " + xyzChallenge);
        }
        public virtual void verifyAndAuthCompleted(bool isSuccessful, string errorText)
        {
            Debug.WriteLine("verifyAndAuthCompleted. IsSuccessful: " + isSuccessful + " - Error: " + errorText);
        }

        public virtual void displayGroupList(int reqId, string groups)
        {
            Debug.WriteLine("DisplayGroupList. Request: " + reqId + ", Groups" + groups);
        }

        public virtual void displayGroupUpdated(int reqId, string contractInfo)
        {
            Debug.WriteLine("displayGroupUpdated. Request: " + reqId + ", ContractInfo: " + contractInfo);
        }

        public virtual void positionMulti(int reqId, string account, string modelCode, Contract contract, double pos, double avgCost)
        {
            Debug.WriteLine("Position Multi. Request: " + reqId + ", Account: " + account + ", ModelCode: " + modelCode + ", Symbol: " + contract.Symbol + ", SecType: " + contract.SecType + ", Currency: " + contract.Currency + ", Position: " + pos + ", Avg cost: " + avgCost + "\n");
        }

        public virtual void positionMultiEnd(int reqId)
        {
            Debug.WriteLine("Position Multi End. Request: " + reqId + "\n");
        }

        public virtual void accountUpdateMulti(int reqId, string account, string modelCode, string key, string value, string currency)
        {
            Debug.WriteLine("Account Update Multi. Request: " + reqId + ", Account: " + account + ", ModelCode: " + modelCode + ", Key: " + key + ", Value: " + value + ", Currency: " + currency + "\n");
        }

        public virtual void accountUpdateMultiEnd(int reqId)
        {
            Debug.WriteLine("Account Update Multi End. Request: " + reqId + "\n");
        }

        public void securityDefinitionOptionParameter(int reqId, string exchange, int underlyingConId, string tradingClass, string multiplier, HashSet<string> expirations, HashSet<double> strikes)
        {
            Debug.WriteLine("Security Definition Option Parameter. Reqest: {0}, Exchange: {1}, Undrelying contract id: {2}, Trading class: {3}, Multiplier: {4}, Expirations: {5}, Strikes: {6}",
                              reqId, exchange, underlyingConId, tradingClass, multiplier, string.Join(", ", expirations), string.Join(", ", strikes));
        }

        public void securityDefinitionOptionParameterEnd(int reqId)
        {
            Debug.WriteLine("Security Definition Option Parameter End. Request: " + reqId + "\n");
        }

        public void softDollarTiers(int reqId, SoftDollarTier[] tiers)
        {
            Debug.WriteLine("Soft Dollar Tiers:");

            foreach (var tier in tiers)
            {
                Debug.WriteLine(tier.DisplayName);
            }
        }

        public void familyCodes(FamilyCode[] familyCodes)
        {
            Debug.WriteLine("Family Codes:");

            foreach (var familyCode in familyCodes)
            {
                Debug.WriteLine("Account ID: {0}, Family Code Str: {1}", familyCode.AccountID, familyCode.FamilyCodeStr);
            }
        }

        public void symbolSamples(int reqId, ContractDescription[] contractDescriptions)
        {
            string derivSecTypes;
            Debug.WriteLine("Symbol Samples. Request Id: {0}", reqId);

            foreach (var contractDescription in contractDescriptions)
            {
                derivSecTypes = "";
                foreach (var derivSecType in contractDescription.DerivativeSecTypes)
                {
                    derivSecTypes += derivSecType;
                    derivSecTypes += " ";
                }
                Debug.WriteLine("Contract: conId - {0}, symbol - {1}, secType - {2}, primExchange - {3}, currency - {4}, derivativeSecTypes - {5}",
                    contractDescription.Contract.ConId, contractDescription.Contract.Symbol, contractDescription.Contract.SecType,
                    contractDescription.Contract.PrimaryExch, contractDescription.Contract.Currency, derivSecTypes);
            }
        }

        public void mktDepthExchanges(DepthMktDataDescription[] depthMktDataDescriptions)
        {
            Debug.WriteLine("Market Depth Exchanges:");

            foreach (var depthMktDataDescription in depthMktDataDescriptions)
            {
                Debug.WriteLine("Depth Market Data Description: Exchange: {0}, Security Type: {1}, Listing Exch: {2}, Service Data Type: {3}, Agg Group: {4}",
                    depthMktDataDescription.Exchange, depthMktDataDescription.SecType,
                    depthMktDataDescription.ListingExch, depthMktDataDescription.ServiceDataType,
                    depthMktDataDescription.AggGroup != Int32.MaxValue ? depthMktDataDescription.AggGroup.ToString() : ""
                    );
            }
        }

        public void tickNews(int tickerId, long timeStamp, string providerCode, string articleId, string headline, string extraData)
        {
            Debug.WriteLine("Tick News. Ticker Id: {0}, Time Stamp: {1}, Provider Code: {2}, Article Id: {3}, headline: {4}, extraData: {5}", tickerId, timeStamp, providerCode, articleId, headline, extraData);
        }

        public void smartComponents(int reqId, Dictionary<int, KeyValuePair<string, char>> theMap)
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendFormat("==== Smart Components Begin (total={0}) reqId = {1} ====\n", theMap.Count, reqId);

            foreach (var item in theMap)
            {
                sb.AppendFormat("bit number: {0}, exchange: {1}, exchange letter: {2}\n", item.Key, item.Value.Key, item.Value.Value);
            }

            sb.AppendFormat("==== Smart Components Begin (total={0}) reqId = {1} ====\n", theMap.Count, reqId);

            Debug.WriteLine(sb);
        }

        public void tickReqParams(int tickerId, double minTick, string bboExchange, int snapshotPermissions)
        {
            Debug.WriteLine("id={0} minTick = {1} bboExchange = {2} snapshotPermissions = {3}", tickerId, minTick, bboExchange, snapshotPermissions);

            BboExchange = bboExchange;
        }

        public void newsProviders(NewsProvider[] newsProviders)
        {
            Debug.WriteLine("News Providers:");

            foreach (var newsProvider in newsProviders)
            {
                Debug.WriteLine("News provider: providerCode - {0}, providerName - {1}",
                    newsProvider.ProviderCode, newsProvider.ProviderName);
            }
        }

        public void newsArticle(int requestId, int articleType, string articleText)
        {
            Debug.WriteLine("News Article. Request Id: {0}, ArticleType: {1}", requestId, articleType);
            if (articleType == 0)
            {
                Debug.WriteLine("News Article Text: {0}", articleText);
            }
            else if (articleType == 1)
            {
                Debug.WriteLine("News Article Text: article text is binary/pdf and cannot be displayed");
            }
        }

        public void historicalNews(int requestId, string time, string providerCode, string articleId, string headline)
        {
            Debug.WriteLine("Historical News. Request Id: {0}, Time: {1}, Provider Code: {2}, Article Id: {3}, headline: {4}", requestId, time, providerCode, articleId, headline);
        }

        public void historicalNewsEnd(int requestId, bool hasMore)
        {
            Debug.WriteLine("Historical News End. Request Id: {0}, Has More: {1}", requestId, hasMore);
        }

        public void headTimestamp(int reqId, string headTimestamp)
        {
            Debug.WriteLine("Head time stamp. Request Id: {0}, Head time stamp: {1}", reqId, headTimestamp);
        }

        public void histogramData(int reqId, HistogramEntry[] data)
        {
            Debug.WriteLine("Histogram data. Request Id: {0}, data size: {1}", reqId, data.Length);
            data.ToList().ForEach(i => Debug.WriteLine("\tPrice: {0}, Size: {1}", i.Price, i.Size));
        }

        public void historicalDataUpdate(int reqId, Bar bar)
        {
            Debug.WriteLine("HistoricalDataUpdate. " + reqId + " - Time: " + bar.Time + ", Open: " + bar.Open + ", High: " + bar.High + ", Low: " + bar.Low + ", Close: " + bar.Close + ", Volume: " + bar.Volume + ", Count: " + bar.Count + ", WAP: " + bar.WAP);
        }

        public void rerouteMktDataReq(int reqId, int conId, string exchange)
        {
            Debug.WriteLine("Re-route market data request. Req Id: {0}, ConId: {1}, Exchange: {2}", reqId, conId, exchange);
        }

        public void rerouteMktDepthReq(int reqId, int conId, string exchange)
        {
            Debug.WriteLine("Re-route market depth request. Req Id: {0}, ConId: {1}, Exchange: {2}", reqId, conId, exchange);
        }

        public void marketRule(int marketRuleId, PriceIncrement[] priceIncrements)
        {
            Debug.WriteLine("Market Rule Id: " + marketRuleId);
            foreach (var priceIncrement in priceIncrements)
            {
                Debug.WriteLine("Low Edge: {0}, Increment: {1}", ((decimal)priceIncrement.LowEdge).ToString(), ((decimal)priceIncrement.Increment).ToString());
            }
        }

        public void pnl(int reqId, double dailyPnL, double unrealizedPnL, double realizedPnL)
        {
            Debug.WriteLine("PnL. Request Id: {0}, Daily PnL: {1}, Unrealized PnL: {2}, Realized PnL: {3}", reqId, dailyPnL, unrealizedPnL, realizedPnL);
        }

        public void pnlSingle(int reqId, int pos, double dailyPnL, double unrealizedPnL, double realizedPnL, double value)
        {
            Debug.WriteLine("PnL Single. Request Id: {0}, Pos {1}, Daily PnL {2}, Unrealized PnL {3}, Realized PnL: {4}, Value: {5}", reqId, pos, dailyPnL, unrealizedPnL, realizedPnL, value);
        }

        public void historicalTicks(int reqId, HistoricalTick[] ticks, bool done)
        {
            foreach (var tick in ticks)
            {
                Debug.WriteLine("Historical Tick. Request Id: {0}, Time: {1}, Price: {2}, Size: {3}", reqId, Util.UnixSecondsToString(tick.Time, "yyyyMMdd-HH:mm:ss zzz"), tick.Price, tick.Size);
            }
        }

        public void historicalTicksBidAsk(int reqId, HistoricalTickBidAsk[] ticks, bool done)
        {
            foreach (var tick in ticks)
            {
                Debug.WriteLine("Historical Tick Bid/Ask. Request Id: {0}, Time: {1}, Price Bid: {2}, Price Ask: {3}, Size Bid: {4}, Size Ask: {5}, Bid/Ask Tick Attribs: {6} ",
                    reqId, Util.UnixSecondsToString(tick.Time, "yyyyMMdd-HH:mm:ss zzz"), tick.PriceBid, tick.PriceAsk, tick.SizeBid, tick.SizeAsk, tick.TickAttribBidAsk.toString());
            }
        }

        public void historicalTicksLast(int reqId, HistoricalTickLast[] ticks, bool done)
        {
            foreach (var tick in ticks)
            {
                Debug.WriteLine("Historical Tick Last. Request Id: {0}, Time: {1}, Price: {2}, Size: {3}, Exchange: {4}, Special Conditions: {5}, Last Tick Attribs: {6} ",
                    reqId, Util.UnixSecondsToString(tick.Time, "yyyyMMdd-HH:mm:ss zzz"), tick.Price, tick.Size, tick.Exchange, tick.SpecialConditions, tick.TickAttribLast.toString());
            }
        }

        public void tickByTickAllLast(int reqId, int tickType, long time, double price, int size, TickAttribLast tickAttribLast, string exchange, string specialConditions)
        {
            Debug.WriteLine("Tick-By-Tick. Request Id: {0}, TickType: {1}, Time: {2}, Price: {3}, Size: {4}, Exchange: {5}, Special Conditions: {6}, PastLimit: {7}, Unreported: {8}",
                reqId, tickType == 1 ? "Last" : "AllLast", Util.UnixSecondsToString(time, "yyyyMMdd-HH:mm:ss zzz"), price, size, exchange, specialConditions, tickAttribLast.PastLimit, tickAttribLast.Unreported);
        }

        public void tickByTickBidAsk(int reqId, long time, double bidPrice, double askPrice, int bidSize, int askSize, TickAttribBidAsk tickAttribBidAsk)
        {
            Debug.WriteLine("Tick-By-Tick. Request Id: {0}, TickType: BidAsk, Time: {1}, BidPrice: {2}, AskPrice: {3}, BidSize: {4}, AskSize: {5}, BidPastLow: {6}, AskPastHigh: {7}",
                reqId, Util.UnixSecondsToString(time, "yyyyMMdd-HH:mm:ss zzz"), bidPrice, askPrice, bidSize, askSize, tickAttribBidAsk.BidPastLow, tickAttribBidAsk.AskPastHigh);
        }

        public void tickByTickMidPoint(int reqId, long time, double midPoint)
        {
            Debug.WriteLine("Tick-By-Tick. Request Id: {0}, TickType: MidPoint, Time: {1}, MidPoint: {2}",
                reqId, Util.UnixSecondsToString(time, "yyyyMMdd-HH:mm:ss zzz"), midPoint);
        }

        public void orderBound(long orderId, int apiClientId, int apiOrderId)
        {
            Debug.WriteLine("Order bound. Order Id: {0}, Api Client Id: {1}, Api Order Id: {2}", orderId, apiClientId, apiOrderId);
        }
    }
}
