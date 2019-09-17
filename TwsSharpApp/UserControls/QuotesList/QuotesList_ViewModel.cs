using IBApi;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Threading;

namespace TwsSharpApp
{
    class QuotesList_ViewModel : Workspace_ViewModel
    {
        public ListCollectionView QuotesListView { get; set; }
        public ObservableCollection<Quote_ViewModel> QuotesList { get; set; } = new ObservableCollection<Quote_ViewModel>();

        private Dictionary<int, Quote_ViewModel> SymbolsList = new Dictionary<int, Quote_ViewModel>();

        public static string MyName = "RT Quotes";

        public Dispatcher Dispatcher { get; set; }

        public QuotesList_ViewModel()
        {
            DisplayName = MyName;

            QuotesListView = CollectionViewSource.GetDefaultView(this.QuotesList) as ListCollectionView;
            QuotesListView.IsLiveSorting = true;
            QuotesListView.SortDescriptions.Add(new SortDescription("Symbol", ListSortDirection.Descending));

            IsTabSelected = true;
            CanClose = false;

            Main_ViewModel.DataFeeder.SocketConnected_Event += DataFeeder_SocketConnected_Event;

            AddSymbol_VM.ContractSelected_Event += AddSymbol_VM_ContractSelected_Event;
        }

        ~QuotesList_ViewModel()
        {
            Main_ViewModel.DataFeeder.RealTimeDataEndReceived_Event -= DataFeeder_RealTimeDataEndReceived_Event;
            Main_ViewModel.DataFeeder.HistoricalDataReceived_Event  -= DataFeeder_HistoricalDataReceived_Event;

            Main_ViewModel.DataFeeder.SocketConnected_Event -= DataFeeder_SocketConnected_Event;
            AddSymbol_VM.ContractSelected_Event -= AddSymbol_VM_ContractSelected_Event;        
        }

        //
        // Called from TWS when the connection with the gateway is established
        //
        private void DataFeeder_SocketConnected_Event(object sender, SocketConnected_EventArgs e)
        {
            if (Main_ViewModel.DataFeeder.IsConnected)
            {
                Main_ViewModel.DataFeeder.RealTimeDataEndReceived_Event += DataFeeder_RealTimeDataEndReceived_Event;
                Main_ViewModel.DataFeeder.HistoricalDataReceived_Event  += DataFeeder_HistoricalDataReceived_Event;
            }
        }

        private readonly object symbolsList_Lock = new object();

        // 
        // Called from AddSymbol_ViewModel when a contract was selected from list, will add it to the listview
        // then will subscribe to receive TWS real time (5s) data.
        //
        private async void AddSymbol_VM_ContractSelected_Event(object sender, ContractDetailsRecv_EventArgs e)
        {
            int reqId = 0;

            Quote_ViewModel symVM = QuotesList.FirstOrDefault(s => s.Symbol == e.ContractData.Contract.Symbol);
            
                if (symVM != null)
                {
                    // First cancel the old:
                    Main_ViewModel.DataFeeder.CancelRealTime(symVM.ReqId);
                    SymbolsList.Remove(symVM.ReqId);
                    QuotesList.Remove(symVM);
                }
           
                // subscribe to receive historical data events then send request receive latest 2 days of daily data for symbol: 
                reqId = await Main_ViewModel.DataFeeder.RequestPrev2Closes(e.ContractData.Contract);

                lock(symbolsList_Lock)
                { 
                    Dispatcher.Invoke(() =>
                    {
                        symVM = addNewQuote(reqId, e.ContractData);
                        if(symVM != null) SymbolsList.Add(reqId, symVM);
                    });
                }

                // subscribe to receive new real time events then send real time request for symbol:            
                reqId = await Main_ViewModel.DataFeeder.StartRealtime(e.ContractData.Contract);  
                lock(symbolsList_Lock)
                { 
                    Dispatcher.Invoke(() =>
                    {
                        if(symVM != null) SymbolsList.Add(reqId, symVM);
                    });
                }      
        }

        //
        // Called from TWS when historical data (previous close) has been receined for a contract.
        //
        private async void DataFeeder_HistoricalDataReceived_Event(object sender, HistoricalRecv_EventArgs e)
        {
            // Historical data list is empty, just return:
            lock(symbolsList_Lock)
            { 
                if (e.HistoricalList == null || !SymbolsList.ContainsKey(e.RequestId)) return;
            }

            CultureInfo provider = CultureInfo.InvariantCulture;
            int reqId = e.RequestId;

            // Identify the contract by the request ID
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
                symbVM.PrevClose = e.HistoricalList[0].Close;
                symbVM.LowValue  = e.HistoricalList[1].Low;
                symbVM.HighValue = e.HistoricalList[1].High;
                symbVM.Latest    = e.HistoricalList[1].Close;
                
                time = DateTime.ParseExact(e.HistoricalList[1].Time, "yyyyMMdd", provider);
            }

            symbVM.Time = time.ToShortDateString();
            await Task.CompletedTask;
        }

        //
        // Called from TWS when realtime bar has been receined for a contract.
        //
        private void DataFeeder_RealTimeDataEndReceived_Event(object sender, RealtimeBarRecv_EventArgs e)
        {
            // a real time bar has been received:
            int reqId = e.RequestId;
            Quote_ViewModel symbVM;
            
            lock(symbolsList_Lock) { symbVM = SymbolsList[reqId]; }

            DateTime time = DateTime.Parse(e.RealtimeBar.Time);

            symbVM.LowValue  = e.RealtimeBar.Low;
            symbVM.HighValue = e.RealtimeBar.High;
            symbVM.Latest    = e.RealtimeBar.Close;
            symbVM.Time      = time.ToString("HH:mm:ss");
        }

        //
        // New contract details is added to the listview' ItemsSource
        //
        private Quote_ViewModel addNewQuote(int reqId, ContractDetails cDetails)
        {
            Quote_ViewModel dq = QuotesList.Where(q => q.Symbol == cDetails.Contract.Symbol).FirstOrDefault();

            if(dq == null)
            {
                Quote_ViewModel qvm = new Quote_ViewModel(reqId, cDetails);
                QuotesList.Add(qvm);
                return qvm;
            }
            return null;
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

        //
        // This is the ViewModel for the Add New Symbol user control
        //
        private AddSymbol_ViewModel addSymbol_VM = null;
        public  AddSymbol_ViewModel AddSymbol_VM
        {
            get
            {
                if (addSymbol_VM == null)
                {
                    addSymbol_VM = new AddSymbol_ViewModel();
                }

                return addSymbol_VM;
            }
        }

        private RelayCommand showAddSymbolCommand;
        public  RelayCommand ShowAddSymbolCommand
        {
            get
            {
                // the command to show the Add New Symbol UserControl is disabled when the UserControl is Visible
                return showAddSymbolCommand ?? (showAddSymbolCommand = new RelayCommand(param => this.ShowAddSymbol(), param => !this.AddSymbol_VM.IsVisible));
            }
        }

        private void ShowAddSymbol()
        {
            AddSymbol_VM.IsVisible = true;
        }
    }
}
