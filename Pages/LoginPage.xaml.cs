using ASKUE.Classes;
using ASKUE.Models; // Добавьте этот using, если его нет
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;

namespace ASKUE.Pages
{
    public partial class LoginPage : Page
    {
        public LoginPage() { InitializeComponent(); }

        private void BtnLogin_Click(object sender, RoutedEventArgs e)
        {
            var user = AppContext.GetContext().Set<K_Polzovateli>()
                .FirstOrDefault(p => p.login == TBoxLogin.Text && p.parol == PBoxPassword.Password);

            if (user != null)
            {
                AppContext.CurrentUser = user;
                switch (user.id_rol)
                {
                    case 1: NavigationService.Navigate(new AdminMainPage()); break;
                    case 2: NavigationService.Navigate(new EmployeeMetersPage()); break;
                    case 3: NavigationService.Navigate(new UserDashboardPage()); break;
                }
            }
            else
            {
                MessageBox.Show("Неверный логин или пароль.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}