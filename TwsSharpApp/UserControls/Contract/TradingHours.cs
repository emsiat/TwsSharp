using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Timers;

namespace TwsSharpApp
{
    class TradingHours
    {
        public const int ResetPeriodBeforeOpen = 5; // minutes

        public static event EventHandler<StatisticsReset_EventArgs> StatisticsNeedReset_Event;

        private Dictionary<string, ContractDetails_ViewModel> openingHours = new Dictionary<string, ContractDetails_ViewModel>();
        private Dictionary<string, DateTime>                  closingHours = new Dictionary<string, DateTime>();

        private readonly object dictionary_Lock = new object();
        private Timer timer = new Timer();

        private TradingHours()
        {
            timer.Elapsed  += async ( sender, e ) => await TickTimer();
            timer.AutoReset = true;
        }

        private static TradingHours instance = null;
        public  static TradingHours Instance => instance ?? (instance = new TradingHours());

        private static async Task TickTimer()
        {
            List<string> expiredSymbols = Instance.expiredUniqueSymbols;

            if(expiredSymbols.Count > 0)
            {
                StatisticsNeedReset_Event?.Invoke(null, new StatisticsReset_EventArgs(Instance.expiredUniqueSymbols));
                Instance.StartTimer();
            }

            await Task.CompletedTask;
        }

        public void AddTime(ContractDetails_ViewModel cd_vm)
        {
            string uniqueSymbol = cd_vm.UniqueName;

            lock(dictionary_Lock)
            {
                openingHours[uniqueSymbol] = cd_vm;
                StartTimer();
            }
        }

        public void StartTimer()
        {
            timer.Stop();

            if (openingHours.Count == 0) return;

            DateTime resetVariablesTime = openingHours.Values.Min(cd => cd.ResetVarsTime).AddSeconds(1);
            TimeSpan dueTime  = resetVariablesTime - DateTime.Now;

            if (dueTime.TotalMilliseconds < 0)
                timer.Interval = 1;
            else
                timer.Interval = dueTime.TotalMilliseconds;

            timer.Start();
        }

        private List<string> expiredUniqueSymbols
        {
            get
            {
                List<string> expiredSymbols;    
                    
                lock(dictionary_Lock)
                {
                    expiredSymbols = openingHours.Where(i => i.Value.ResetVarsTime < DateTime.Now).Select(i => i.Key).ToList();
                }
                return expiredSymbols;
            }
        }

        public void RemoveSymbol(string uniqueName)
        {
            lock(dictionary_Lock)
            {
                openingHours.Remove(uniqueName);
            }
        }
    }
}
