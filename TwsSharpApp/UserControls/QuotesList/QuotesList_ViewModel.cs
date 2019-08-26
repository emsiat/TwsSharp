using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Data;

namespace TwsSharpApp
{
    class QuotesList_ViewModel : Workspace_ViewModel
    {
        public ListCollectionView QuotesListView { get; set; }
        public ObservableCollection<Quote_ViewModel> QuotesList { get; set; } = new ObservableCollection<Quote_ViewModel>();

        private Dictionary<int, Quote_ViewModel> SymbolsList = new Dictionary<int, Quote_ViewModel>();

        public static string MyName = "RT Quotes";

        public QuotesList_ViewModel()
        {
            DisplayName = MyName;

            QuotesListView = CollectionViewSource.GetDefaultView(this.QuotesList) as ListCollectionView;
            QuotesListView.IsLiveSorting = true;
            QuotesListView.SortDescriptions.Add(new SortDescription("Var", ListSortDirection.Descending));

            IsTabSelected = true;
            CanClose = false;

            Main_ViewModel.DataFeeder.SocketConnected_Event += DataFeeder_SocketConnected_Event;
        }

        private async void DataFeeder_SocketConnected_Event(object sender, SocketConnected_EventArgs e)
        {
            if (Main_ViewModel.DataFeeder.IsConnected)
            {
                Main_ViewModel.DataFeeder.RealTimeDataEndReceived_Event += DataFeeder_RealTimeDataEndReceived_Event;
                Main_ViewModel.DataFeeder.HistoricalDataReceived_Event  += DataFeeder_HistoricalDataReceived_Event;
                Main_ViewModel.DataFeeder.ContractDetailsReceived_Event += DataFeeder_ContractDetailsReceived_Event;

                foreach(Quote_ViewModel smbVM in QuotesList)
                {
                    await Main_ViewModel.DataFeeder.GetStockContract(smbVM.Symbol);
                    await Task.Delay(1000);
                }
            }
        }

        private async void DataFeeder_ContractDetailsReceived_Event(object sender, ContractDetailsRecv_EventArgs e)
        {
            int reqId = 0;

            Quote_ViewModel symVM = QuotesList.FirstOrDefault(s => s.Symbol == e.ContractData.Contract.Symbol);

            if(symVM != null)
            {
                // subscribe to receive new real time events then send real time request for symbol:            
                reqId = await Main_ViewModel.DataFeeder.StartRealtime(e.ContractData.Contract);
                SymbolsList.Add(reqId, symVM);

                // subscribe to receive historical data events then send request receive latest 2 days of daily data for symbol: 
                reqId = await Main_ViewModel.DataFeeder.PrevClose(e.ContractData.Contract);
                SymbolsList.Add(reqId, symVM);
            }
        }

        private async void DataFeeder_HistoricalDataReceived_Event(object sender, HistoricalRecv_EventArgs e)
        {
            // Historical data list is empty, just return:
            if (e.HistoricalList == null) return;

            CultureInfo provider = CultureInfo.InvariantCulture;
            int reqId = e.RequestId;
            Quote_ViewModel symbVM = SymbolsList[reqId];
            DateTime time = DateTime.Now;

            if(e.HistoricalList.Count == 1)
            {
                symbVM.PrevClose = e.HistoricalList[0].Open;
                symbVM.LowValue  = e.HistoricalList[0].Low;
                symbVM.HighValue = e.HistoricalList[0].High;
                symbVM.Latest    = e.HistoricalList[0].Close;

                time = DateTime.ParseExact(e.HistoricalList[0].Time, "yyyyMMdd", provider);
            }
            else if (e.HistoricalList.Count >= 2)
            {
                symbVM.PrevClose = e.HistoricalList[0].Close; // PrevClose is the 
                symbVM.LowValue  = e.HistoricalList[1].Low;
                symbVM.HighValue = e.HistoricalList[1].High;
                symbVM.Latest    = e.HistoricalList[1].Close;
                
                time = DateTime.ParseExact(e.HistoricalList[1].Time, "yyyyMMdd", provider);
            }

            symbVM.Time = time.ToShortDateString();

            //bool isThick = false;
            //while(true)
            //{
            //    await Task.Delay(5000);
            //    if(isThick)
            //        --symbVM.Latest;
            //    else
            //        ++symbVM.Latest;

            //    isThick = !isThick;
            //}
            await Task.CompletedTask;
        }

        private void DataFeeder_RealTimeDataEndReceived_Event(object sender, RealtimeBarRecv_EventArgs e)
        {
            // a real time bar has been received:
            int reqId = e.RequestId;
            Quote_ViewModel symbVM = SymbolsList[reqId];
            DateTime time = DateTime.Parse(e.RealtimeBar.Time);

            symbVM.LowValue  = e.RealtimeBar.Low;
            symbVM.HighValue = e.RealtimeBar.High;
            symbVM.Latest    = e.RealtimeBar.Close;
            symbVM.Time      = time.ToString("HH:mm:ss");
        }

        public void AddNewQuote(string symbol)
        {
            Quote_ViewModel dq = QuotesList.Where(q => q.Symbol == symbol).FirstOrDefault();

            if(dq == null)
            {
                QuotesList.Add(new Quote_ViewModel(symbol));
            }
        }

        private DateTime refDate;
        public  DateTime RefDate
        {
            get {  return refDate;}
            set
            {
                if(value == refDate) return;
                refDate = value;
                OnPropertyChanged(nameof(RefDate));
            }
        }

        private double itemWidth = 0;
        public  double ItemWidth
        {
            get {  return itemWidth; }
            set
            {
                if(itemWidth == value) return;
                itemWidth = value;
                OnPropertyChanged(nameof(ItemWidth));
            }
        }

        private double height = 0;
        private double width  = 0;
        private static double maxWidth = 150;

        public void ChangeDimensions(double h, double w)
        {
            if(h > height) itemWidth = maxWidth;
            height = h;
            width  = w;
            
            int slotsWidth  = (int)Math.Floor(width  / itemWidth); 
            int slotsHeight = (int)Math.Floor(height / itemWidth);

            while (QuotesList.Count > slotsWidth * slotsHeight)
            {
                itemWidth--;
                slotsWidth  = (int)Math.Floor(width  / itemWidth); 
                slotsHeight = (int)Math.Floor(height / itemWidth);
            }  

            OnPropertyChanged(nameof(ItemWidth));
        }

        private bool showGrid;
        public  bool ShowGrid
        {
            get { return showGrid;}
            set
            {
                if(value == showGrid) return;
                showGrid = value;
                OnPropertyChanged(nameof(ShowGrid));

                QuotesListView.SortDescriptions.Clear();
                QuotesListView.SortDescriptions.Add(new SortDescription("Var", ListSortDirection.Descending));
            }
        }
    }
}
