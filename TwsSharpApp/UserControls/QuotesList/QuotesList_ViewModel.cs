using IBApi;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Threading;
using TwsSharpApp.Data;

namespace TwsSharpApp
{
    class QuotesList_ViewModel : Workspace_ViewModel, IFullScreen
    {
        public ListCollectionView QuotesListView { get; set; }
        public ObservableCollection<Quote_ViewModel> QuotesList { get; set; } = new ObservableCollection<Quote_ViewModel>();

        private Dictionary<int, Quote_ViewModel> SymbolsList = new Dictionary<int, Quote_ViewModel>();

        public static string MyName = "RT Quotes";

        private readonly object symbolsList_Lock = new object();

        private Dispatcher dispatcher = Application.Current?.Dispatcher;

        private static QuotesList_ViewModel instance = null;
        public  static QuotesList_ViewModel Instance => instance ?? (instance = new QuotesList_ViewModel());

        private QuotesList_ViewModel()
        {
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
            TradingHours.StatisticsNeedReset_Event        += TradingHours_StatisticsNeedReset_Event;
        }

        ~QuotesList_ViewModel()
        {
            TwsData.DataFeeder.RealTimeDataReceived_Event -= DataFeeder_RealTimeDataEndReceived_Event;
            AddSymbol_VM.ContractSelected_Event           -= AddSymbol_VM_ContractSelected_Event;
            TradingHours.StatisticsNeedReset_Event        -= TradingHours_StatisticsNeedReset_Event;
        }

        private void TradingHours_StatisticsNeedReset_Event(object sender, StatisticsReset_EventArgs e)
        {
            Task task = Task.Factory.StartNew(() =>
            {
                foreach(string uniqueName in e.UniqueNames_List)
                {
                    lock (symbolsList_Lock)
                    {
                        Quote_ViewModel symVM = QuotesList.FirstOrDefault(s => s.UniqueName == uniqueName);
                        
                        if (symVM != null)
                        {
                            symVM.RenewPreviousClose();
                            TradingHours.Instance.RemoveSymbol(uniqueName);
                        }
                    }
                    break;
                }
            });

            task.Wait();
        }

        // 
        // Called from AddSymbol_ViewModel when a contract was selected from list, will add it to the listview
        // then will subscribe to receive TWS real time (5s) data.
        //
        private void AddSymbol_VM_ContractSelected_Event(object sender, ContractDetailsRecv_EventArgs e)
        {
            addNew(e.ContractData);
            ChangeDimensions(height, width);
        }

        private void addNew(ContractDetails contractDetails, bool needsSaveToDB = true)
        {
            Quote_ViewModel symVM = null;
            ContractDetails_ViewModel cd_vm = new ContractDetails_ViewModel(contractDetails);

            lock (symbolsList_Lock)
            {
                // Get the openning and closing time from ContractDetails' liquid hours list
                cd_vm.GetExchangesTimes();

                // Add to the times list wich request reset statistics before
                TradingHours.Instance.AddTime(cd_vm);

                symVM = QuotesList.FirstOrDefault(s => s.Symbol == contractDetails.Contract.Symbol);
                if(symVM != null)
                {
                    // Little prob to find a symbol previously added to the list:
                    // First cancel the old:
                    TwsData.DataFeeder.CancelRealTime(symVM.ReqId);
 
                    // then remove it from list
                    dispatcher.InvokeAsync(() =>
                    {
                        QuotesList .Remove(symVM);
                        SymbolsList.Remove(symVM.ReqId);
                    });
                }
            }
         
            UpdateClosePrices(cd_vm, needsSaveToDB);
        }

        private void UpdateClosePrices(ContractDetails_ViewModel contractDetails, bool needsSaveToDB = true)
        {
            List<Bar> closePricesList = null;

            Tuple<List<Bar>, TwsError> tuple = TwsData.DataFeeder.GetPreviousCloses(contractDetails.Contract, 2);

            closePricesList = tuple.Item1;

            // Historical data list is empty, just return:
            if (closePricesList == null || closePricesList.Count == 0) return;

            // send real time request for symbol: 
            int reqId = TwsData.DataFeeder.RequestRealTime(contractDetails.Contract); 

            Quote_ViewModel symbVM = new Quote_ViewModel(reqId, contractDetails);
            if (symbVM == null) return;

            // Update the previous and last closes values and times:
            if (symbVM.SetClosedValues(closePricesList) == false)
                return;

            lock (symbolsList_Lock)
            {
                dispatcher.InvokeAsync(() =>
                {
                    QuotesList .Add(symbVM);
                    SymbolsList.Add(reqId, symbVM);
                });
            }

            if(needsSaveToDB == true) symbVM.SaveToDB();
        }

        //
        // Called from TWS when realtime bar has been received for a contract.
        //
        private void DataFeeder_RealTimeDataEndReceived_Event(object sender, RealtimeBarRecv_EventArgs e)
        {
            // a real time bar has been received:
            int reqId = e.RequestId;
            Quote_ViewModel symbVM;
            
            // Find the right symbol ViewModel, based from reqId
            lock(symbolsList_Lock) { symbVM = SymbolsList[reqId]; }

            // update with received Bar values:
            symbVM.UpdateRealTimeData(e.RealtimeBar);
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

        public void ChangeDimensions(double h, double w)
        {
            double itemW = 0;

            // store the new height and width:
            height = h;
            width  = w;

            if(QuotesList.Count == 0) return;

            // compute raw dimension for a square:
            itemW = Math.Pow(h * w / QuotesList.Count, 0.5);

            // compute how many squares fits in window width and increase the number by 1
            int slotsWidth = (int)Math.Ceiling(width / itemW) + 1;

            // compute the final dimension for a square:
            ItemWidth = width / slotsWidth - 1 ;
        }

        //
        // This is the ViewModel for the Add New Symbol user control
        //
        private AddSymbol_ViewModel addSymbol_VM = null;
        public  AddSymbol_ViewModel AddSymbol_VM => addSymbol_VM ?? (addSymbol_VM = new AddSymbol_ViewModel());

        private RelayCommand showAddSymbolCommand;
        public  RelayCommand ShowAddSymbolCommand => 
                             showAddSymbolCommand ?? (showAddSymbolCommand = new RelayCommand(param =>  this.ShowAddSymbol(), 
                                                                                              param => !this.AddSymbol_VM.IsVisible));

        private void ShowAddSymbol()
        {
            AddSymbol_VM.IsVisible = true;
        }

        public async void LoadFromDB()
        {
            DB_ModelContainer  db        = new DB_ModelContainer();
            List<ContractData> cDataList = db.DisplayedContracts.ToList();
            List<Task>         tasks     = new List<Task>();
            
            cDataList.ForEach(cData => tasks.Add(Task.Factory.StartNew(() => { addContractDetails(cData); })));
            await Task.WhenAll(tasks);

            await dispatcher.InvokeAsync(() =>
            {
                TwsData.DataFeeder.error("No. of symbols added to display list: " + QuotesList.Count.ToString());
            });


            ChangeDimensions(height, width);
        }

        public void addContractDetails(object obj)
        {
            // return if obj is not ContractData
            if(!(obj is ContractData cData)) return;

            // Get a list of contracts details using the ContactData cData:
            List<ContractDetails> contractDetailsList = TwsData.DataFeeder.GetContractDetailsList(cData);

            if ((contractDetailsList is null) || (contractDetailsList.Count == 0)) return;
            
            // It should be only one:
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
                try
                {
                    dispatcher.InvokeAsync(() =>
                    {
                        SymbolsList.Clear();
                        QuotesList.Clear();
                    });
                }
                catch (Exception)
                { }
            }
        }

        public void SetFullScreen(bool isfs)
        {
            IsFullScreen = isfs;
            SymbolsList.Values.ToList().ForEach(q => { q.IsFullScreen = isfs; q.IsMouseOver = false; });
        }

        private RelayCommand setFullScreen_Command;
        public  RelayCommand SetFullScreen_Command => 
                             setFullScreen_Command ?? (setFullScreen_Command = new RelayCommand(param => this.InvokeFullScreen()));   
    }
}
