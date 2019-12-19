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
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            Main_ViewModel mvm = DataContext as Main_ViewModel;
            if(mvm != null)
            {
                mvm.ExecuteOnLoad();
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
