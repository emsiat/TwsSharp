using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Data;

namespace TwsSharpApp
{
    public class Main_ViewModel : Workspace_ViewModel
    {
        public static TwsWrapper DataFeeder{get; set;} = new TwsWrapper();

        public Main_ViewModel()
        {
            DisplayName = "TwsSharp";
        }

        ~Main_ViewModel()
        {
            CloseConnection();
        }

        #region TWS Code
        public void StartConnection()
        {
            Main_ViewModel.DataFeeder.ErrorReceived_Event += DataFeeder_ErrorReceived_Event;

            // Open connection to TWS:
            DataFeeder.REQ_OpenSocket();
        }

        public void CloseConnection()
        {
            // close the socket (called at exit):
            Main_ViewModel.DataFeeder.CloseSocket();
        }

        private void DataFeeder_ErrorReceived_Event(object sender, ErrorRecv_EventArgs e)
        {
            // Update Error property when an error is received:
            Error = DateTime.Now.ToString("HH:mm:ss.f") + 
                    ", Error. Id: " + e.Error.id + 
                    ", Code: " +  e.Error.errorCode + 
                    ", Msg: " + e.Error.errorMsg + "\n"
                    + error;
        }

        private string error;
        public  string Error
        {
            get { return error; }
            set 
            {
                if (error == value) return;
                error = value;
                OnPropertyChanged(nameof(Error));
            }
        }
        #endregion

        private ICollectionView commandsCollection;
        public  ICollectionView CommandsCollection
        {
            get
            {
                if(commandsCollection == null)
                {
                    commandsCollection = CollectionViewSource.GetDefaultView(this.TabsCollection);
                    commandsCollection.GroupDescriptions.Add(new PropertyGroupDescription("Category"));
                }

                return commandsCollection;
            }
        }

        private ObservableCollection<Workspace_ViewModel> tabsCollection;
        public  ObservableCollection<Workspace_ViewModel> TabsCollection
        {
            get
            {
                if(tabsCollection == null)
                {
                    tabsCollection = new ObservableCollection<Workspace_ViewModel>();
                    tabsCollection.CollectionChanged += this.OnTabsCollection_Changed;
                }
                return tabsCollection;
            }
        }

        public long TabsCount
        {
            get
            {
                if(tabsCollection == null) return 0;
                return tabsCollection.Count;
            }
        }

        void OnTabsCollection_Changed(object sender, NotifyCollectionChangedEventArgs e)
        {
            if(e.NewItems != null && e.NewItems.Count != 0)
                foreach(Workspace_ViewModel tab in e.NewItems)
                    tab.RequestClose += this.OnTab_RequestsClose;

            if(e.OldItems != null && e.OldItems.Count != 0)
                foreach(Workspace_ViewModel tab in e.OldItems)
                    tab.RequestClose -= this.OnTab_RequestsClose;

            base.OnPropertyChanged(nameof(TabsCount));
        }

        void OnTab_RequestsClose(object sender, EventArgs e)
        {
            Workspace_ViewModel tab = sender as Workspace_ViewModel;
            this.TabsCollection.Remove(tab);
        }

        public async Task ShowFrontPage()
        {
            QuotesList_ViewModel tab = this.TabsCollection.FirstOrDefault(t => t.DisplayName == QuotesList_ViewModel.MyName)
                                        as QuotesList_ViewModel;

            if(tab == null)
            {
                tab = new QuotesList_ViewModel();
                this.TabsCollection.Add(tab);
            }

            this.SetActiveTab(tab);

            tab.AddNewQuote("QCOM");
            tab.AddNewQuote("AVGO");
            //tab.AddNewQuote("INTC");
            //tab.AddNewQuote("MSFT");
            //tab.AddNewQuote("IBM");
            //tab.AddNewQuote("AVGO");
            //tab.AddNewQuote("VOD");
            //tab.AddNewQuote("T");
            //tab.AddNewQuote("NOK");
            //tab.AddNewQuote("OXLC");
            //tab.AddNewQuote("BABA");
            //tab.AddNewQuote("RY");

            await Task.CompletedTask;
        }

        private void SetActiveTab(Workspace_ViewModel tab)
        {
            CommandsCollection.MoveCurrentTo(tab);
        }
    }
}
