using System;
using System.Windows;

namespace TwsSharpApp
{
    class SettingsList_ViewModel : Workspace_ViewModel
    {
        private SettingsList settingsList = null;
        private SettingsList prevList = null;

        public static string MyName = "Settings";

        public SettingsList_ViewModel(SettingsList sList)
        {
            DisplayName = MyName;
            settingsList = sList;
        }

        //
        // ConnIp
        //
        public string ConnIp
        {
            get{ return settingsList.GetValueStr(SettingsList.Keys.ConnIp); }
            set
            {
                if( settingsList.GetValueStr(SettingsList.Keys.ConnIp) == value ) return;
                settingsList.SetValue(SettingsList.Keys.ConnIp, value);
                OnPropertyChanged(nameof(ConnIp));
                OnPropertyChanged(nameof(IsConnIpChanged));
                OnPropertyChanged(nameof(IsAnySettingsChanged));
                OnPropertyChanged(nameof(IsRestartNeeded));
            }
        }

        public bool IsConnIpChanged => settingsList.IsValueChanged(SettingsList.Keys.ConnIp);

        //
        // ConnPort
        //
        public int? ConnPort
        {
            get{ return settingsList.GetValueInt(SettingsList.Keys.ConnPort); }
            set
            {
                if( settingsList.GetValueInt(SettingsList.Keys.ConnPort) == value ) return;
                settingsList.SetValue(SettingsList.Keys.ConnPort, value);
                OnPropertyChanged(nameof(ConnPort));
                OnPropertyChanged(nameof(IsConnPortChanged));
                OnPropertyChanged(nameof(IsAnySettingsChanged));
                OnPropertyChanged(nameof(IsRestartNeeded));
            }
        }

        public bool IsConnPortChanged => settingsList.IsValueChanged(SettingsList.Keys.ConnPort);

        //
        // ConnClientID
        //
        public string ConnClientID
        {
            get{ return settingsList.GetValueStr(SettingsList.Keys.ConnClientID); }
            set
            {
                if( settingsList.GetValueStr(SettingsList.Keys.ConnClientID) == value ) return;
                settingsList.SetValue(SettingsList.Keys.ConnClientID, value);
                OnPropertyChanged(nameof(ConnClientID));
                OnPropertyChanged(nameof(IsConnClientIDChanged));
                OnPropertyChanged(nameof(IsAnySettingsChanged));
                OnPropertyChanged(nameof(IsRestartNeeded));
            }
        }

        public bool IsConnClientIDChanged => settingsList.IsValueChanged(SettingsList.Keys.ConnClientID);

        //
        // IsAnySettingsChanged
        //
        public bool IsAnySettingsChanged
        {
            get
            {
                return IsConnIpChanged   ||
                       IsConnPortChanged ||
                       IsConnClientIDChanged;
            }
        }

        public bool IsRestartNeeded
        {
            get
            {
                return IsConnIpChanged   ||
                       IsConnPortChanged ||
                       IsConnClientIDChanged;
            }
        }

        private RelayCommand connUndo_Command;
        public  RelayCommand ConnUndo_Command
        {
            get
            {
                return connUndo_Command ?? (connUndo_Command = new RelayCommand(param => this.ConnUndo()));
            }
        }

        private void ConnUndo()
        {
            settingsList.ConnUndo();

            OnPropertyChanged(nameof(ConnIp));
            OnPropertyChanged(nameof(IsConnIpChanged));

            OnPropertyChanged(nameof(ConnPort));
            OnPropertyChanged(nameof(IsConnPortChanged));

            OnPropertyChanged(nameof(ConnClientID));
            OnPropertyChanged(nameof(IsConnClientIDChanged));

            OnPropertyChanged(nameof(IsAnySettingsChanged));
            OnPropertyChanged(nameof(IsRestartNeeded));
        }

        private RelayCommand update_Command;
        public  RelayCommand Update_Command
        {
            get
            {
                return update_Command ?? (update_Command = new RelayCommand(param => this.UpdateSettings()));
            }
        }

        public event EventHandler RestartRequest;

        private void UpdateSettings()
        {
            settingsList.SaveToDB();
            RestartRequest?.Invoke(this, EventArgs.Empty);
        }
    }
}
