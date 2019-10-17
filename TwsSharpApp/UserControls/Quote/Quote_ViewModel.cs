using IBApi;
using System;
using System.Windows;
using System.Windows.Media;

namespace TwsSharpApp
{
    public class Quote_ViewModel : Workspace_ViewModel
    {
        public string Symbol { get { return ContractDetails.Contract.Symbol; } }
        public int ReqId { get; set; }

        public ContractDetails ContractDetails { get; set; }

        public Quote_ViewModel(int reqId, ContractDetails cDetails)
        {
            ReqId  = reqId;
            ContractDetails = cDetails;
            IsTabSelected = true;
        }

        private double prevClose = 0;
        public  double PrevClose
        {
            get { return prevClose; }
            set 
            {
                if (prevClose == value) return;
                prevClose = value;
                OnPropertyChanged(nameof(PrevClose));
            }
        }

        private double latestClose = 0;
        public  double LatestClose
        {
            get { return latestClose; }
            set 
            {
                if (latestClose == value) return;
                latestClose = value;
                OnPropertyChanged(nameof(LatestClose));

                Latest = latestClose;
            }
        }

        private double lowValue = double.MaxValue;
        public  double LowValue
        {
            get
            {
                if (lowValue == double.MaxValue) return 0;

                return lowValue;
            }

            set
            {
                if (lowValue <= value) return;
                lowValue = value;
                OnPropertyChanged(nameof(LowValue));
            }
        }

        private double highValue = 0;
        public  double HighValue
        {
            get { return highValue; }
            set
            {
                if (highValue >= value) return;
                highValue = value;
                OnPropertyChanged(nameof(HighValue));
            }
        }

        private double previous = 0;
        private double latest = double.NaN;
        public  double Latest
        {
            get { return latest; }
            set
            {
                if (latest == value) return;
                previous = latest;
                latest = value;
                OnPropertyChanged(nameof(Latest));

                IsDefined = !(double.IsNaN(latest));
                
                // Compute variations:
                Var = latest - prevClose;
                VarPercent = 100 * var / prevClose;

                OnPropertyChanged(nameof(Background_TickVariation));
            }
        }

        private bool isDefined = false;
        public  bool IsDefined
        {
            get { return isDefined; }
            set
            {
                if (isDefined == value) return;
                isDefined = value;
                OnPropertyChanged(nameof(IsDefined));
            }
        }


        private double var = 0;
        public  double Var
        {
            get { return var; }
            set
            {
                if (var == value) return;
                bool isColorChanging = (Math.Sign(var) != Math.Sign(value));

                var = value;

                if (isColorChanging)
                {
                    OnPropertyChanged(nameof(Background_DailyVariation));
                }
                OnPropertyChanged(nameof(Var));
            }
        }

        private double varPercent = 0;
        public  double VarPercent
        {
            get { return varPercent; }
            set
            {
                if (varPercent == value) return;
                varPercent = value;
                OnPropertyChanged(nameof(VarPercent));
            }
        }

        private string time;
        public  string Time
        {
            get { return time; }
            set 
            {
                if (time == value) return;
                time = value;
                OnPropertyChanged(nameof(Time));
            }
        }

        Color Color_Var_Daily_Backgroud_Positive = (Color)Application.Current.Resources["Color.Var.Daily.Backgroud.Positive"];
        Color Color_Var_Daily_Backgroud_Negative = (Color)Application.Current.Resources["Color.Var.Daily.Backgroud.Negative"];
        Color Color_Var_Daily_Backgroud_Zero     = (Color)Application.Current.Resources["Color.Var.Daily.Backgroud.Zero"];

        public Color Background_DailyVariation
        {
            get
            {
                if (Var < 0)
                    return Color_Var_Daily_Backgroud_Negative;
                else if (Var > 0)
                    return Color_Var_Daily_Backgroud_Positive;
                else
                    return Color_Var_Daily_Backgroud_Zero;
            }
        }

        Color Color_Var_Tick_Backgroud_Positive = (Color)Application.Current.Resources["Color.Var.Tick.Backgroud.Positive"];
        Color Color_Var_Tick_Backgroud_Negative = (Color)Application.Current.Resources["Color.Var.Tick.Backgroud.Negative"];
        Color Color_Var_Tick_Backgroud_Zero     = (Color)Application.Current.Resources["Color.Var.Tick.Backgroud.Zero"];

        public Color Background_TickVariation
        {
            get
            {
                if (latest - previous < 0)
                    return Color_Var_Tick_Backgroud_Negative;
                else if (latest - previous > 0)
                    return Color_Var_Tick_Backgroud_Positive;
                else
                    return Color_Var_Tick_Backgroud_Zero;
            }
        }
    }
}
