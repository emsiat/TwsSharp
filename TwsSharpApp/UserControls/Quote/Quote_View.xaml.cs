using System.Windows.Controls;

namespace TwsSharpApp
{
    /// <summary>
    /// Interaction logic for Quote_View.xaml
    /// </summary>
    public partial class Quote_View : UserControl
    {
        public Quote_View()
        {
            InitializeComponent();
        }

        private void Border_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            Quote_ViewModel vm = DataContext as Quote_ViewModel;

            if (vm == null) return;

            vm.IsMouseOver = true;
        }

        private void Border_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            Quote_ViewModel vm = DataContext as Quote_ViewModel;

            if (vm == null) return;

            vm.IsMouseOver = false;
        }
    }
}
