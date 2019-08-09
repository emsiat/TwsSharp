using IBApi;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
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
        public List<Bar> NewQuotesList = new List<Bar>();

        private static int nextOrderId;
        public ReturnedCodes TWSReturnedCode { get; set; } = ReturnedCodes.None;

        public TwsWrapper() : base()
        {
        }

        public void ClearAll()
        {
            NewQuotesList.Clear();
        }

        //
        // Historical Data
        //
#region Historical Data
        public event EventHandler<HistoricalRecv_EventArgs> HistoricalDataReceived_Event;

        public void historicalData(int reqId, Bar bar)
        {
            Debug.WriteLine("HistoricalData. " + reqId + " - Time: " + bar.Time + ", Open: " + bar.Open + ", High: " + bar.High + ", Low: " + bar.Low + ", Close: " + bar.Close + ", Volume: " + bar.Volume + ", Count: " + bar.Count + ", WAP: " + bar.WAP);

            TWSReturnedCode = ReturnedCodes.None;

            NewQuotesList.Add(bar);
        }

        public void historicalDataEnd(int reqId, string startDate, string endDate)
        {
            Debug.WriteLine("Historical data end - " + reqId + " from " + startDate + " to " + endDate);
            TWSReturnedCode = ReturnedCodes.HistoricalDataEndReceived;

            HistoricalDataReceived_Event?.Invoke(this, new HistoricalRecv_EventArgs(NewQuotesList));
        }

        public async Task PrevClose(string symbol)
        {
            string queryTime = DateTime.Today.AddDays(1).ToString("yyyyMMdd HH:mm:ss");
            NewQuotesList.Clear();
            ClientSocket.reqHistoricalData(nextValidId(), Contracts.USStock(symbol), queryTime, "2 D", "1 day", "TRADES", 1, 1, false, null);

            await Task.CompletedTask;
        }
#endregion

        //
        // Real time data:
        //
#region Real time data
        public async Task  StartRealtime(string symbol)
        {
            ClientSocket.reqRealTimeBars(nextValidId(), Contracts.USStock(symbol), 1, "TRADES", true, null);

            await Task.CompletedTask;
        }

        public event EventHandler<RealtimeBarRecv_EventArgs> RealTimeDataEndReceived_Event;

        public void realtimeBar(int reqId, long time, double open, double high, double low, double close, long volume, double WAP, int count)
        {
            Bar recvBar = new Bar(UnixSecondsToEST(time, "o"/*"HH:mm:ss zzz"*/), open, high, low, close, volume, count, WAP);
            RealTimeDataEndReceived_Event?.Invoke(this, new RealtimeBarRecv_EventArgs(recvBar));
        }

        private static TimeZoneInfo tst = TimeZoneInfo.FindSystemTimeZoneById("Eastern Standard Time");
        private DateTime firstUnixTime = new DateTime(1970, 1, 1, 0, 0, 0);

        public string UnixSecondsToEST(long seconds, string format)
        {
            DateTime tstTime = TimeZoneInfo.ConvertTime(firstUnixTime.AddSeconds(seconds), TimeZoneInfo.Utc, tst);
            return tstTime.ToString();
        }
        #endregion

        //
        // Handling Next Orderd ID
        //
#region Handling Next Orderd ID
        public int NextOrderId
        {
            get { return nextOrderId; }
            set { nextOrderId = value; }
        }

        private readonly object orderId_Lock = new object();
        public void nextValidId(int orderId)
        {
            Debug.WriteLine("Next Valid Id: " + orderId);
            lock (orderId_Lock) { NextOrderId = orderId; }
        }

        public int nextValidId()
        {
            lock (orderId_Lock) { ++NextOrderId; }

            return NextOrderId;
        }
#endregion

        //
        // Handling Client Socket
        //
#region Handling Client Socket and Signal

        public event EventHandler<SocketConnected_EventArgs> SocketConnected_Event;
        static EClientSocket clientSocket = null;
        public EClientSocket ClientSocket
        {
            get
            {
                if (clientSocket == null) clientSocket = new EClientSocket(this, Signal);
                return clientSocket;
            }
            set { clientSocket = value; }
        }

        public void REQ_OpenSocket()
        {
            if (ClientSocket.IsConnected()) return;

            try
            {
                EReaderSignal readerSignal = Signal;
                //! [connect]
                ClientSocket.eConnect("127.0.0.1", 7497, 0);
                //! [connect]
                //! [ereader]
                //Create a reader to consume messages from the TWS. The EReader will consume the incoming messages and put them in a queue
                var reader = new EReader(ClientSocket, readerSignal);
                reader.Start();
                //Once the messages are in the queue, an additional thread can be created to fetch them
                new Thread(() => { while (ClientSocket.IsConnected()) { readerSignal.waitForSignal(); reader.processMsgs(); } }) { IsBackground = true }.Start();
                //! [ereader]
                /*************************************************************************************************************************************************/
                /* One (although primitive) way of knowing if we can proceed is by monitoring the order's nextValidId reception which comes down automatically after connecting. */
                /*************************************************************************************************************************************************/
                while (NextOrderId <= 0) {/*await Task.Delay(1);*/ }
            }
            catch(Exception ex)
            {
                error(ex.Message);
            }
            finally
            {
                SocketConnected_Event?.Invoke(this, new SocketConnected_EventArgs(ClientSocket.IsConnected()));
            }

            //await Task.Delay(30000);
            //await Task.CompletedTask;
        }

        public void CloseSocket()
        {
            if (ClientSocket.IsConnected()) ClientSocket.eDisconnect();
        }

        public bool IsConnected { get { return ClientSocket.IsConnected(); } }

        public void connectAck()
        {
            if (ClientSocket.AsyncEConnect)
                ClientSocket.startApi();
        }

        static EReaderSignal signal;
        public EReaderSignal Signal
        {
            get
            {
                if (signal == null) signal = new EReaderMonitorSignal();
                return signal;
            }
        }
#endregion

        public event EventHandler<ErrorRecv_EventArgs> ErrorReceived_Event;

        public virtual void error(string str)
        {
            Debug.WriteLine("Error: " + str + "\n");
            ErrorReceived_Event?.Invoke(this, new ErrorRecv_EventArgs(new TwsError(str)));
        }

        public virtual void error(int id, int errorCode, string errorMsg)
        {
            Debug.WriteLine("Error. Id: " + id + ", Code: " + errorCode + ", Msg: " + errorMsg + "\n");
            ErrorReceived_Event?.Invoke(this, new ErrorRecv_EventArgs(new TwsError(id, errorCode, errorMsg)));
        }
    }
}
