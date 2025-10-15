using ASKUE.Classes;
using ASKUE.Pages;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;

namespace ASKUE
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            MainFrame.Navigate(new LoginPage());
        }

        private void MainFrame_Navigated(object sender, NavigationEventArgs e)
        {
            BtnBack.Visibility = MainFrame.CanGoBack ? Visibility.Visible : Visibility.Collapsed;
            
            BtnLogout.Visibility = !(e.Content is LoginPage) && AppContext.CurrentUser != null ? Visibility.Visible : Visibility.Collapsed;

            if (e.Content is Page page)
                TBlockTitle.Text = page.Title;

            TBlockUserFio.Text = AppContext.CurrentUser?.fio ?? "";
        }

        private void BtnBack_Click(object sender, RoutedEventArgs e)
        {
            if (MainFrame.CanGoBack)
                MainFrame.GoBack();
        }

        private void BtnLogout_Click(object sender, RoutedEventArgs e)
        {
            AppContext.CurrentUser = null;
            while (MainFrame.CanGoBack)
            {
                MainFrame.RemoveBackEntry();
            }
            MainFrame.Navigate(new LoginPage());
        }
    }
}