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
    public partial class AddEditTariffPage : Page
    {
        private K_Tarify _current;
        public AddEditTariffPage(K_Tarify item)
        {
            InitializeComponent();
            LoadResourceTypes();
            if (item == null)
            {
                _current = new K_Tarify { data_nachala_deystviya = DateTime.Today };
                Title = "Добавление тарифа";
            }
            else
            {
                _current = item;
                Title = "Редактирование тарифа";
            }
            DataContext = _current;
        }

        private void LoadResourceTypes()
        {
            var resourceTypes = new List<ResourceType>(); // Используем класс из AddEditMeterPage
            try
            {
                string sqlConnectionString = "metadata=res://*/Models.Model1.csdl|res://*/Models.Model1.ssdl|res://*/Models.Model1.msl;provider=System.Data.SqlClient;provider connection string=\"data source=stud-mssql.sttec.yar.ru,38325;persist security info=True;user id=user182_db;password=user182;encrypt=True;trustservercertificate=True;MultipleActiveResultSets=True;App=EntityFramework\"";
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
                ComboResourceType.DisplayMemberPath = "Name";
                ComboResourceType.SelectedValuePath = "Id";
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка загрузки типов ресурсов: " + ex.Message);
            }
        }

        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            if (_current.id_tipa_resursa == 0 || _current.stoimost_za_edinitsu <= 0)
            {
                MessageBox.Show("Выберите тип ресурса и укажите стоимость.");
                return;
            }
            if (_current.tarif_id == 0)
            {
                Classes.AppContext.GetContext().Set<K_Tarify>().Add(_current);
            }
            try
            {
                Classes.AppContext.GetContext().SaveChanges();
                NavigationService.GoBack();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка сохранения: " + ex.Message);
            }
        }
    }
}
