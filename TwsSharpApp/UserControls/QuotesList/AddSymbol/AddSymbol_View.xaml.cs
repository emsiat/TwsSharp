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

        private void TxtSymbol_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if ((bool)e.NewValue == true)
                txtSymbol.Focus();
        }

        private void TxtSymbol_PreviewMouseUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            txtSymbol.SelectAll();
        }
    }
}
