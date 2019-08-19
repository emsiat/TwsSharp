using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;

namespace TwsSharpApp
{
    /// <summary>
    /// Interaction logic for Main_View.xaml
    /// </summary>
    public partial class Main_View : UserControl
    {
        public Main_View()
        {
            InitializeComponent();

            LoadViewModel();
        }

        private void LoadViewModel()
        {
            // Now, finally get the MainWindowViewModel in action:
            var viewModel = new Main_ViewModel();

            // When the ViewModel asks to be closed, 
            // close the window.
            EventHandler handler = null;
            handler = delegate
            {
                viewModel.RequestClose -= handler;
                //this.Close();
            };
            viewModel.RequestClose += handler;
           
            // Allow all controls in the window to 
            // bind to the ViewModel by setting the 
            // DataContext, which propagates down 
            // the element tree.
            this.DataContext = viewModel;
        }

        private async void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            Main_ViewModel mvm = DataContext as Main_ViewModel;
            if(mvm != null)
            {
                await mvm.ShowFrontPage();
                await Task.Run(() => mvm.StartConnection());
            }
        }

        private void Hyperlink_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                System.Diagnostics.Process.Start((e.Source as Hyperlink).NavigateUri.ToString());
            }
            catch {}
        }
    }
}
