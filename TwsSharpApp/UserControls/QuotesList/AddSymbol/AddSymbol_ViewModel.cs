using IBApi;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Threading;
using TwsSharpApp.Data;

namespace TwsSharpApp
{
    class AddSymbol_ViewModel : Base_ViewModel
    {
        private Dispatcher dispatcher = Application.Current?.Dispatcher;
        public ListCollectionView Contracts_ListView { get; set; }

        public ObservableCollection<ContractDetails_ViewModel> Contracts_List { get; set; } = new ObservableCollection<ContractDetails_ViewModel>();

        public AddSymbol_ViewModel()
        {
            Contracts_ListView = CollectionViewSource.GetDefaultView(this.Contracts_List) as ListCollectionView;
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
        public  RelayCommand CloseCommand => 
                             closeCommand ?? (closeCommand = new RelayCommand(async param => await this.Close(), 
                                                                                    param => true));

        private async Task Close()
        {
            await CancelSearch();
            Symbol = string.Empty;
            Contracts_List.Clear();

            IsVisible = false;
        }

        private RelayCommand startSearch_Command;
        public  RelayCommand StartSearch_Command => 
                             startSearch_Command ?? (startSearch_Command = new RelayCommand(async param => await this.StartSearch(param as string), 
                                                                                                  param => true));

        private async Task StartSearch(string smb)
        {
            IsSearchingInProgress = true;
            Contracts_List.Clear();

            List<ContractDetails> contractDetailsList;
            
            if(string.IsNullOrEmpty(smb))
                contractDetailsList = TwsData.DataFeeder.GetContractDetailsList(new ContractData(){Symbol = this.Symbol, SecType="STK"});
            else
                contractDetailsList = TwsData.DataFeeder.GetContractDetailsList(new ContractData(){Symbol = smb, SecType="STK"});

            foreach(ContractDetails cd in contractDetailsList)
            {
                ContractDetails_ViewModel contractVM = new ContractDetails_ViewModel(cd);
                await dispatcher.InvokeAsync(() => Contracts_List.Add(contractVM));

                contractVM.EndPriceUpdate_Event += ContractVM_EndPriceUpdate_Event;

                Thread t = new Thread(new ThreadStart(contractVM.StartDownloadData));
                t.Start();

                lock(countContractsUpdating_lock)
                {
                    ++countContractsUpdating;
                }
            }
        }

        private RelayCommand cancelSearch_Command;
        public  RelayCommand CancelSearch_Command => 
                             cancelSearch_Command ?? (cancelSearch_Command = new RelayCommand(async param => await this.CancelSearch(), 
                                                                                                    param => true));

        private async Task CancelSearch()
        {
            // Cancel price update for all ContractDetails
            foreach(ContractDetails_ViewModel cdVm in Contracts_List)
            {
                cdVm.CancelPriceUpdate();
            }

            await Task.CompletedTask;
        }

        private RelayCommand leftDoubleClick_Command;
        public  RelayCommand LeftDoubleClick_Command => 
                             leftDoubleClick_Command ?? (leftDoubleClick_Command = new RelayCommand(async param => await leftDoubleClicked()));

        public  event EventHandler<ContractDetailsRecv_EventArgs> ContractSelected_Event;

        //
        // Double click on contract will fire up ContractSelected_Event (to AddSymbol_ViewModel)
        //
        private async Task leftDoubleClicked()
        {
            if(SelectedReqId <= 0) return;

            ContractDetails_ViewModel contractVM = Contracts_List.FirstOrDefault(c => c.ReqId == SelectedReqId);
            if (contractVM.IsEnabled == false) return;

            // First cancel others ContractDetails requests
            foreach (ContractDetails_ViewModel cdVm in Contracts_List)
            {
                cdVm.CancelPriceUpdate();
            }

            ContractSelected_Event?.Invoke(this, new ContractDetailsRecv_EventArgs(SelectedReqId, contractVM.ContractDetails));

            await Close();
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

            if(countContractsUpdating == 0) IsSearchingInProgress = false;
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
    }
}
