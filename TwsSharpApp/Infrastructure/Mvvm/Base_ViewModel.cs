using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace TwsSharpApp
{
    /// <summary>
    /// Base class for all ViewModel classes in the application.
    /// It provides support for property change notifications 
    /// and has a DisplayName property.  This class is abstract.
    /// </summary>
    public abstract class Base_ViewModel : INotifyPropertyChanged
    {
        /// <summary>
        /// Raised when a property on this object has a new value.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Raises this object's PropertyChanged event.
        /// </summary>
        /// <param columnName="propertyName">The property that has a new value.</param>
        public void OnPropertyChanged([CallerMemberName] string propertyName = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        protected bool SetProperty<T>(ref T storage, T value, [CallerMemberName] String propertyName = null)
        {
            if(object.Equals(storage, value)) return false;
            storage = value;
            OnPropertyChanged(propertyName);
            return true;
        }

        private string displayName;
        public  string DisplayName
        {
            get { return displayName; }
            set
            {
                if (displayName == value) return;
                displayName = value;
                OnPropertyChanged("DisplayName");
            }
        }
    }
}
