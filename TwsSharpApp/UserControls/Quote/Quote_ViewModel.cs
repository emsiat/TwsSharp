namespace TwsSharpApp
{
    public class Quote_ViewModel : Workspace_ViewModel
    {
        public string Symbol { get; set; }

        public Quote_ViewModel(string symb)
        {
            Symbol = symb;
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

        private double lowValue = double.MaxValue;
        public  double LowValue
        {
            get { return lowValue; }
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

        private double latest = 0;
        public  double Latest
        {
            get { return latest; }
            set
            {
                if (latest == value) return;
                latest = value;
                OnPropertyChanged(nameof(Latest));

                // Also bind the Var property:
                OnPropertyChanged(nameof(Var));
            }
        }

        public  double Var
        {
            get { return Latest - PrevClose; }
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
    }
}
