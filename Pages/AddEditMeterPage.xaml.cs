using ASKUE.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity.Core.EntityClient;
using System.Data.SqlClient;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using ASKUE.Classes;

namespace ASKUE.Pages
{
    public partial class AddEditMeterPage : Page
    {
        private K_Schetchiki _current;
        public AddEditMeterPage(K_Schetchiki item)
        {
            InitializeComponent();
            LoadApartments();
            LoadResourceTypes();

            if (item == null)
            {
                _current = new K_Schetchiki { data_ustanovki = DateTime.Today };
                Title = "Добавление счетчика";
            }
            else
            {
                _current = item;
                Title = "Редактирование счетчика";
            }
            DataContext = _current;
        }

        private void LoadApartments()
        {
            var apartments = Classes.AppContext.GetContext().Set<K_Kvartiry>().ToList()
                .Select(k => new { Id = k.kvartira_id, Display = $"{k.adres}, кв. {k.nomer_kvartiry}" }).ToList();
            ComboApartment.ItemsSource = apartments;
            ComboApartment.DisplayMemberPath = "Display";
            ComboApartment.SelectedValuePath = "Id";
        }

        private void LoadResourceTypes()
        {
            var resourceTypes = new List<ResourceType>();
            try
            {
                string sqlConnectionString = System.Configuration.ConfigurationManager.ConnectionStrings["sqlConnectionString"].ConnectionString;
                using (var c = new SqlConnection(sqlConnectionString))
                {
                    c.Open();
                    using (var cmd = new SqlCommand("SELECT tip_resursa_id, naimenovaniye FROM K_TipyResursov", c))
                    using (var r = cmd.ExecuteReader())
                    {
                        while (r.Read())
                        {
                            resourceTypes.Add(new ResourceType { Id = r.GetInt32(0), Name = r.GetString(1) });
                        }
                    }
                }
                ComboResourceType.ItemsSource = resourceTypes;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка загрузки типов ресурсов: " + ex.Message);
            }
        }

        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(_current.seriyniy_nomer) || _current.id_kvartiry == 0 || _current.id_tipa_resursa == 0)
            {
                MessageBox.Show("Заполните все поля.");
                return;
            }
            if (_current.schetchik_id == 0)
            {
                Classes.AppContext.GetContext().Set<K_Schetchiki>().Add(_current);
            }
            try
            {
                Classes.AppContext.GetContext().SaveChanges();
                NavigationService.GoBack();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка сохранения: " + (ex.InnerException?.Message ?? ex.Message));
            }
        }
    }
}
