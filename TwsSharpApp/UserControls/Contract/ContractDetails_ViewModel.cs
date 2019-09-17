using IBApi;
using System;
using System.Globalization;
using System.Threading.Tasks;

namespace TwsSharpApp
{
    public class ContractDetails_ViewModel : Base_ViewModel
    {
        private bool isCanceled = false;
        private bool isLoading = false;

        public event EventHandler EndPriceUpdate_Event;
        
        public ContractDetails_ViewModel()
        {
        }

        public ContractDetails_ViewModel(ContractDetails cd)
        {
            contractDetails = cd;
            Main_ViewModel.DataFeeder.HistoricalDataReceived_Event += DataFeeder_HistoricalDataReceived_Event;
            Main_ViewModel.DataFeeder.ErrorReceived_Event          += DataFeeder_ErrorReceived_Event;
        }

        private ContractDetails contractDetails;
        public  ContractDetails ContractDetails { get { return contractDetails; } }

        private int reqId = 0;
        public  int ReqId
        {
            get { return reqId; }
        }

        private double price = double.NaN;
        public  double Price { get { return price; } }

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
            }
        }

        public string Exchange    { get { return contractDetails.Contract.Exchange; } }
        public string PrimaryExch { get { return contractDetails.Contract.PrimaryExch; } }
        public string SecType     { get { return contractDetails.Contract.SecType; } }
        public string Currency    { get { return contractDetails.Contract.Currency; } }

        public string LongName    { get { return contractDetails.LongName; } }
        public string UnderSymbol { get { return contractDetails.UnderSymbol; } }
        public string MarketName  { get { return contractDetails.MarketName; } }

        private void DataFeeder_ErrorReceived_Event(object sender, ErrorRecv_EventArgs e)
        {
            if (e.Error != null && e.Error.id == reqId)
            {
                price = 0;
                changeStatus(isCanceled, false);
                
                ErrorMsg = e.Error.errorCode.ToString() + " : " + e.Error.errorMsg;
            }
        }

        private void DataFeeder_HistoricalDataReceived_Event(object sender, HistoricalRecv_EventArgs e)
        {
            if (e.HistoricalList == null) return;

            CultureInfo provider = CultureInfo.InvariantCulture;
            if(reqId == e.RequestId)
            {
                price = e.HistoricalList[0].Close;

                changeStatus(false, false);
            }
        }

        public async Task StartGettingData()
        {
            changeStatus(false, true);
            reqId = await Main_ViewModel.DataFeeder.RequestLatestClose(contractDetails.Contract);
        }

        public void CancelPriceUpdate()
        {
            changeStatus(true, false);
            Main_ViewModel.DataFeeder.CancelHistoricalData(reqId);
        }

        private void changeStatus(bool isCanc, bool isInProgr)
        {
            isCanceled = isCanc;
            isLoading = isInProgr;

            if(!isLoading)
            {
                Main_ViewModel.DataFeeder.HistoricalDataReceived_Event -= DataFeeder_HistoricalDataReceived_Event;
                Main_ViewModel.DataFeeder.ErrorReceived_Event -= DataFeeder_ErrorReceived_Event;
                EndPriceUpdate_Event?.Invoke(this, new EventArgs());
            }

            OnPropertyChanged(nameof(PriceStr));
        }

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
