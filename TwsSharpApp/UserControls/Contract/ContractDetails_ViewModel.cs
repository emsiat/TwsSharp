using IBApi;
using System;
using System.Collections.Generic;
using System.Threading;

namespace TwsSharpApp
{
    public class ContractDetails_ViewModel : Base_ViewModel
    {
        public ContractDetails ContractDetails { get; }
        public int ReqId { get; private set; } = 0;

        private double price = double.NaN;

        private bool isCanceled = false;
        private bool isLoading = false;

        public event EventHandler EndPriceUpdate_Event;
        
        public ContractDetails_ViewModel()
        {
        }

        public ContractDetails_ViewModel(ContractDetails cd)
        {
            ContractDetails = cd;
        }

        //
        // Starts by requesting 1 previous close data:
        //
        public void StartDownloadData()
        {
            changeStatus(false, true);

            if(Thread.CurrentThread.Name == null)
            {
                Thread.CurrentThread.Name = ContractDetails.Contract.ToString();
            }

            TwsData.DataFeeder.RequestId_Event += DataFeeder_RequestId_Event;
            Tuple<List<Bar>, TwsError> tuple = TwsData.DataFeeder.GetPreviousCloses(ContractDetails.Contract, 1);

            if (tuple.Item2 != null && tuple.Item2.id == ReqId)
            {
                price = double.NaN;
                ErrorMsg = tuple.Item2.errorCode.ToString() + " : " + tuple.Item2.errorMsg;

                changeStatus(isCanceled, false);
            }
            else if(tuple.Item1.Count > 0)
            {
                price = tuple.Item1[0].Close;
                changeStatus(false, false);
            }        
        }

        private void DataFeeder_RequestId_Event(object sender, RequestId_EventArgs e)
        {
            Contract contract = sender as Contract;
            if(contract == ContractDetails.Contract)
            {
                TwsData.DataFeeder.RequestId_Event -= DataFeeder_RequestId_Event;
                ReqId = e.RequestId;
            }
        }

        // 
        // Send a cancel request:
        //
        public void CancelPriceUpdate()
        {
            changeStatus(true, false);
            TwsData.DataFeeder.CancelHistoricalData(ReqId);
        }

        private void changeStatus(bool isCanc, bool isInProgr)
        {
            isCanceled = isCanc;
            isLoading  = isInProgr;

            if(!isLoading)
            {
                EndPriceUpdate_Event?.Invoke(this, new EventArgs());
            }

            OnPropertyChanged(nameof(PriceStr));
        }

        public string PriceStr
        {
            get
            {
                if(price == 0) return "N/A";
                else if(isCanceled && !isLoading)
                {
                    if (double.IsNaN(price)) return "Canceled";
                    else return price.ToString("N2");
                }
                else if(isCanceled && isLoading)
                {
                    if(double.IsNaN(price)) return "N/A";
                    else return price.ToString("N2");
                }
                else if(!isCanceled && isLoading)
                {
                    if(double.IsNaN(price)) return "Loading...";
                    else return price.ToString("N2");
                }
                else if(!isCanceled && !isLoading)
                {
                    if(double.IsNaN(price)) return "N/A";
                    else return price.ToString("N2");
                }

                return price.ToString("N2");         
            }
        }

        private string errorMsg = string.Empty;
        public string  ErrorMsg
        {
            get { return errorMsg; }
            set
            {
                if (errorMsg == value) return;
                errorMsg = value.Replace("Historical Market Data Service error message:", "");
                OnPropertyChanged(nameof(ErrorMsg));
                OnPropertyChanged(nameof(IsEnabled));
            }
        }

        public bool IsEnabled
        {
            get { return string.IsNullOrEmpty(errorMsg) ? true : false; }
        }

        public string Exchange    { get { return ContractDetails.Contract.Exchange; } }
        public string PrimaryExch { get { return ContractDetails.Contract.PrimaryExch; } }
        public string SecType     { get { return ContractDetails.Contract.SecType; } }
        public string Currency    { get { return ContractDetails.Contract.Currency; } }

        public string LongName    { get { return ContractDetails.LongName; } }
        public string UnderSymbol { get { return ContractDetails.UnderSymbol; } }
        public string TimeZoneId  { get { return ContractDetails.TimeZoneId; } }

/*
        private double minTick;
        private int priceMagnifier;
        private string orderTypes;
        private string validExchanges;
        private int underConId;
        
        private string contractMonth;
        private string industry;
        private string category;
        private string subcategory;
        private string timeZoneId;
        private string tradingHours;
        private string liquidHours;
        private string evRule;
        private double evMultiplier;
        private int mdSizeMultiplier;
        private int aggGroup;
        private List<TagValue> secIdList;
        
        private string underSecType;
        private string marketRuleIds;
        private string realExpirationDate;
        private string lastTradeTime;
*/
    }
}
