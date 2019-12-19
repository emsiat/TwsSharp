using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Threading;

namespace TwsSharpApp
{
    public class Main_ViewModel : Workspace_ViewModel
    {
        private Dispatcher dispatcher;
        private SettingsList settingsList = null;

        public Main_ViewModel(Dispatcher dspchr)
        {
            dispatcher  = dspchr;
            DisplayName = "TwsSharp";
        }

        ~Main_ViewModel()
        {
            TwsData.DataFeeder.ErrorReceived_Event    -= DataFeeder_ErrorReceived_Event;
            TwsData.DataFeeder.SocketConnected_Event  -= DataFeeder_SocketConnected_Event;

            CloseConnection();
        }

        #region TWS Code
        public void StartConnection()
        {
            TwsData.DataFeeder.ErrorReceived_Event    += DataFeeder_ErrorReceived_Event;
            TwsData.DataFeeder.SocketConnected_Event  += DataFeeder_SocketConnected_Event;

            // Open connection to TWS:
            string connIp       = settingsList.GetValueStr(SettingsList.Keys.ConnIp);
            int?   connPort     = settingsList.GetValueInt(SettingsList.Keys.ConnPort);
            int?   connClientID = settingsList.GetValueInt(SettingsList.Keys.ConnClientID);

            if(!string.IsNullOrEmpty(connIp) && connPort != null && connClientID != null)
            {
                TwsData.DataFeeder.REQ_OpenSocket(connIp, connPort.Value, connClientID.Value);
            }
        }

        private void DataFeeder_SocketConnected_Event(object sender, SocketConnected_EventArgs e)
        {
            if(e.IsConnected == true)
            {
                // The Socket is connected, load the symbols from DB
                QuotesList_ViewModel.Instance.LoadFromDB();
            }
            else
            {
                dispatcher.Invoke(() =>
                {
                    ShowSettings();
                });
            }
        }

        public void CloseConnection()
        {
            // close the socket (called at exit):
            TwsData.DataFeeder.CloseSocket();
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

            if(sender is SettingsList_ViewModel)
            {
                (sender as SettingsList_ViewModel).RestartRequest -= Tab_RestartRequest;
            }
        }

        private void ShowFrontPage()
        {
            QuotesList_ViewModel tab = this.TabsCollection.FirstOrDefault(t => t.DisplayName == QuotesList_ViewModel.MyName)
                                        as QuotesList_ViewModel;

            if(tab == null)
            {
                tab = QuotesList_ViewModel.CreateNew(dispatcher);
                this.TabsCollection.Add(tab);
            }

            this.SetActiveTab(tab);
        }

        private void SetActiveTab(Workspace_ViewModel tab)
        {
            CommandsCollection.MoveCurrentTo(tab);
        }

        public object GetActiveTab => CommandsCollection.CurrentItem;

        private RelayCommand showSettings_Command;
        public  RelayCommand ShowSettings_Command
        {
            get
            {
                return showSettings_Command ?? (showSettings_Command = new RelayCommand(param => this.ShowSettings()));
            }
        }

        private void ShowSettings()
        {
            SettingsList_ViewModel tab = TabsCollection.FirstOrDefault(vm => vm.DisplayName == SettingsList_ViewModel.MyName)
                                         as SettingsList_ViewModel;

            if(tab == null)
            {
                tab = new SettingsList_ViewModel(settingsList);
                TabsCollection.Add(tab);
                tab.RestartRequest += Tab_RestartRequest;
            }

            SetActiveTab(tab);
        }

        public bool IsRestartRequested = false;
        private async void Tab_RestartRequest(object sender, EventArgs e)
        {
            CloseConnection();

            await Task.Delay(1000);

            IsRestartRequested = true;
 
            Application.Current.Shutdown();
        }

        public void ExecuteOnLoad()
        {
            if(LoadSettingsFromDB() == false)
            {
                ShowSettings();
            }
            else
            {
                ShowFrontPage();
                StartConnection();
            }
        }

        private bool LoadSettingsFromDB()
        {
            settingsList = new SettingsList();
            return settingsList.LoadSettingsFromDB();
        }
    }
}
