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
        private Main_ViewModel mainViewModel;

        public MainWindow()
        {
            InitializeComponent();

            DB_ModelContainer db = new DB_ModelContainer();
            db.Database.Migrate();

            mainViewModel = new Main_ViewModel(Dispatcher);
            DataContext = mainViewModel;

            Workspace_ViewModel.SetFullScreen_Event += VmFullScreen_SetFullScreen_Event;
        }

        ~MainWindow()
        {
            Workspace_ViewModel.SetFullScreen_Event -= VmFullScreen_SetFullScreen_Event;
        }

        protected override void OnClosing (System.ComponentModel.CancelEventArgs e)
        {
            base.OnClosing(e);

            Main_ViewModel mainViewModel = DataContext as Main_ViewModel;
            if(mainViewModel != null && mainViewModel.IsRestartRequested == true)
                System.Windows.Forms.Application.Restart();
        }

        private void Window_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.Escape)
            {
                if(isFullScreen == true) setNormalWindow();
            }
            else if(e.Key == System.Windows.Input.Key.F11)
            {
                if(isFullScreen == false) setFullScreen();
            }
        }

        //
        // Stuf related to Enable/Disable Full Screen Mode:
        //
        private double lastHeight, lastWidth, lastTop, lastLeft;
        private WindowStyle lastWindowStyle;
        private ResizeMode  lastResizeMode;
        private WindowState lastWindowState;

        private void storeWindowStatus()
        {
            // Store windows position and dimensions
            if(WindowState == WindowState.Normal)
            {
                lastHeight = (ActualHeight > SystemParameters.PrimaryScreenHeight) ? 
                             SystemParameters.PrimaryScreenHeight : ActualHeight;
                lastWidth  = (ActualWidth > SystemParameters.PrimaryScreenWidth) ?
                             SystemParameters.PrimaryScreenWidth : ActualWidth;
                lastTop    = Top;
                lastLeft   = Left;
            }

            // Store windows style and resize mode
            lastWindowStyle = WindowStyle;
            lastResizeMode  = ResizeMode;

            lastWindowState = WindowState;
        }

        private void VmFullScreen_SetFullScreen_Event(object sender, System.EventArgs e)
        {
            setFullScreen();
        }


        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            storeWindowStatus();
        }

        private bool isFullScreen = false;

        private void setFullScreen()
        {
            if (mainViewModel != null)
            {
                IFullScreen selectedVM = mainViewModel.GetActiveTab as IFullScreen;
                if(selectedVM != null)
                {
                    if (!this.Resources.Contains(new DataTemplateKey(selectedVM.GetType())))
                        return;

                    DataContext = selectedVM;
                    selectedVM.SetFullScreen(true);
                }
                else return;
            }
            else return;

            storeWindowStatus();

            // Set values for FullScreen Mode
            WindowStyle = WindowStyle.None;
            ResizeMode  = ResizeMode.NoResize;

            Top    = 0;
            Left   = 0;
            Height = SystemParameters.PrimaryScreenHeight;
            Width  = SystemParameters.PrimaryScreenWidth;

            WindowState = WindowState.Normal;

            isFullScreen = true;
        }

        private void setNormalWindow()
        {
            IFullScreen selectedVM = DataContext as IFullScreen;

            if ((selectedVM != null) )
            {
                selectedVM.SetFullScreen(false);
            }
            else return;      

            DataContext = mainViewModel;

            Topmost = false;

            WindowStyle = lastWindowStyle;
            ResizeMode  = lastResizeMode;

            WindowState = lastWindowState;

            Top    = lastTop;
            Left   = lastLeft;
            Height = lastHeight;
            Width  = lastWidth;

            isFullScreen = false;
        }
    }
}
