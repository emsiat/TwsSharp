using IBApi;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
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

        private PipelineStartup pipelineStartup;

        private ActionBlock<Tuple<int, Bar>> processReceivedRT;

        private QuotesList_ViewModel()
        {
            DisplayName = MyName;

            QuotesListView = CollectionViewSource.GetDefaultView(this.QuotesList) as ListCollectionView;
            QuotesListView.IsLiveSorting = true;
            QuotesListView.SortDescriptions.Add(new SortDescription("VarPercent", ListSortDirection.Descending));

            IsTabSelected = true;
            CanClose = false;

            pipelineStartup = new PipelineStartup();

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

        public void StartRealTime()
        {
            if (SynchronizationContext.Current == null)
                SynchronizationContext.SetSynchronizationContext(new SynchronizationContext());

            TaskScheduler CurrentTaskScheduler = TaskScheduler.FromCurrentSynchronizationContext();

            processReceivedRT = new ActionBlock<Tuple<int, Bar>>(input =>
            {
                int reqId = input.Item1;
                Quote_ViewModel symbVM;

                // Find the right symbol ViewModel, based from reqId
                lock (symbolsList_Lock) 
                {
                    if (SymbolsList.ContainsKey(reqId))
                        symbVM = SymbolsList[reqId];
                    else
                        return;
                }
                
                // update with received Bar values:
                symbVM.UpdateRealTimeData(input.Item2);
            },
            new ExecutionDataflowBlockOptions
            {
                //CancellationToken = cancellationSource.Token,
                MaxDegreeOfParallelism = -1,
                TaskScheduler = CurrentTaskScheduler
            });
        }

        public async void Start(ContractData cd = null)
        {
            List<ContractData> cDataList;
            Task rtTask;

            if (cd == null)
            {
                DB_ModelContainer db = new DB_ModelContainer();
                cDataList = db.DisplayedContracts.ToList();

                // Create Real Time queue only once, 
                rtTask = Task.Factory.StartNew(StartRealTime);
            }
            else
            {
                // Got only one contract data to add a symbol,
                // possible source is AddSymbol_ViewModel:
                cDataList = new List<ContractData>();
                cDataList.Add(cd);
            }

            pipelineStartup.QuoteAdded            += PipelineStartup_AddQuote;
            pipelineStartup.QuoteRemoved          += PipelineStartup_QuoteRemoved;
            pipelineStartup.QuotesRealTimeStarted += PipelineStartup_QuotesRealTimeStarted;

            await pipelineStartup.Run(cDataList);
 
            ChangeDimensions(height, width);

            pipelineStartup.QuoteAdded            -= PipelineStartup_AddQuote;
            pipelineStartup.QuoteRemoved          -= PipelineStartup_QuoteRemoved;
            pipelineStartup.QuotesRealTimeStarted -= PipelineStartup_QuotesRealTimeStarted;

            //if (cd == null)
            //{
            //    System.Timers.Timer timer = new System.Timers.Timer();

            //    timer.Elapsed += async (sender, e) => await TickTimer();
            //    timer.AutoReset = true;
            //    timer.Interval = 5000;
            //    timer.Start();
            //}
        }

        //private async Task TickTimer()
        //{
        //    try
        //    {
        //        foreach (var q in QuotesList)
        //        {
        //            await Task.Delay(1);
        //            var rand = new Random((int)DateTime.UtcNow.Ticks);
        //            double val = rand.NextDouble();
        //            processReceivedRT.Post(new Tuple<int, Bar>(q.ReqId, new Bar(DateTime.Now.ToString("HH:mm:ss"),
        //                q.Latest, q.HighValue, q.LowValue, q.Latest * (1 + (0.5 - val)), 0, 0, 0)));
        //        }
        //    }
        //    catch (Exception) {}
        //}

        private async void PipelineStartup_QuoteRemoved(object sender, Quote_EventArgs e)
        {
            await dispatcher.InvokeAsync(() =>
            {
                QuotesList .Remove(e.Quote_VM);
                SymbolsList.Remove(e.Quote_VM.ReqId);
            });
        }

        private async void PipelineStartup_QuotesRealTimeStarted(object sender, Quote_EventArgs e)
        {
            await dispatcher.InvokeAsync(() =>
            {
                SymbolsList.Add(e.Quote_VM.ReqId, e.Quote_VM);
            });
        }

        private async void PipelineStartup_AddQuote(object sender, Quote_EventArgs e)
        {
            await dispatcher.InvokeAsync(() =>
            {
                Quote_ViewModel qVM = QuotesList.FirstOrDefault(s => s.Symbol == e.Quote_VM.Symbol);

                if (qVM != null)
                {
                    // We have to remove the already existing quote with the same symbol:
                    QuotesList.Remove(qVM);
                    if (SymbolsList.ContainsKey(qVM.ReqId))
                    {
                        SymbolsList.Remove(qVM.ReqId);
                    }
                }
                QuotesList.Add(e.Quote_VM);
            });
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
        private async void AddSymbol_VM_ContractSelected_Event(object sender, ContractDetailsRecv_EventArgs e)
        {
            ContractData cd = new ContractData(e.ContractDetails.Contract);
            Start(cd);

            DB_ModelContainer db = new DB_ModelContainer();
            db.DisplayedContracts.Add(cd);
            await db.SaveChangesAsync();
        }

        //
        // Called from TWS when realtime bar has been received for a contract.
        //
        private void DataFeeder_RealTimeDataEndReceived_Event(object sender, RealtimeBarRecv_EventArgs e)
        {
            // a real time bar has been received:
            processReceivedRT.Post(new Tuple<int, Bar>(e.RequestId, e.RealtimeBar));
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
