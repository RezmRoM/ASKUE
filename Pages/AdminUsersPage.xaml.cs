using ASKUE.Models; // Assuming this contains K_Polzovateli
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation; // Important: Add this for NavigationService

namespace ASKUE.Pages
{
    public partial class AdminUsersPage : Page
    {
        public AdminUsersPage()
        {
            InitializeComponent();
            // It's generally better to load data in the Loaded event for Pages,
            // or when the page is navigated to, to ensure NavigationService is available
            // and the UI is fully initialized.
            this.Loaded += AdminUsersPage_Loaded;
        }

        private void AdminUsersPage_Loaded(object sender, RoutedEventArgs e)
        {
            RefreshData();
        }

        private void RefreshData()
        {
            // Ensure your AppContext is correctly referenced.
            // If Classes.AppContext is in the root namespace, you might need 'global::' or just 'Classes.AppContext.GetContext()'
            // if 'using Classes;' is present, or the full namespace as shown.
            UsersGrid.ItemsSource = ASKUE.Classes.AppContext.GetContext().K_Polzovateli.ToList();
        }

        private void BtnAdd_Click(object sender, RoutedEventArgs e)
        {
            // Use NavigationService to navigate to the AddEditUserPage
            // Check if NavigationService is available (it should be if hosted in a Frame/NavigationWindow)
            if (NavigationService != null)
            {
                NavigationService.Navigate(new AddEditUserPage(null)); // Pass null for adding a new user
            }
            else
            {
                MessageBox.Show("NavigationService is not available. Ensure this page is hosted within a Frame or NavigationWindow.");
            }
        }

        private void BtnEdit_Click(object sender, RoutedEventArgs e)
        {
            if (UsersGrid.SelectedItem is K_Polzovateli selectedUser)
            {
                if (NavigationService != null)
                {
                    // Pass the selected user object for editing
                    NavigationService.Navigate(new AddEditUserPage(selectedUser));
                }
                else
                {
                    MessageBox.Show("NavigationService is not available. Ensure this page is hosted within a Frame or NavigationWindow.");
                }
            }
            else
            {
                MessageBox.Show("Выберите пользователя для редактирования.");
            }
        }

        private void BtnDelete_Click(object sender, RoutedEventArgs e)
        {
            if (UsersGrid.SelectedItem is K_Polzovateli selectedUser)
            {
                if (MessageBox.Show($"Вы уверены, что хотите удалить пользователя {selectedUser.fio}?", "Подтверждение", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                {
                    try
                    {
                        ASKUE.Classes.AppContext.GetContext().K_Polzovateli.Remove(selectedUser);
                        ASKUE.Classes.AppContext.GetContext().SaveChanges();
                        RefreshData(); // Refresh the data grid after deletion
                    }
                    catch (System.Exception ex) // Catch potential database errors
                    {
                        MessageBox.Show($"Ошибка при удалении пользователя: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
            else
            {
                MessageBox.Show("Выберите пользователя для удаления.");
            }
        }
    }
}