using ASKUE.Classes;
using ASKUE.Models;
using ASKUE.Windows;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace ASKUE.Pages
{
    public partial class AdminUsersPage : Page
    {
        public AdminUsersPage() { InitializeComponent(); RefreshData(); }

        private void RefreshData()
        {
            // !!! ИСПРАВЛЕНО: Полный путь к AppContext !!!
            UsersGrid.ItemsSource = ASKUE.Classes.AppContext.GetContext().K_Polzovateli.ToList();
        }

        private void BtnAdd_Click(object sender, RoutedEventArgs e)
        {
            // !!! ИСПРАВЛЕНО: Убран .Windows из вызова !!!
            var addEditWindow = new AddEditUserWindow(null);
            addEditWindow.ShowDialog();
            RefreshData();
        }

        private void BtnEdit_Click(object sender, RoutedEventArgs e)
        {
            if (UsersGrid.SelectedItem is K_Polzovateli selectedUser)
            {
                // !!! ИСПРАВЛЕНО: Убран .Windows из вызова !!!
                var addEditWindow = new AddEditUserWindow(selectedUser);
                addEditWindow.ShowDialog();
                RefreshData();
            }
            else MessageBox.Show("Выберите пользователя для редактирования.");
        }

        private void BtnDelete_Click(object sender, RoutedEventArgs e)
        {
            if (UsersGrid.SelectedItem is K_Polzovateli selectedUser)
            {
                if (MessageBox.Show($"Вы уверены, что хотите удалить пользователя {selectedUser.fio}?", "Подтверждение", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                {
                    // !!! ИСПРАВЛЕНО: Полный путь к AppContext !!!
                    ASKUE.Classes.AppContext.GetContext().K_Polzovateli.Remove(selectedUser);
                    ASKUE.Classes.AppContext.GetContext().SaveChanges();
                    RefreshData();
                }
            }
            else MessageBox.Show("Выберите пользователя для удаления.");
        }
    }
}