using System.Threading;
using System.Windows;
using System.Windows.Controls;

namespace TwsSharpApp
{
    /// <summary>
    /// Interaction logic for RealTime_View.xaml
    /// </summary>
    public partial class QuotesList_View : UserControl
    {
        public QuotesList_View()
        {
            InitializeComponent();
        }

        private void lvQuotes_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            QuotesList_ViewModel vm = DataContext as QuotesList_ViewModel;
            if(vm != null)
            {
                vm.ChangeDimensions(e.NewSize.Height, e.NewSize.Width);
            }
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            QuotesList_ViewModel vm = DataContext as QuotesList_ViewModel;
            if(vm != null)
            {
                vm.LoadFromDB();
            }
        }
    }
}
