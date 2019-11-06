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
    }
}
