using IBApi;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Threading;
using TwsSharpApp.Data;

namespace TwsSharpApp
{
    class QuotesList_ViewModel : Workspace_ViewModel
    {
        public ListCollectionView QuotesListView { get; set; }
        public ObservableCollection<Quote_ViewModel> QuotesList { get; set; } = new ObservableCollection<Quote_ViewModel>();

        private Dictionary<int, Quote_ViewModel> SymbolsList = new Dictionary<int, Quote_ViewModel>();

        public static string MyName = "RT Quotes";

        private readonly object symbolsList_Lock = new object();
        private Dispatcher      dispatcher { get; set; }

        SettingsList settingsList = null;

        private static QuotesList_ViewModel instance = null;
        public  static QuotesList_ViewModel Instance
        {
            get
            {
                return instance ?? throw new ArgumentNullException(nameof(instance), "Instance cannot be null");
            }
        }

        public  static QuotesList_ViewModel CreateNew(Dispatcher dspchr)
        {
            instance = new QuotesList_ViewModel(dspchr);
            return instance;
        }

        private QuotesList_ViewModel(Dispatcher dspchr)
        {
            dispatcher  = dspchr;
            DisplayName = MyName;

            QuotesListView = CollectionViewSource.GetDefaultView(this.QuotesList) as ListCollectionView;
            QuotesListView.IsLiveSorting = true;
            QuotesListView.SortDescriptions.Add(new SortDescription("VarPercent", ListSortDirection.Descending));

            IsTabSelected = true;
            CanClose = false;

            TwsData.DataFeeder.RealTimeDataReceived_Event += DataFeeder_RealTimeDataEndReceived_Event;
            TwsData.DataFeeder.ConnectionClosed_Event     += DataFeeder_ConnectionClosed_Event;
            AddSymbol_VM.ContractSelected_Event           += AddSymbol_VM_ContractSelected_Event;
            Quote_ViewModel.ContractRemoved_Event         += Quote_ViewModel_ContractRemoved_Event;
        }

        ~QuotesList_ViewModel()
        {
            TwsData.DataFeeder.RealTimeDataReceived_Event -= DataFeeder_RealTimeDataEndReceived_Event;
            AddSymbol_VM.ContractSelected_Event           -= AddSymbol_VM_ContractSelected_Event;
        }

        // 
        // Called from AddSymbol_ViewModel when a contract was selected from list, will add it to the listview
        // then will subscribe to receive TWS real time (5s) data.
        //
        private void AddSymbol_VM_ContractSelected_Event(object sender, ContractDetailsRecv_EventArgs e)
        {
            addNew(e.ContractData);
        }

        private async void addNew(ContractDetails contractDetails, bool needsSaveToDB = true)
        {
            Quote_ViewModel symVM = QuotesList.FirstOrDefault(s => s.Symbol == contractDetails.Contract.Symbol);
            
            if (symVM != null)
            {
                // First cancel the old:
                TwsData.DataFeeder.CancelRealTime(symVM.ReqId);
                lock (symbolsList_Lock)
                {
                    dispatcher.Invoke(() =>
                    {
                        SymbolsList.Remove(symVM.ReqId);
                        QuotesList.Remove(symVM);
                    });
                }
            }

            Tuple<List<Bar>, TwsError> tuple = TwsData.DataFeeder.GetPreviousCloses(contractDetails.Contract, 2);

            await UpdateClosePrices(contractDetails, tuple.Item1, needsSaveToDB);
        }

        private async Task UpdateClosePrices(ContractDetails contractDetails, List<Bar> closePricesList, bool needsSaveToDB = true)
        {
            // Historical data list is empty, just return:
            lock(symbolsList_Lock)
            { 
                if (closePricesList == null || closePricesList.Count == 0) return;
            }

            // send real time request for symbol: 
            int reqId = await TwsData.DataFeeder.RequestRealTime(contractDetails.Contract);

            Quote_ViewModel symbVM = new Quote_ViewModel(reqId, contractDetails);
            if (symbVM == null) return;

            CultureInfo provider = CultureInfo.InvariantCulture;

            DateTime time = DateTime.Now;

            if (closePricesList.Count == 1)
            {
                symbVM.PrevClose   = closePricesList[0].Open;
                symbVM.LowValue    = closePricesList[0].Low;
                symbVM.HighValue   = closePricesList[0].High;
                symbVM.LatestClose = closePricesList[0].Close;

                time = DateTime.ParseExact(closePricesList[0].Time, "yyyyMMdd", provider);
            }
            else if (closePricesList.Count == 2)
            {
                symbVM.PrevClose   = closePricesList[0].Close;
                symbVM.LowValue    = closePricesList[1].Low;
                symbVM.HighValue   = closePricesList[1].High;
                symbVM.LatestClose = closePricesList[1].Close;

                time = DateTime.ParseExact(closePricesList[1].Time, "yyyyMMdd", provider);
            }
            else return;

            symbVM.Time = time.ToShortDateString();

            lock (symbolsList_Lock)
            {
                dispatcher.Invoke(() =>
                {
                    QuotesList.Add(symbVM);
                    SymbolsList.Add(reqId, symbVM);
                });
            }

            ChangeDimensions(height, width);

            if(needsSaveToDB == true) symbVM.SaveToDB();
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
                    addSymbol_VM = new AddSymbol_ViewModel(dispatcher);
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

        public void LoadFromDB()
        {
            DB_ModelContainer db = new DB_ModelContainer();

            List<ContractData> cDataList = db.DisplayedContracts.ToList();

            foreach (ContractData cData in cDataList)
            {
                Thread t = new Thread(new ParameterizedThreadStart(addContractDetails));
                t.Start(cData);
            }
        }

        public async void addContractDetails(object obj)
        {
            if (!(obj is ContractData cData)) return;

            List<ContractDetails> contractDetailsList;
            contractDetailsList = await TwsData.DataFeeder.GetContractDetailsList(cData);

            if (contractDetailsList.Count == 0) return;

            addNew(contractDetailsList[0], false);
        }
        
        private void Quote_ViewModel_ContractRemoved_Event(object sender, EventArgs e)
        {
            Quote_ViewModel qvm = sender as Quote_ViewModel;

            if (qvm == null) return;

            Quote_ViewModel symVM = QuotesList.FirstOrDefault(s => s == qvm);
            
            if (symVM != null)
            {
                // First cancel the old:
                TwsData.DataFeeder.CancelRealTime(symVM.ReqId);
                lock (symbolsList_Lock)
                {
                    SymbolsList.Remove(symVM.ReqId);
                    QuotesList.Remove(symVM);
                }
            }
        }

        private void DataFeeder_ConnectionClosed_Event(object sender, EventArgs e)
        {
            TwsData.DataFeeder.ConnectionClosed_Event -= DataFeeder_ConnectionClosed_Event;

            foreach(Quote_ViewModel symVM in QuotesList)
            {
                TwsData.DataFeeder.CancelRealTime(symVM.ReqId);
            }

            lock (symbolsList_Lock)
            {
                dispatcher.Invoke(() =>
                {
                    SymbolsList.Clear();
                    QuotesList.Clear();
                });
            }
        }
    }
}
