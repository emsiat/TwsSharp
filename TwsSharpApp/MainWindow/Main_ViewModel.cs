using IBApi;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TwsSharpApp
{
    public class Main_ViewModel : Base_ViewModel
    {
        TwsWrapper dataFeeder;

        public Main_ViewModel()
        {
            dataFeeder = new TwsWrapper();

            // Subscribe to receive errors and to get notified when socket is connected:
            dataFeeder.ErrorReceived_Event   += DataFeeder_ErrorReceived_Event;
            dataFeeder.SocketConnected_Event += DataFeeder_SocketConnected_Event;
        }

        public void Start()
        {
              // Open connection to TWS:
              dataFeeder.REQ_OpenSocket();
        }

        private void DataFeeder_SocketConnected_Event(object sender, SocketConnected_EventArgs e)
        {
            if (dataFeeder.IsConnected)
            {
                // subscribe to receive new real time events then send real time request for symbol: 
                dataFeeder.RealTimeDataEndReceived_Event += DataFeeder_RealTimeDataEndReceived_Event;
                Task.Run(() => dataFeeder.StartRealtime("MSFT"));

                // subscribe to receive historical data events then send request receive latest 2 days of daily data for symbol: 
                dataFeeder.HistoricalDataReceived_Event  += DataFeeder_HistoricalDataReceived_Event;
                Task.Run(() => dataFeeder.PrevClose("MSFT"));
            }
        }

        private void DataFeeder_ErrorReceived_Event(object sender, ErrorRecv_EventArgs e)
        {
            // Update Error property when an error is received:
            Error = "Error. Id: " + e.Error.id + ", Code: " + e.Error.errorCode + ", Msg: " + e.Error.errorMsg + "\n" + error;
        }

        private void DataFeeder_HistoricalDataReceived_Event(object sender, HistoricalRecv_EventArgs e)
        {
            // Historical data list is empty, just return:
            if (e.HistoricalList == null) return;
            
            // e.HistoricalList is supposed to be ascending sorted by time:
            // Latest day is at index 1
            //if(e.HistoricalList[1].Time == DateTime.Today.ToString("yyyyMMdd"))
            //{
            if(e.HistoricalList.Count == 1)
            {
                PrevClose = e.HistoricalList[0].Open;
                LowValue  = e.HistoricalList[0].Low;
                HighValue = e.HistoricalList[0].High;
                Latest    = e.HistoricalList[0].Close;
            }
            else if (e.HistoricalList.Count >= 2)
            {
                PrevClose = e.HistoricalList[0].Close; // PrevClose is the 
                LowValue  = e.HistoricalList[1].Low;
                HighValue = e.HistoricalList[1].High;
                Latest    = e.HistoricalList[1].Close;
            }
            //}
        }

        private void DataFeeder_RealTimeDataEndReceived_Event(object sender, RealtimeBarRecv_EventArgs e)
        {
            // a real time bar has been received:
            LowValue  = e.RealtimeBar.Low;
            HighValue = e.RealtimeBar.High;
            Latest    = e.RealtimeBar.Close;
            Time      = e.RealtimeBar.Time;
        }

        public void Close()
        {
            // close the socket (called at exit):
            dataFeeder.CloseSocket();
        }

        private double prevClose = 0;
        public  double PrevClose
        {
            get { return prevClose; }
            set 
            {
                if (prevClose == value) return;
                prevClose = value;
                OnPropertyChanged(nameof(PrevClose));
            }
        }

        private double lowValue = double.MaxValue;
        public  double LowValue
        {
            get { return lowValue; }
            set
            {
                if (lowValue <= value) return;
                lowValue = value;
                OnPropertyChanged(nameof(LowValue));
            }
        }

        private double highValue = 0;
        public  double HighValue
        {
            get { return highValue; }
            set
            {
                if (highValue >= value) return;
                highValue = value;
                OnPropertyChanged(nameof(HighValue));
            }
        }

        private double latest = 0;
        public  double Latest
        {
            get { return latest; }
            set
            {
                if (latest == value) return;
                latest = value;
                OnPropertyChanged(nameof(Latest));

                // Also bind the Var property:
                OnPropertyChanged(nameof(Var));
            }
        }

        public  double Var
        {
            get { return Latest - PrevClose; }
        }

        private string time;
        public  string Time
        {
            get { return time; }
            set 
            {
                if (time == value) return;
                time = value;
                OnPropertyChanged(nameof(Time));
            }
        }

        private string error;
        public  string Error
        {
            get { return error; }
            set 
            {
                if (error == value) return;
                error = value;
                OnPropertyChanged(nameof(Error));
            }
        }
    }
}
