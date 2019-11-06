using IBApi;
using System;
using System.Threading.Tasks;

namespace TwsSharpApp
{
    public enum ReturnedCodes : int
    {
        None = 0,
        HistoricalDataEndReceived,
        BeginOfErrors,
        HistoricalNoDataReceived,
        BadSymbol,
        ConnectionLost
    }

    public enum UseRegularTradingHours : int
    {
        No = 0,
        Yes
    }

    public partial class TwsWrapper : EWrapper
    {
#region Historical Data
        //
        // Historical Data
        //

        protected event EventHandler<RequestId_EventArgs>          HistoricalDataEndReceived_Event;
        protected event EventHandler<HistoricalBarRecv_EventArgs>  HistoricalBarReceived_Event;

        public int RequestPreviousCloses(Contract contract, int days)
        {
            string queryTime = DateTime.Now.ToString("yyyyMMdd HH:mm:ss");

            int reqId = nextValidId();
            ClientSocket.reqHistoricalData(reqId, contract, queryTime,
                                           days.ToString() + " D", "1 day", "TRADES",
                                           (int)UseRegularTradingHours.Yes, 1, false, null);

            return reqId;
        }

        public void CancelHistoricalData(int reqId)
        {
            ClientSocket.cancelHistoricalData(reqId);
        }

        public void historicalData(int reqId, Bar bar)
        {
            HistoricalBarReceived_Event?.Invoke(this, new HistoricalBarRecv_EventArgs(reqId, bar));
        }

        public void historicalDataEnd(int reqId, string startDate, string endDate)
        {
            HistoricalDataEndReceived_Event?.Invoke(this, new RequestId_EventArgs(reqId));
        }

#endregion

#region Real time data
        //
        // Real time data:
        //

        public event EventHandler<RealtimeBarRecv_EventArgs> RealTimeDataReceived_Event;

        public async Task<int> RequestRealTime(Contract contract)
        {
            int reqId = nextValidId();

            ClientSocket.reqRealTimeBars(reqId, contract, 1, "TRADES", true, null);
            await Task.CompletedTask;

            return reqId;
        }

        public void CancelRealTime(int tickerId)
        {
            ClientSocket.cancelRealTimeBars(tickerId);
        }

        public void realtimeBar(int reqId, long time, double open, double high, double low, double close, long volume, double WAP, int count)
        {
            Bar recvBar = new Bar(UnixSecondsToEST(time, "o"/*"HH:mm:ss zzz"*/), open, high, low, close, volume, count, WAP);
            RealTimeDataReceived_Event?.Invoke(this, new RealtimeBarRecv_EventArgs(reqId, recvBar));
        }

        private static TimeZoneInfo tst = TimeZoneInfo.FindSystemTimeZoneById("Eastern Standard Time");
        private DateTime firstUnixTime = new DateTime(1970, 1, 1, 0, 0, 0);

        public string UnixSecondsToEST(long seconds, string format)
        {
            DateTime tstTime = TimeZoneInfo.ConvertTime(firstUnixTime.AddSeconds(seconds), TimeZoneInfo.Utc, tst);
            return tstTime.ToString();
        }
#endregion
    }
}