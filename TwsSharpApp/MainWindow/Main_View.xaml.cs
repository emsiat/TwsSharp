using System.Threading.Tasks;
using System.Windows.Controls;

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

        private async void UserControl_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
              await Task.Run(() => ViewModel.Start());
        }

        ~Main_View()
        {
            ViewModel.Close();
        }
    }
}
