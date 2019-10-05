using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Threading;

namespace TwsSharpApp
{
    class AddSymbol_ViewModel : Base_ViewModel
    {
        public Dispatcher         Dispatcher { get; set; }
        public ListCollectionView Contracts_ListView { get; set; }

        public ObservableCollection<ContractDetails_ViewModel> Contracts_List { get; set; } = new ObservableCollection<ContractDetails_ViewModel>();

        public AddSymbol_ViewModel()
        {
            Contracts_ListView = CollectionViewSource.GetDefaultView(this.Contracts_List) as ListCollectionView;
            Contracts_ListView.IsLiveFiltering = true;
            
            //Contracts_ListView.Filter = FindPoints;
        }

        private void InitAll()
        {
            Symbol = string.Empty;
            Contracts_List.Clear();
        }

        private string symbol = string.Empty;
        public  string Symbol
        {
            get { return symbol; }
            set
            {
                if (symbol == value) return;
                symbol = value;
                OnPropertyChanged(nameof(Symbol));
            }
        }

        private bool isVisible = false;
        public  bool IsVisible
        {
            get { return isVisible; }
            set
            {
                if (isVisible == value) return;
                isVisible = value;
                OnPropertyChanged(nameof(IsVisible));
            }
        }

        private string selectedSymbol = string.Empty;
        public  string SelectedSymbol
        {
            get { return selectedSymbol; }
            set
            {
                if (selectedSymbol == value) return;
                selectedSymbol = value;
                OnPropertyChanged(nameof(SelectedSymbol));
            }
        }

        public int SelectedReqId { get; set; } = -1;

        private RelayCommand closeCommand;
        public  RelayCommand CloseCommand
        {
            get
            {
                return closeCommand ?? (closeCommand = new RelayCommand(async param => await this.Close(), param => true));
            }
        }

        private async Task Close()
        {
            await CancelSearch();
            Symbol = string.Empty;
            Contracts_List.Clear();

            IsVisible = false;
        }

        private RelayCommand startSearch_Command;
        public  RelayCommand StartSearch_Command
        {
            get
            {
                return startSearch_Command ?? (startSearch_Command = new RelayCommand(async param => await this.StartSearch(), param => true));
            }
        }

        private async Task StartSearch()
        {
            IsSearchingInProgress = true;
            Contracts_List.Clear();

            Main_ViewModel.DataFeeder.ContractDetailsReceived_Event    += DataFeeder_ContractDetailsReceived_Event;
            Main_ViewModel.DataFeeder.ContractDetailsEndReceived_Event += DataFeeder_ContractDetailsEndReceived_Event;
            
            Main_ViewModel.DataFeeder.RequestContractDetails_Stock(Symbol);

            await Task.CompletedTask;
        }

        private RelayCommand cancelSearch_Command;
        public  RelayCommand CancelSearch_Command
        {
            get
            {
                return cancelSearch_Command ?? (cancelSearch_Command = new RelayCommand(async param => await this.CancelSearch(), param => true));
            }
        }

        private async Task CancelSearch()
        {
            Main_ViewModel.DataFeeder.ContractDetailsReceived_Event    -= DataFeeder_ContractDetailsReceived_Event;
            Main_ViewModel.DataFeeder.ContractDetailsEndReceived_Event -= DataFeeder_ContractDetailsEndReceived_Event;

            // Cancel price update for all ContractDetails
            foreach(ContractDetails_ViewModel cdVm in Contracts_List)
            {
                cdVm.CancelPriceUpdate();
            }

            await Task.CompletedTask;
        }

        private RelayCommand leftDoubleClick_Command;
        public  RelayCommand LeftDoubleClick_Command
        {
            get
            {
                return leftDoubleClick_Command ?? (leftDoubleClick_Command = new RelayCommand(async param => await leftDoubleClicked()));
            }
        }

        public  event EventHandler<ContractDetailsRecv_EventArgs> ContractSelected_Event;

        //
        // Double click on contract will fire up ContractSelected_Event (to AddSymbol_ViewModel)
        //
        private async Task leftDoubleClicked()
        {
            if(SelectedReqId <= 0) return;

            // First cancel others ContractDetails requests
            foreach (ContractDetails_ViewModel cdVm in Contracts_List)
            {
                cdVm.CancelPriceUpdate();
            }

            ContractDetails_ViewModel contractVM = Contracts_List.FirstOrDefault(c => c.ReqId == SelectedReqId);
            ContractSelected_Event?.Invoke(this, new ContractDetailsRecv_EventArgs(SelectedReqId, contractVM.ContractDetails));

            await Close();
        }

        //
        // Called from TWS when data about one contract has been received. 
        //
        private async void DataFeeder_ContractDetailsReceived_Event(object sender, ContractDetailsRecv_EventArgs e)
        {
            ContractDetails_ViewModel contractVM = new ContractDetails_ViewModel(e.ContractData);
            Dispatcher.Invoke(() =>  Contracts_List.Add(contractVM));

            contractVM.EndPriceUpdate_Event += ContractVM_EndPriceUpdate_Event;

            await contractVM.StartGettingData();

            lock(countContractsUpdating_lock) { ++countContractsUpdating; }
        }

        private int countContractsUpdating = 0;
        private object countContractsUpdating_lock = new object();

        //
        // Called from ContractDetails_ViewModel when data about one contract has been received. 
        // When all ContractDetails are received, IsSearchingInProgress is set to false
        //
        private void ContractVM_EndPriceUpdate_Event(object sender, EventArgs e)
        {
            ContractDetails_ViewModel contractVM = sender as ContractDetails_ViewModel;

            if (contractVM == null) return;

            contractVM.EndPriceUpdate_Event -= ContractVM_EndPriceUpdate_Event;

            lock(countContractsUpdating_lock) { --countContractsUpdating; }

            if (countContractsUpdating == 0) IsSearchingInProgress = false;
        }

        //
        // Called from TWS where all contracts detail have been received. Unsubscribe from all events.
        //
        private void DataFeeder_ContractDetailsEndReceived_Event(object sender, ContractDetailsRecv_EventArgs e)
        {
            Main_ViewModel.DataFeeder.ContractDetailsReceived_Event    -= DataFeeder_ContractDetailsReceived_Event;
            Main_ViewModel.DataFeeder.ContractDetailsEndReceived_Event -= DataFeeder_ContractDetailsEndReceived_Event;
        }

        public bool IsNotSearchingInProgress { get { return !isSearchingInProgress;  } }

        private bool isSearchingInProgress = false;
        public  bool IsSearchingInProgress
        {
            get { return isSearchingInProgress; }
            set
            {
                if (isSearchingInProgress == value) return;
                isSearchingInProgress = value;

                // Do not update this, but the negation of it:
                OnPropertyChanged(nameof(IsNotSearchingInProgress));
                OnPropertyChanged(nameof(IsSearchingInProgress));
            }
        }

        private bool FindPoints(object obj)
        {
            ContractDetails_ViewModel contractVM = obj as ContractDetails_ViewModel;
            return contractVM.Price > 0;
        }
    }
}
