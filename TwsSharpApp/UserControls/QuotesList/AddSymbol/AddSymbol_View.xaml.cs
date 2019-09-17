using System.Windows;
using System.Windows.Controls;

namespace TwsSharpApp
{
    /// <summary>
    /// Interaction logic for AddSymbol_View.xaml
    /// </summary>
    public partial class AddSymbol_View : UserControl
    {
        public AddSymbol_View()
        {
            InitializeComponent();
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            AddSymbol_ViewModel vm = DataContext as AddSymbol_ViewModel;
            if(vm != null)
            {
                vm.Dispatcher = this.Dispatcher;
            }
        }
    }
}
