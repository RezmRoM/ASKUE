using ASKUE.Classes;
using ASKUE.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity.Core.EntityClient;
using System.Data.SqlClient;
using System.Windows;

namespace ASKUE.Windows
{
    public class Role { public int Id { get; set; } public string Name { get; set; } }

    public partial class AddEditUserWindow : Window
    {
        private K_Polzovateli _currentUser;
        private string _sqlConnectionString;

        public AddEditUserWindow(K_Polzovateli user)
        {
            InitializeComponent();
            try
            {
                var entityConnectionString = "metadata=res://*/Models.Model1.csdl|res://*/Models.Model1.ssdl|res://*/Models.Model1.msl;provider=System.Data.SqlClient;provider connection string=\"data source=stud-mssql.sttec.yar.ru,38325;persist security info=True;user id=user182_db;password=user182;encrypt=True;trustservercertificate=True;MultipleActiveResultSets=True;App=EntityFramework\"";
                var builder = new EntityConnectionStringBuilder(entityConnectionString);
                _sqlConnectionString = builder.ProviderConnectionString;

                LoadRoles();
                _currentUser = user;

                if (_currentUser != null) // Режим редактирования
                {
                    Title = "Редактирование пользователя";
                    DataContext = _currentUser;
                }
                else // Режим добавления
                {
                    Title = "Добавление пользователя";
                    _currentUser = new K_Polzovateli();
                    DataContext = _currentUser;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка инициализации окна: " + ex.Message);
            }
        }

        private void LoadRoles()
        {
            var roles = new List<Role>();
            try
            {
                using (var connection = new SqlConnection(_sqlConnectionString))
                {
                    connection.Open();
                    using (var command = new SqlCommand("SELECT rol_id, naimenovaniye FROM K_Roli", connection))
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read()) { roles.Add(new Role { Id = reader.GetInt32(0), Name = reader.GetString(1) }); }
                    }
                }
                ComboRole.ItemsSource = roles;
            }
            catch (Exception ex) { MessageBox.Show("Ошибка загрузки ролей: " + ex.Message); }
        }

        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            // Валидация
            if (string.IsNullOrWhiteSpace(_currentUser.fio) || string.IsNullOrWhiteSpace(_currentUser.login) ||
                (_currentUser.polzovatel_id == 0 && string.IsNullOrWhiteSpace(TBoxPassword.Text)) || // Пароль обязателен только при создании
                ComboRole.SelectedItem == null)
            {
                MessageBox.Show("Все поля (кроме пароля при редактировании) должны быть заполнены.", "Внимание", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Если это новый пользователь, добавляем его в контекст
            if (_currentUser.polzovatel_id == 0)
            {
                _currentUser.parol = TBoxPassword.Text; // Пароль устанавливается только при создании
                Classes.AppContext.GetContext().Set<K_Polzovateli>().Add(_currentUser);
            }
            else
            {
                // Если пароль был изменен в режиме редактирования, обновляем его
                if (!string.IsNullOrWhiteSpace(TBoxPassword.Text))
                {
                    _currentUser.parol = TBoxPassword.Text;
                }
            }

            try
            {
                Classes.AppContext.GetContext().SaveChanges();
                MessageBox.Show("Данные успешно сохранены.", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                this.Close();
            }
            catch (Exception ex)
            {
                // Обработка ошибки уникальности логина
                if (ex.InnerException != null && ex.InnerException.Message.Contains("UNIQUE KEY constraint"))
                {
                    MessageBox.Show("Пользователь с таким логином уже существует!", "Ошибка сохранения", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                else
                {
                    MessageBox.Show("Ошибка сохранения данных: " + ex.Message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
    }
}