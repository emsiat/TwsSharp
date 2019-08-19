using System;

namespace TwsSharpApp
{
    public abstract class Workspace_ViewModel : Base_ViewModel
    {
        private RelayCommand closeCommand;
        private bool isTabSelected;
        
        protected Workspace_ViewModel()
        {
            CanClose = true;
        }

        public virtual Int32 Id { get { return 0; } }

        /// <summary>
        /// Returns the command that, when invoked, attempts
        /// to remove this workspace from the user interface.
        /// </summary>
        public RelayCommand CloseCommand
        {
            get
            {
                if (closeCommand == null)
                    closeCommand = new RelayCommand(param => this.OnRequestClose(),
                                    param => this.canClose);

                return closeCommand;
            }
        }

        /// <summary>
        /// Raised when this workspace should be removed from the UI.
        /// </summary>
        public event EventHandler RequestClose;
        public void OnRequestClose()
        {
            this.RequestClose?.Invoke(this, EventArgs.Empty);
        }

        public virtual bool IsTabSelected
        {
            get { return isTabSelected; }
            set
            {
                if (value == isTabSelected) return;
                isTabSelected = value;
                base.OnPropertyChanged(nameof(IsTabSelected));
            }
        }

        private bool canClose = true;
        public  bool CanClose
        {
            get { return canClose; }
            set
            {
                if (value == canClose) return;
                canClose = value;
                base.OnPropertyChanged(nameof(CanClose));
            }
        }

        private int icon;
        public  int Icon
        {
            get { return icon; }
            set
            {
                if (icon == value) return;
                icon = value;
                OnPropertyChanged(nameof(Icon));
            }
        }

        private string category;
        public  string Category
        {
            get { return category; }
            set
            {
                if (category == value) return;
                category = value;
                OnPropertyChanged(nameof(Category));
            }
        }

        private bool isLoading = true;
        public  bool IsLoading
        {
            get { return isLoading; }
            set
            {
                if (value == isLoading) return;
                isLoading = value;
                base.OnPropertyChanged(nameof(IsLoading));
            }
        }
    }
}
