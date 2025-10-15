using ASKUE.Models; // Этот using может понадобиться для других частей, оставляем
using System;
using System.Collections.Generic;
using System.Configuration; // <-- Добавьте эту строку
using System.Data.Entity.Core.EntityClient; // <-- Добавьте эту строку
using System.Data.SqlClient; // <-- Добавьте эту строку
using System.Windows;
using System.Windows.Controls;

namespace ASKUE.Pages
{
    // Вспомогательный класс для удобного отображения данных в таблице
    public class EmployeeMeterViewModel
    {
        public int Id { get; set; }
        public string SeriyniyNomer { get; set; }
        public string AdresKvartiry { get; set; }
        public string TipResursa { get; set; }
        public DateTime? DataUstanovki { get; set; }
    }

    public partial class EmployeeMetersPage : Page
    {
        public EmployeeMetersPage()
        {
            InitializeComponent();
            LoadDataWithRawSql(); // Вызываем новый метод загрузки
        }

        private void LoadDataWithRawSql()
        {
            var viewModels = new List<EmployeeMeterViewModel>();
            string sqlConnectionString = "";

            try
            {
                // 1. Получаем строку подключения из App.config
                var entityConnectionString = "metadata=res://*/Models.Model1.csdl|res://*/Models.Model1.ssdl|res://*/Models.Model1.msl;provider=System.Data.SqlClient;provider connection string=\"data source=stud-mssql.sttec.yar.ru,38325;persist security info=True;user id=user182_db;password=user182;encrypt=True;trustservercertificate=True;MultipleActiveResultSets=True;App=EntityFramework\"";
                var builder = new EntityConnectionStringBuilder(entityConnectionString);
                sqlConnectionString = builder.ProviderConnectionString;

                // 2. Определяем SQL-запрос с JOIN'ами
                string sqlQuery = @"
                    SELECT 
                        s.schetchik_id,
                        s.seriyniy_nomer,
                        k.adres,
                        tr.naimenovaniye,
                        s.data_ustanovki
                    FROM 
                        K_Schetchiki AS s
                    INNER JOIN 
                        K_Kvartiry AS k ON s.id_kvartiry = k.kvartira_id
                    INNER JOIN 
                        K_TipyResursov AS tr ON s.id_tipa_resursa = tr.tip_resursa_id";

                // 3. Выполняем запрос с помощью ADO.NET
                using (SqlConnection connection = new SqlConnection(sqlConnectionString))
                {
                    connection.Open();
                    using (SqlCommand command = new SqlCommand(sqlQuery, connection))
                    {
                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            if (reader.HasRows)
                            {
                                while (reader.Read())
                                {
                                    viewModels.Add(new EmployeeMeterViewModel
                                    {
                                        Id = reader.GetInt32(0),
                                        SeriyniyNomer = reader.GetString(1),
                                        AdresKvartiry = reader.GetString(2),
                                        TipResursa = reader.GetString(3),
                                        // Проверяем, что дата не NULL
                                        DataUstanovki = reader.IsDBNull(4) ? (DateTime?)null : reader.GetDateTime(4)
                                    });
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка при загрузке данных из базы: \n" + ex.Message);
            }

            // 4. Привязываем результат к таблице
            MetersGrid.ItemsSource = viewModels;
        }


        private void BtnGoToTariffs_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Страница управления тарифами в разработке.");
        }

        private void BtnAdd_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Функция добавления счетчика в разработке.");
        }
    }
}