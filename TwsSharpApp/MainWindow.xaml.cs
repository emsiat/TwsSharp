using Microsoft.EntityFrameworkCore;
using System.Windows;
using TwsSharpApp.Data;

namespace TwsSharpApp
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            DB_ModelContainer db = new DB_ModelContainer();
            db.Database.Migrate();
        }

        protected override void OnClosing (System.ComponentModel.CancelEventArgs e)
        {
            base.OnClosing(e);

            Main_ViewModel mainViewModel = mainViewControl.DataContext as Main_ViewModel;
            if(mainViewModel != null && mainViewModel.IsRestartRequested == true)
                System.Windows.Forms.Application.Restart();
        }
    }
}
