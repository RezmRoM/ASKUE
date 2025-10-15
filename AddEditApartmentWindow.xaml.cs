using ASKUE.Models;
using ASKUE.Pages;
using System;
using System.Linq;
using System.Windows;
namespace ASKUE.Windows
{
    public partial class AddEditApartmentWindow : Window
    {
        private K_Kvartiry _current;
        public AddEditApartmentWindow(AdminApartmentViewModel item)
        {
            InitializeComponent();
            var allUsers = Classes.AppContext.GetContext().Set<K_Polzovateli>().Where(p => p.id_rol == 3).ToList();
            allUsers.Insert(0, new K_Polzovateli { fio = "Не назначен" }); // Опция для не заселенных
            ComboResident.ItemsSource = allUsers;

            if (item == null) { _current = new K_Kvartiry(); Title = "Добавление квартиры"; }
            else
            {
                _current = Classes.AppContext.GetContext().Set<K_Kvartiry>().Find(item.Id);
                Title = "Редактирование квартиры";
                var residentLink = Classes.AppContext.GetContext().Set<K_Prozhivayushchiye>().FirstOrDefault(p => p.id_kvartiry == _current.kvartira_id);
                if (residentLink != null) ComboResident.SelectedValue = residentLink.id_polzovatelya;
            }
            DataContext = _current;
        }
        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(_current.adres) || _current.nomer_kvartiry <= 0) { MessageBox.Show("Заполните адрес и номер."); return; }
            if (_current.kvartira_id == 0) Classes.AppContext.GetContext().Set<K_Kvartiry>().Add(_current);
            try
            {
                Classes.AppContext.GetContext().SaveChanges();
                var residentLink = Classes.AppContext.GetContext().Set<K_Prozhivayushchiye>().FirstOrDefault(p => p.id_kvartiry == _current.kvartira_id);
                if (ComboResident.SelectedIndex > 0)
                {
                    int selectedUserId = (int)ComboResident.SelectedValue;
                    if (residentLink == null) Classes.AppContext.GetContext().Set<K_Prozhivayushchiye>().Add(new K_Prozhivayushchiye { id_kvartiry = _current.kvartira_id, id_polzovatelya = selectedUserId });
                    else residentLink.id_polzovatelya = selectedUserId;
                }
                else if (residentLink != null) Classes.AppContext.GetContext().Set<K_Prozhivayushchiye>().Remove(residentLink);
                Classes.AppContext.GetContext().SaveChanges();
                Close();
            }
            catch (Exception ex) { MessageBox.Show("Ошибка сохранения: " + ex.Message); }
        }
    }
}