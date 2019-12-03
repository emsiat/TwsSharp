using IBApi;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using TwsSharpApp.Data;

namespace TwsSharpApp
{
    class TwsData : TwsWrapper
    {
        public event EventHandler<RequestId_EventArgs> RequestId_Event;
        private TwsData()
        {
        }

        public static TwsData DataFeeder { get; } = new TwsData();

        public Tuple<List<Bar>, TwsError> GetPreviousCloses(Contract contract, int days)
        {
            Semaphore semaphore = new Semaphore(0, 1);

            Tuple<List<Bar>, TwsError> reTuple;
            List<Bar> barsList = null;
            TwsError error = null;

            int reqId = int.MinValue;

            void hHistoricalBarReceived(object sender, HistoricalBarRecv_EventArgs e)
            {
                if (e.RequestId == reqId)
                {
                    if (barsList == null) barsList = new List<Bar>();

                    barsList.Add(e.HistoricalBar);
                }
            }

            void hHistoricalDataReceived(object sender, RequestId_EventArgs e)
            {
                if (e.RequestId == reqId) semaphore.Release();
            }

            void hErrorReceived(object sender, ErrorRecv_EventArgs e)
            {
                if (e.Error.id == reqId)
                {
                    error = e.Error;
                    semaphore.Release();
                }
            }

            HistoricalBarReceived_Event  += hHistoricalBarReceived;
            HistoricalDataEndReceived_Event += hHistoricalDataReceived;
            ErrorReceived_Event          += hErrorReceived;

            reqId = RequestPreviousCloses(contract, days);
            RequestId_Event?.Invoke(contract, new RequestId_EventArgs(reqId));

            semaphore.WaitOne();

            HistoricalDataEndReceived_Event -= hHistoricalDataReceived;
            HistoricalBarReceived_Event  -= hHistoricalBarReceived;
            ErrorReceived_Event          -= hErrorReceived;

            reTuple = new Tuple<List<Bar>, TwsError>(barsList, error);
            return reTuple;
        }

        public async Task<List<ContractDetails>> GetContractDetailsList(ContractData cd)
        {
            Semaphore semaphore = new Semaphore(0, 1);
            List<ContractDetails> contractDetailsList = new List<ContractDetails>();
            int reqId = -1;

            void hContractDetailsReceived(object sender, ContractDetailsRecv_EventArgs e)
            {
                if(e.RequestId == reqId) contractDetailsList.Add(e.ContractData);
            }

            void hContractDetailsEndReceived(object sender, RequestId_EventArgs e)
            {          
                if(e.RequestId == reqId) semaphore.Release();
            }

            void hErrorReceived(object sender, ErrorRecv_EventArgs e)
            {
                if(e.Error.id == reqId) semaphore.Release();
            }

            ContractDetailsReceived_Event    += hContractDetailsReceived;
            ContractDetailsEndReceived_Event += hContractDetailsEndReceived;
            ErrorReceived_Event              += hErrorReceived;

            reqId = RequestContractDetails(cd.ToContract());

            semaphore.WaitOne();

            ContractDetailsReceived_Event    -= hContractDetailsReceived;
            ContractDetailsEndReceived_Event -= hContractDetailsEndReceived;
            ErrorReceived_Event              -= hErrorReceived;

            await Task.CompletedTask;
            return contractDetailsList;
        }
    }
}
