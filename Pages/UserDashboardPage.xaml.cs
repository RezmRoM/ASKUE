using System;
using System.Collections.Generic;
using System.Data.Entity.Core.EntityClient;
using System.Data.SqlClient;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;

namespace ASKUE.Pages
{
    public class MeterViewModel
    {
        public string ResourceName { get; set; }
        public string Color { get; set; }
        public int Consumption { get; set; }
        public string CostString { get; set; }
        public string Unit { get; set; }
    }

    public partial class UserDashboardPage : Page
    {
        public UserDashboardPage()
        {
            InitializeComponent();
            LoadDataWithRawSql();
            CheckReadingSubmissionStatus();
        }

        private void LoadDataWithRawSql()
        {
            var viewModels = new List<MeterViewModel>();
            decimal totalCost = 0;
            string sqlConnectionString = "";

            try
            {
                var entityConnectionString = "metadata=res://*/Models.Model1.csdl|res://*/Models.Model1.ssdl|res://*/Models.Model1.msl;provider=System.Data.SqlClient;provider connection string=\"data source=stud-mssql.sttec.yar.ru,38325;persist security info=True;user id=user182_db;password=user182;encrypt=True;trustservercertificate=True;MultipleActiveResultSets=True;App=EntityFramework\"";

                var builder = new EntityConnectionStringBuilder(entityConnectionString);
                sqlConnectionString = builder.ProviderConnectionString;

                // !!! ФИНАЛЬНЫЙ, АБСОЛЮТНО КОРРЕКТНЫЙ SQL-ЗАПРОС !!!
                string sqlQuery = @"
                    -- Определяем последний месяц, за который есть данные
                    DECLARE @LastReadingMonth DATE = (SELECT TOP 1 DATEFROMPARTS(YEAR(data_snyatiya), MONTH(data_snyatiya), 1) 
                                                     FROM K_Pokazaniya ORDER BY data_snyatiya DESC);
                    
                    -- Собираем данные
                    WITH MonthlyReadings AS (
                        -- Находим первое и последнее показание для каждого счетчика ВНУТРИ последнего месяца
                        SELECT 
                            id_schetchika,
                            MIN(znacheniye) AS FirstValue,
                            MAX(znacheniye) AS LastValue
                        FROM K_Pokazaniya
                        WHERE DATEFROMPARTS(YEAR(data_snyatiya), MONTH(data_snyatiya), 1) = @LastReadingMonth
                        GROUP BY id_schetchika
                    )
                    SELECT 
                        k.adres, k.nomer_kvartiry,
                        tr.naimenovaniye, tr.edinitsa_izmereniya,
                        ISNULL(mr.LastValue, 0) - ISNULL(mr.FirstValue, 0) as Consumption,
                        t.stoimost_za_edinitsu
                    FROM K_Prozhivayushchiye AS pr
                    JOIN K_Kvartiry AS k ON pr.id_kvartiry = k.kvartira_id
                    JOIN K_Schetchiki AS s ON k.kvartira_id = s.id_kvartiry
                    JOIN K_TipyResursov AS tr ON s.id_tipa_resursa = tr.tip_resursa_id
                    JOIN K_Tarify AS t ON tr.tip_resursa_id = t.id_tipa_resursa
                    LEFT JOIN MonthlyReadings mr ON s.schetchik_id = mr.id_schetchika
                    WHERE pr.id_polzovatelya = @userId AND t.data_okonchaniya_deystviya IS NULL";

                using (SqlConnection connection = new SqlConnection(sqlConnectionString))
                {
                    connection.Open();
                    using (SqlCommand command = new SqlCommand(sqlQuery, connection))
                    {
                        command.Parameters.AddWithValue("@userId", ASKUE.Classes.AppContext.CurrentUser.polzovatel_id);

                        var colors = new List<string> { "#ec4899", "#0ea5e9", "#f59e0b", "#06b6d4" };
                        int colorIndex = 0;
                        var ruCulture = new CultureInfo("ru-RU");

                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                if (string.IsNullOrEmpty(TBlockAddress.Text) || TBlockAddress.Text == "Загрузка адреса...")
                                {
                                    string address = reader.GetString(0);
                                    int apartmentNumber = reader.GetInt32(1);
                                    TBlockAddress.Text = $"Адрес: {address}, кв. {apartmentNumber}";
                                }

                                int consumption = reader.IsDBNull(4) ? 0 : reader.GetInt32(4);
                                decimal tariff = reader.GetDecimal(5);
                                decimal cost = consumption * tariff;
                                totalCost += cost;

                                viewModels.Add(new MeterViewModel
                                {
                                    ResourceName = reader.GetString(2),
                                    Unit = reader.GetString(3),
                                    Consumption = consumption,
                                    CostString = cost.ToString("C2", ruCulture),
                                    Color = colors[colorIndex % colors.Count]
                                });
                                colorIndex++;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка при загрузке данных: " + ex.Message);
            }

            MetersDataItemsControl.ItemsSource = viewModels;
            var russianCulture = new CultureInfo("ru-RU");
            TBlockTotalCost.Text = totalCost.ToString("C2", russianCulture);
        }

        private void CheckReadingSubmissionStatus()
        {
            int currentDay = DateTime.Now.Day;
            if (currentDay >= 15 && currentDay <= 25)
            {
                BtnSubmitReadings.IsEnabled = true;
                BtnSubmitReadings.Content = "Подать показания за текущий месяц";
            }
            else
            {
                BtnSubmitReadings.IsEnabled = false;
                BtnSubmitReadings.Content = "Подача показаний доступна с 15 по 25 число";
            }
        }

        private void BtnSubmitReadings_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new SubmitReadingsPage());
        }
    }
}