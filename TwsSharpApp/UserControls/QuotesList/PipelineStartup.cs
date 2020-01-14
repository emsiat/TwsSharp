using IBApi;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using System.Windows;
using System.Windows.Threading;
using TwsSharpApp.Data;

namespace TwsSharpApp
{
    public class PipelineStartup
    {
        //private ObservableCollection<Quote_ViewModel> QuotesList;
        private CancellationTokenSource cancellationSource;

        TransformBlock<ContractData, Tuple<string, ContractDetails_ViewModel>> downloadContractDetails;
        TransformBlock<Tuple<string, ContractDetails_ViewModel>, Tuple<ContractDetails_ViewModel, List<Bar>>> downloadPreviousCloses;
        TransformBlock<Tuple<ContractDetails_ViewModel, List<Bar>>, Quote_ViewModel> pushQuoteToList;
        ActionBlock<Quote_ViewModel> startRealTime;

        public static TaskScheduler CurrentTaskScheduler;
        //private Dispatcher dispatcher = Application.Current?.Dispatcher;

        public PipelineStartup(/*ObservableCollection<Quote_ViewModel> quotesList*/)
        {
            //QuotesList = quotesList;
        }

        public async Task Run(List<ContractData> cDataList)
        {
            var taskSchedulerPair  = new ConcurrentExclusiveSchedulerPair();
            var exclusiveScheduler = new ConcurrentExclusiveSchedulerPair().ExclusiveScheduler;

            var linkOptions = new DataflowLinkOptions { PropagateCompletion = true };

            cancellationSource = new CancellationTokenSource();

            if (SynchronizationContext.Current == null)
                SynchronizationContext.SetSynchronizationContext(new SynchronizationContext());

            CurrentTaskScheduler = TaskScheduler.FromCurrentSynchronizationContext();

            downloadContractDetails = new TransformBlock<ContractData, Tuple<string, ContractDetails_ViewModel>>(contractData =>
            {
                return DownloadContractDetails(contractData);
            },
            new ExecutionDataflowBlockOptions
            {
                CancellationToken = cancellationSource.Token,
                MaxDegreeOfParallelism = -1,
                TaskScheduler = taskSchedulerPair.ConcurrentScheduler
            });

            downloadPreviousCloses = new TransformBlock<Tuple<string, ContractDetails_ViewModel>, Tuple<ContractDetails_ViewModel, List<Bar>>>(inputTuple =>
            {
                return DownloadPreviousCloses(inputTuple.Item2);
            },
            new ExecutionDataflowBlockOptions
            {
                CancellationToken = cancellationSource.Token,
                MaxDegreeOfParallelism = -1,
                TaskScheduler = taskSchedulerPair.ConcurrentScheduler
            });

            pushQuoteToList = new TransformBlock<Tuple<ContractDetails_ViewModel, List<Bar>>, Quote_ViewModel>(async inputTuple =>
            {
                ContractDetails_ViewModel cd_vm = inputTuple.Item1;
                List<Bar> bars_List = inputTuple.Item2;

                return await PushQuoteToList(cd_vm, bars_List);
            },
            new ExecutionDataflowBlockOptions
            {
                CancellationToken = cancellationSource.Token,
                MaxDegreeOfParallelism = -1,
                TaskScheduler = CurrentTaskScheduler
            });

            startRealTime = new ActionBlock<Quote_ViewModel>(quote_vm =>
            {
                StartRealTime(quote_vm);
            },
            new ExecutionDataflowBlockOptions
            {
                CancellationToken = cancellationSource.Token,
                MaxDegreeOfParallelism = -1,
                TaskScheduler = CurrentTaskScheduler
            });

            Predicate<Tuple<string, ContractDetails_ViewModel>> checkNullContractDetails = cd_vm => cd_vm.Item2 != null;
            downloadContractDetails.LinkTo(downloadPreviousCloses, linkOptions, checkNullContractDetails);

            // Discard if downloaded ContractDetails is null
            downloadContractDetails.LinkTo(DataflowBlock.NullTarget<Tuple<string, ContractDetails_ViewModel>>(), linkOptions, cd_vm => !checkNullContractDetails(cd_vm));

            downloadPreviousCloses.LinkTo(pushQuoteToList, linkOptions, output => (output.Item2 != null && output.Item2.Count > 0));
            downloadPreviousCloses.LinkTo(DataflowBlock.NullTarget<Tuple<ContractDetails_ViewModel, List<Bar>>>(), linkOptions, output => (output.Item2 == null || output.Item2.Count == 0));

            pushQuoteToList.LinkTo(startRealTime, linkOptions, quoteVM => quoteVM != null);
            pushQuoteToList.LinkTo(DataflowBlock.NullTarget<Quote_ViewModel>(), linkOptions, quoteVM => quoteVM == null);

            cDataList.ForEach(cData => { downloadContractDetails.Post(cData); });

            downloadContractDetails.Complete();

            try
            {
                // Asynchronously wait for the pipeline to complete processing and for
                // the progress bars to update.
                await Task.WhenAll(
                   downloadContractDetails.Completion,
                   downloadPreviousCloses.Completion,
                   startRealTime.Completion,
                   pushQuoteToList.Completion);
            }
            catch (OperationCanceledException)
            {
            }
        }

        // 
        // From IB, returns a ContractDetails for the specified contract.
        // Normally only one contract should be received since at this point
        // the presented contract data specifies all the fields
        // 
        private Tuple<string, ContractDetails_ViewModel> DownloadContractDetails(ContractData contractData)
        {
            ContractDetails_ViewModel contractDetails_vm = null;
            ContractDetails contractDetails = null;

            contractDetails = TwsData.DataFeeder.GetContractDetailsList(contractData).FirstOrDefault();

            if (contractDetails != null)
            {
                contractDetails_vm = new ContractDetails_ViewModel(contractDetails);
            }

            return new Tuple<string, ContractDetails_ViewModel>(contractData.Symbol, contractDetails_vm);
        }

        private Tuple<ContractDetails_ViewModel, List<Bar>> DownloadPreviousCloses(ContractDetails_ViewModel cd_vm)
        {
            //Quote_ViewModel symVM = null;
            List<Bar> closePricesList = null;
            ContractDetails_ViewModel retCd_vm = cd_vm;

            //symVM = QuotesList.FirstOrDefault(s => s.Symbol == cd_vm.Symbol);
            //if (symVM != null)
            //{
            //    // Little prob. to find a symbol previously added to the list:
            //    // First cancel the old:
            //    TwsData.DataFeeder.CancelRealTime(symVM.ReqId);

            //    QuoteRemoved?.Invoke(this, new Quote_EventArgs(symVM));
            //}
            //else
            //{
                Tuple<List<Bar>, TwsError> tuple = TwsData.DataFeeder.GetPreviousCloses(cd_vm.ContractDetails.Contract, 2);
                closePricesList = tuple.Item1;

                if (closePricesList == null || (closePricesList.Count < 1 || closePricesList.Count > 2))
                {
                    // Not the right count, give up
                    retCd_vm = null;
                }
                else
                {
                    closePricesList = tuple.Item1;
                }
            //}

            // finally, if everything was ok, retCd_vm is valid, otherwise is null
            return new Tuple<ContractDetails_ViewModel, List<Bar>>(cd_vm, closePricesList);
        }

        public event EventHandler<Quote_EventArgs> QuoteRemoved;
        public event EventHandler<Quote_EventArgs> QuoteAdded;
        private async Task<Quote_ViewModel> PushQuoteToList(ContractDetails_ViewModel cd_vm, List<Bar> closePricesList)
        {
            Quote_ViewModel retQuote_vm;

            retQuote_vm = new Quote_ViewModel(0, cd_vm);
            retQuote_vm.SetClosedValues(closePricesList);

            QuoteAdded?.Invoke(this, new Quote_EventArgs(retQuote_vm));

            await Task.CompletedTask;
            return retQuote_vm;
        }

        public event EventHandler<Quote_EventArgs> QuotesRealTimeStarted;
        private void StartRealTime(Quote_ViewModel quote_vm)
        {
            // send real time request for symbol: 
            int reqId = TwsData.DataFeeder.RequestRealTime(quote_vm.ContractDetails.Contract);
            quote_vm.ReqId = reqId;

            QuotesRealTimeStarted?.Invoke(this, new Quote_EventArgs(quote_vm));
        }
    }
}
