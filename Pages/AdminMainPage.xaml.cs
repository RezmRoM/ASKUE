using ASKUE.Models;
using ASKUE.Windows;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Entity.Core.EntityClient;
using System.Data.SqlClient;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace ASKUE.Pages
{
    // ViewModels для отображения в DataGrid
    public class AdminUserViewModel { public int Id { get; set; } public string Fio { get; set; } public string Login { get; set; } public string RoleName { get; set; } }
    public class AdminApartmentViewModel { public int Id { get; set; } public string Address { get; set; } public int Number { get; set; } public string ResidentFio { get; set; } }
    public class AdminMeterViewModel { public int Id { get; set; } public string SerialNumber { get; set; } public string ApartmentAddress { get; set; } public string ResourceType { get; set; } public DateTime? InstallDate { get; set; } }
    public class AdminTariffViewModel { public int Id { get; set; } public string ResourceType { get; set; } public decimal Cost { get; set; } public string CostString { get; set; } public DateTime StartDate { get; set; } public DateTime? EndDate { get; set; } public Visibility EndDateVisibility => EndDate.HasValue ? Visibility.Visible : Visibility.Collapsed; }

    public partial class AdminMainPage : Page
    {
        private string _sqlConnectionString;
        // Локальные списки для хранения всех данных
        private List<AdminUserViewModel> _allUsers = new List<AdminUserViewModel>();
        private List<AdminApartmentViewModel> _allApartments = new List<AdminApartmentViewModel>();
        private List<AdminMeterViewModel> _allMeters = new List<AdminMeterViewModel>();
        private List<AdminTariffViewModel> _allTariffs = new List<AdminTariffViewModel>();

        public AdminMainPage()
        {
            InitializeComponent();
            this.Loaded += AdminMainPage_Loaded;
        }

        private void NavigationService_Navigated(object sender, System.Windows.Navigation.NavigationEventArgs e)
        {
            LoadAllData();
            UpdateUsersView(null, null);
            UpdateApartmentsView(null, null);
            UpdateMetersView(null, null);
            UpdateTariffsView(null, null);
        }

        private void AdminMainPage_Loaded(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigated += NavigationService_Navigated;
            try
            {
                _sqlConnectionString = System.Configuration.ConfigurationManager.ConnectionStrings["sqlConnectionString"].ConnectionString;

                LoadAllData(); // Загружаем все данные из БД в фоновые списки
                UpdateUsersView(null, null); // Отображаем первую вкладку при загрузке
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка подключения к базе данных: " + ex.Message);
            }
        }

        private void LoadAllData()
        {
            LoadUsers();
            LoadApartments();
            LoadMeters();
            LoadTariffs();
            // Заполняем фильтр ролей
            var roles = _allUsers.Select(u => u.RoleName).Distinct().ToList();
            roles.Insert(0, "Все роли");
            ComboFilterRole.ItemsSource = roles;
            // Устанавливаем значения по умолчанию для всех ComboBox
            ComboSortUser.SelectedIndex = 0;
            ComboFilterRole.SelectedIndex = 0;
            ComboSortApartment.SelectedIndex = 0;
            ComboSortTariff.SelectedIndex = 0;
        }

        // !!! ИСПРАВЛЕНО: Этот метод теперь ГАРАНТИРОВАННО вызывает обновление нужной вкладки !!!
        private void MainTabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Проверяем, что событие пришло от TabControl и страница уже загружена
            if (e.Source is TabControl && IsLoaded)
            {
                if (MainTabControl.SelectedItem is TabItem selectedTab)
                {
                    switch (selectedTab.Header.ToString())
                    {
                        case "Пользователи": UpdateUsersView(null, null); break;
                        case "Квартиры": UpdateApartmentsView(null, null); break;
                        case "Счетчики": UpdateMetersView(null, null); break;
                        case "Тарифы": UpdateTariffsView(null, null); break;
                    }
                }
            }
        }

        #region Users Logic
        private void LoadUsers()
        {
            _allUsers.Clear();
            string query = "SELECT p.polzovatel_id, p.fio, p.login, r.naimenovaniye FROM K_Polzovateli p JOIN K_Roli r ON p.id_rol = r.rol_id";
            try
            {
                using (var c = new SqlConnection(_sqlConnectionString))
                {
                    c.Open(); using (var cmd = new SqlCommand(query, c)) using (var r = cmd.ExecuteReader())
                    {
                        while (r.Read()) _allUsers.Add(new AdminUserViewModel { Id = r.GetInt32(0), Fio = r.IsDBNull(1) ? "Не указано" : r.GetString(1), Login = r.GetString(2), RoleName = r.GetString(3) });
                    }
                }
            }
            catch (Exception ex) { MessageBox.Show("Ошибка загрузки пользователей: " + ex.Message); }
        }
        private void UpdateUsersView(object sender, RoutedEventArgs e)
        {
            var currentUsers = _allUsers.AsEnumerable();
            if (ComboFilterRole.SelectedIndex > 0)
                currentUsers = currentUsers.Where(u => u.RoleName == ComboFilterRole.SelectedItem.ToString());
            if (!string.IsNullOrWhiteSpace(TBoxSearchUser.Text))
                currentUsers = currentUsers.Where(u => u.Fio.ToLower().Contains(TBoxSearchUser.Text.ToLower()) || u.Login.ToLower().Contains(TBoxSearchUser.Text.ToLower()));
            if (ComboSortUser.SelectedIndex == 0) currentUsers = currentUsers.OrderBy(u => u.Fio);
            else if (ComboSortUser.SelectedIndex == 1) currentUsers = currentUsers.OrderByDescending(u => u.Fio);
            LViewUsers.ItemsSource = currentUsers.ToList();
            TextBlockUserCount.Text = $"Найдено: {currentUsers.Count()} из {_allUsers.Count}";
        }
        private void BtnAddUser_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new AddEditUserPage(null));
        }
        private void BtnEditUser_Click(object sender, RoutedEventArgs e)
        {
            var selectedVm = ((Button)sender).DataContext as AdminUserViewModel; if (selectedVm == null) return;
            var obj = Classes.AppContext.GetContext().Set<K_Polzovateli>().Find(selectedVm.Id);
            NavigationService.Navigate(new AddEditUserPage(obj));
        }
        private void BtnDeleteUser_Click(object sender, RoutedEventArgs e)
        {
            var selectedVm = ((Button)sender).DataContext as AdminUserViewModel; if (selectedVm == null) return;
            if (MessageBox.Show($"Удалить '{selectedVm.Fio}'?", "Подтверждение", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
            {
                try
                {
                    var obj = Classes.AppContext.GetContext().Set<K_Polzovateli>().Find(selectedVm.Id);
                    if (obj != null) { Classes.AppContext.GetContext().Set<K_Polzovateli>().Remove(obj); Classes.AppContext.GetContext().SaveChanges(); LoadAllData(); UpdateUsersView(null, null); }
                }
                catch (Exception ex) { MessageBox.Show("Ошибка удаления: " + ex.Message); }
            }
        }
        #endregion

        #region Apartments Logic
        private void LoadApartments()
        {
            _allApartments.Clear();
            string query = @"SELECT k.kvartira_id, k.adres, k.nomer_kvartiry, p.fio FROM K_Kvartiry k LEFT JOIN K_Prozhivayushchiye pr ON k.kvartira_id = pr.id_kvartiry LEFT JOIN K_Polzovateli p ON pr.id_polzovatelya = p.polzovatel_id";
            try
            {
                using (var c = new SqlConnection(_sqlConnectionString))
                {
                    c.Open(); using (var cmd = new SqlCommand(query, c)) using (var r = cmd.ExecuteReader())
                    {
                        while (r.Read()) _allApartments.Add(new AdminApartmentViewModel { Id = r.GetInt32(0), Address = r.GetString(1), Number = r.GetInt32(2), ResidentFio = r.IsDBNull(3) ? "Не заселена" : r.GetString(3) });
                    }
                }
            }
            catch (Exception ex) { MessageBox.Show("Ошибка загрузки квартир: " + ex.Message); }
        }
        private void UpdateApartmentsView(object sender, RoutedEventArgs e)
        {
            var currentItems = _allApartments.AsEnumerable();
            if (!string.IsNullOrWhiteSpace(TBoxSearchApartment.Text))
                currentItems = currentItems.Where(a => a.Address.ToLower().Contains(TBoxSearchApartment.Text.ToLower()) || a.ResidentFio.ToLower().Contains(TBoxSearchApartment.Text.ToLower()));
            if (ComboSortApartment.SelectedIndex == 0) currentItems = currentItems.OrderBy(a => a.Address).ThenBy(a => a.Number);
            else if (ComboSortApartment.SelectedIndex == 1) currentItems = currentItems.OrderByDescending(a => a.Address).ThenByDescending(a => a.Number);
            LViewApartments.ItemsSource = currentItems.ToList();
            TextBlockApartmentCount.Text = $"Найдено: {currentItems.Count()} из {_allApartments.Count}";
        }
        private void BtnAddApartment_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new AddEditApartmentPage(null));
        }
        private void BtnEditApartment_Click(object sender, RoutedEventArgs e)
        {
            var selectedVm = ((Button)sender).DataContext as AdminApartmentViewModel; if (selectedVm == null) return;
            NavigationService.Navigate(new AddEditApartmentPage(selectedVm));
        }
        private void BtnDeleteApartment_Click(object sender, RoutedEventArgs e)
        {
            var selectedVm = ((Button)sender).DataContext as AdminApartmentViewModel; if (selectedVm == null) return;
            if (MessageBox.Show($"Удалить квартиру {selectedVm.Address}, {selectedVm.Number}?", "Подтверждение", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
            {
                try
                {
                    var obj = Classes.AppContext.GetContext().Set<K_Kvartiry>().Find(selectedVm.Id);
                    if (obj != null) { Classes.AppContext.GetContext().Set<K_Kvartiry>().Remove(obj); Classes.AppContext.GetContext().SaveChanges(); LoadAllData(); UpdateApartmentsView(null, null); }
                }
                catch (Exception ex) { MessageBox.Show("Ошибка удаления: " + ex.Message); }
            }
        }
        #endregion

        #region Meters Logic
        private void LoadMeters()
        {
            _allMeters.Clear();
            string query = @"SELECT s.schetchik_id, s.seriyniy_nomer, k.adres + ', кв. ' + CAST(k.nomer_kvartiry AS VARCHAR(10)), tr.naimenovaniye, s.data_ustanovki FROM K_Schetchiki s JOIN K_Kvartiry k ON s.id_kvartiry = k.kvartira_id JOIN K_TipyResursov tr ON s.id_tipa_resursa = tr.tip_resursa_id";
            try
            {
                using (var c = new SqlConnection(_sqlConnectionString))
                {
                    c.Open(); using (var cmd = new SqlCommand(query, c)) using (var r = cmd.ExecuteReader())
                    {
                        while (r.Read()) _allMeters.Add(new AdminMeterViewModel { Id = r.GetInt32(0), SerialNumber = r.GetString(1), ApartmentAddress = r.GetString(2), ResourceType = r.GetString(3), InstallDate = r.IsDBNull(4) ? (DateTime?)null : r.GetDateTime(4) });
                    }
                }
            }
            catch (Exception ex) { MessageBox.Show("Ошибка загрузки счетчиков: " + ex.Message); }
        }
        private void UpdateMetersView(object sender, RoutedEventArgs e)
        {
            var currentItems = _allMeters.AsEnumerable();
            if (!string.IsNullOrWhiteSpace(TBoxSearchMeter.Text))
                currentItems = currentItems.Where(m => m.SerialNumber.ToLower().Contains(TBoxSearchMeter.Text.ToLower()) || m.ApartmentAddress.ToLower().Contains(TBoxSearchMeter.Text.ToLower()));
            LViewMeters.ItemsSource = currentItems.OrderBy(m => m.SerialNumber).ToList();
            TextBlockMeterCount.Text = $"Найдено: {currentItems.Count()} из {_allMeters.Count}";
        }
        private void BtnAddMeter_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new AddEditMeterPage(null));
        }
        private void BtnEditMeter_Click(object sender, RoutedEventArgs e)
        {
            var selectedVm = ((Button)sender).DataContext as AdminMeterViewModel; if (selectedVm == null) return;
            var obj = Classes.AppContext.GetContext().Set<K_Schetchiki>().Find(selectedVm.Id);
            NavigationService.Navigate(new AddEditMeterPage(obj));
        }
        private void BtnDeleteMeter_Click(object sender, RoutedEventArgs e)
        {
            var selectedVm = ((Button)sender).DataContext as AdminMeterViewModel; if (selectedVm == null) return;
            if (MessageBox.Show($"Удалить счетчик {selectedVm.SerialNumber}?", "Подтверждение", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
            {
                try
                {
                    var obj = Classes.AppContext.GetContext().Set<K_Schetchiki>().Find(selectedVm.Id);
                    if (obj != null) { Classes.AppContext.GetContext().Set<K_Schetchiki>().Remove(obj); Classes.AppContext.GetContext().SaveChanges(); LoadAllData(); UpdateMetersView(null, null); }
                }
                catch (Exception ex) { MessageBox.Show("Ошибка удаления: " + ex.Message); }
            }
        }
        #endregion

        #region Tariffs Logic
        private void LoadTariffs()
        {
            _allTariffs.Clear();
            string query = @"SELECT t.tarif_id, tr.naimenovaniye, t.stoimost_za_edinitsu, t.data_nachala_deystviya, t.data_okonchaniya_deystviya FROM K_Tarify t JOIN K_TipyResursov tr ON t.id_tipa_resursa = tr.tip_resursa_id";
            try
            {
                var ruCulture = new CultureInfo("ru-RU");
                using (var c = new SqlConnection(_sqlConnectionString))
                {
                    c.Open(); using (var cmd = new SqlCommand(query, c)) using (var r = cmd.ExecuteReader())
                    {
                        while (r.Read())
                        {
                            decimal cost = r.GetDecimal(2);
                            _allTariffs.Add(new AdminTariffViewModel { Id = r.GetInt32(0), ResourceType = r.GetString(1), Cost = cost, CostString = cost.ToString("C2", ruCulture), StartDate = r.GetDateTime(3), EndDate = r.IsDBNull(4) ? (DateTime?)null : r.GetDateTime(4) });
                        }
                    }
                }
            }
            catch (Exception ex) { MessageBox.Show("Ошибка загрузки тарифов: " + ex.Message); }
        }
        private void UpdateTariffsView(object sender, RoutedEventArgs e)
        {
            var currentItems = _allTariffs.AsEnumerable();
            if (ComboSortTariff.SelectedIndex == 0) currentItems = currentItems.OrderBy(t => t.Cost);
            else if (ComboSortTariff.SelectedIndex == 1) currentItems = currentItems.OrderByDescending(t => t.Cost);
            LViewTariffs.ItemsSource = currentItems.ToList();
            TextBlockTariffCount.Text = $"Найдено: {currentItems.Count()} из {_allTariffs.Count}";
        }
        private void BtnAddTariff_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new AddEditTariffPage(null));
        }
        private void BtnEditTariff_Click(object sender, RoutedEventArgs e)
        {
            var selectedVm = ((Button)sender).DataContext as AdminTariffViewModel; if (selectedVm == null) return;
            var obj = Classes.AppContext.GetContext().Set<K_Tarify>().Find(selectedVm.Id);
            NavigationService.Navigate(new AddEditTariffPage(obj));
        }
        private void BtnDeleteTariff_Click(object sender, RoutedEventArgs e)
        {
            var selectedVm = ((Button)sender).DataContext as AdminTariffViewModel; if (selectedVm == null) return;
            if (MessageBox.Show($"Удалить тариф для {selectedVm.ResourceType}?", "Подтверждение", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
            {
                try
                {
                    var obj = Classes.AppContext.GetContext().Set<K_Tarify>().Find(selectedVm.Id);
                    if (obj != null) { Classes.AppContext.GetContext().Set<K_Tarify>().Remove(obj); Classes.AppContext.GetContext().SaveChanges(); LoadAllData(); UpdateTariffsView(null, null); }
                }
                catch (Exception ex) { MessageBox.Show("Ошибка удаления: " + ex.Message); }
            }
        }
        #endregion
    }
}