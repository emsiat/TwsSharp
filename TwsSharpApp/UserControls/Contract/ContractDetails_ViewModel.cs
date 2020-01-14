using IBApi;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text.RegularExpressions;
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

            // Get the openning and closing time from ContractDetails' liquid hours list
            GetExchangesTimes();

            // Add to the times list wich request reset statistics before
            TradingHours.Instance.AddTime(this);
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

        public Contract Contract => ContractDetails.Contract;

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

        public DateTime ResetVarsTime { get; private set; } = DateTime.MaxValue;
        public DateTime OpeningTime   { get; private set; } = DateTime.MaxValue;
        public DateTime ClosingTime   { get; private set; } = DateTime.MaxValue;

        public void GetExchangesTimes()
        {
            string[] days = ContractDetails.LiquidHours.Split(';');
            string strTimeZone = Regex.Match(ContractDetails.TimeZoneId, @"\(([^)]*)\)").Groups[1].Value;
            TimeZoneInfo tst = TimeZoneInfo.FindSystemTimeZoneById(strTimeZone);

            foreach(string day in days)
            {
                string[] hours = day.Split('-');
                if(hours.Length == 1 && hours[0].Contains("CLOSED"))
                {
                    continue;
                }
                else if(hours.Length == 2)
                {
                    DateTime dtNow = DateTime.Now;
                    
                    CultureInfo provider = CultureInfo.InvariantCulture;

                    if(OpeningTime == DateTime.MaxValue)
                    {
                        // OpeningTime has not been set yet.
                        DateTime parsedOpeningTime  = DateTime.ParseExact(hours[0], "yyyyMMdd:HHmm", provider);
                        parsedOpeningTime = TimeZoneInfo.ConvertTime(parsedOpeningTime, tst, TimeZoneInfo.Local);

                        // Do not set OpeningTime if is in the past.
                        // For example when we are during the trading hours, openingParsedTime is in the past. 
                        if (DateTime.Now < parsedOpeningTime)
                        {
                            OpeningTime   = parsedOpeningTime;
                            ResetVarsTime = OpeningTime.AddMinutes(-1 * TradingHours.ResetPeriodBeforeOpen);

                            // For tests:
                            //OpeningTime = DateTime.Now.AddSeconds(120);
                            //ResetVarsTime = OpeningTime.AddMinutes(-1);
                        }
                    }

                    if(ClosingTime == DateTime.MaxValue)
                    {
                        // ClosingTime has not been set yet.
                        DateTime parsedClosingTime  = DateTime.ParseExact(hours[1], "yyyyMMdd:HHmm", provider);
                        parsedClosingTime = TimeZoneInfo.ConvertTime(parsedClosingTime, tst, TimeZoneInfo.Local);

                        if (DateTime.Now < parsedClosingTime)
                        {
                            ClosingTime = parsedClosingTime;
                        }
                    }
                    //OpenTime = DateTime.Now.AddSeconds(60);

                    // ClosingTime and OpeningTime has been set, do not look for anything else
                    if ((ClosingTime != DateTime.MaxValue) && (OpeningTime != DateTime.MaxValue))
                    {
                        break;
                    }
                }
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

        public string Symbol     { get { return ContractDetails.Contract.Symbol; } }
        public string UniqueName { get { return ContractDetails.Contract.Symbol + ContractDetails.Contract.SecType + ContractDetails.Contract.Exchange; } }

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
