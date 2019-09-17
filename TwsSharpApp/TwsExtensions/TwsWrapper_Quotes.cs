using IBApi;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
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

    public partial class TwsWrapper : EWrapper
    {
        private Dictionary<int, List<Bar>> historicData = new Dictionary<int, List<Bar>>();

        //
        // Historical Data
        //
#region Historical Data
        public event EventHandler<HistoricalRecv_EventArgs> HistoricalDataReceived_Event;

        public void historicalData(int reqId, Bar bar)
        {
            if(historicData.ContainsKey(reqId) == false)
            {
                historicData.Add(reqId, new List<Bar>());
            }

            historicData[reqId].Add(bar);
        }

        public void CancelHistoricalData(int reqId)
        {
            ClientSocket.cancelHistoricalData(reqId);
        }

        public void historicalDataEnd(int reqId, string startDate, string endDate)
        {
            HistoricalDataReceived_Event?.Invoke(this, new HistoricalRecv_EventArgs(reqId, historicData[reqId]));
            historicData.Remove(reqId);
        }

        public async Task<int> RequestPrev2Closes(Contract contract)
        {
            string queryTime = DateTime.Today.AddDays(1).ToString("yyyyMMdd HH:mm:ss");

            ClientSocket.reqHistoricalData(nextValidId(), contract, queryTime, "2 D", "1 day", "TRADES", 1, 1, false, null);

            await Task.CompletedTask;
            return NextOrderId;
        }

        public async Task<int> RequestLatestClose(Contract contract)
        {
            string queryTime = DateTime.Today.AddDays(1).ToString("yyyyMMdd HH:mm:ss");

            ClientSocket.reqHistoricalData(nextValidId(), contract, queryTime, "1 D", "1 day", "TRADES", 1, 1, false, null);
            
            await Task.CompletedTask;
            return NextOrderId;
        }
#endregion

        //
        // Real time data:
        //
#region Real time data
        public async Task<int> StartRealtime(Contract contract)
        {
            ClientSocket.reqRealTimeBars(nextValidId(), contract, 1, "TRADES", true, null);
            await Task.CompletedTask;

            return NextOrderId;
        }

        public void CancelRealTime(int tickerId)
        {
            ClientSocket.cancelRealTimeBars(tickerId);
        }

        public event EventHandler<RealtimeBarRecv_EventArgs> RealTimeDataEndReceived_Event;

        public void realtimeBar(int reqId, long time, double open, double high, double low, double close, long volume, double WAP, int count)
        {
            Bar recvBar = new Bar(UnixSecondsToEST(time, "o"/*"HH:mm:ss zzz"*/), open, high, low, close, volume, count, WAP);
            RealTimeDataEndReceived_Event?.Invoke(this, new RealtimeBarRecv_EventArgs(reqId, recvBar));
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
