using ASKUE.Classes;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Entity.Core.EntityClient;
using System.Data.SqlClient;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Navigation;

namespace ASKUE.Pages
{
    // Вспомогательный класс для удобной передачи данных на страницу
    public class MeterToSubmitViewModel
    {
        public int MeterId { get; set; }
        public string ResourceName { get; set; }
        public string SerialNumber { get; set; }
        public int LastValue { get; set; }
        public string NewValueText { get; set; } // Используем string для отслеживания пустоты
    }

    public partial class SubmitReadingsPage : Page
    {
        private List<MeterToSubmitViewModel> _metersToSubmit = new List<MeterToSubmitViewModel>();

        public SubmitReadingsPage()
        {
            InitializeComponent();
            LoadMetersWithRawSql();
        }

        private void LoadMetersWithRawSql()
        {
            string sqlConnectionString = "";
            try
            {
                var entityConnectionString = "metadata=res://*/Models.Model1.csdl|res://*/Models.Model1.ssdl|res://*/Models.Model1.msl;provider=System.Data.SqlClient;provider connection string=\"data source=stud-mssql.sttec.yar.ru,38325;persist security info=True;user id=user182_db;password=user182;encrypt=True;trustservercertificate=True;MultipleActiveResultSets=True;App=EntityFramework\"";
                var builder = new EntityConnectionStringBuilder(entityConnectionString);
                sqlConnectionString = builder.ProviderConnectionString;

                string sqlQuery = @"
                    SELECT 
                        s.schetchik_id,
                        tr.naimenovaniye,
                        s.seriyniy_nomer,
                        ISNULL(p1.znacheniye, 0) AS LastValue
                    FROM K_Prozhivayushchiye AS pr
                    JOIN K_Schetchiki AS s ON pr.id_kvartiry = s.id_kvartiry
                    JOIN K_TipyResursov AS tr ON s.id_tipa_resursa = tr.tip_resursa_id
                    OUTER APPLY (
                        SELECT TOP 1 znacheniye
                        FROM K_Pokazaniya 
                        WHERE id_schetchika = s.schetchik_id
                        ORDER BY data_snyatiya DESC
                    ) AS p1
                    WHERE pr.id_polzovatelya = @userId";

                using (SqlConnection connection = new SqlConnection(sqlConnectionString))
                {
                    connection.Open();
                    using (SqlCommand command = new SqlCommand(sqlQuery, connection))
                    {
                        command.Parameters.AddWithValue("@userId", ASKUE.Classes.AppContext.CurrentUser.polzovatel_id);
                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                _metersToSubmit.Add(new MeterToSubmitViewModel
                                {
                                    MeterId = reader.GetInt32(0),
                                    ResourceName = reader.GetString(1),
                                    SerialNumber = reader.GetString(2),
                                    LastValue = reader.GetInt32(3)
                                });
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка при загрузке счетчиков: " + ex.Message);
            }

            MetersToSubmitItemsControl.ItemsSource = _metersToSubmit;
        }

        private void TBoxNewValue_TextChanged(object sender, TextChangedEventArgs e)
        {
            var textBox = sender as TextBox;
            var grid = FindParent<Grid>(textBox);
            if (grid == null) return;
            var errorHint = grid.FindName("ErrorHint") as TextBlock;
            var meterId = (int)textBox.Tag;
            var meterViewModel = _metersToSubmit.FirstOrDefault(m => m.MeterId == meterId);

            if (meterViewModel == null) return;

            // Сохраняем текстовое значение в модель. Binding в XAML сделает остальное.
            // meterViewModel.NewValueText = textBox.Text;

            if (string.IsNullOrWhiteSpace(textBox.Text))
            {
                textBox.BorderBrush = Brushes.Gray;
                textBox.BorderThickness = new Thickness(1);
                errorHint.Text = "";
                return;
            }

            if (int.TryParse(textBox.Text, out int newValue))
            {
                if (newValue < meterViewModel.LastValue)
                {
                    textBox.BorderBrush = Brushes.Red;
                    textBox.BorderThickness = new Thickness(2);
                    errorHint.Text = "Новое значение не может быть меньше предыдущего!";
                }
                else
                {
                    textBox.BorderBrush = Brushes.Green; // Позитивная обратная связь
                    textBox.BorderThickness = new Thickness(2);
                    errorHint.Text = "";
                }
            }
            else
            {
                textBox.BorderBrush = Brushes.Red;
                textBox.BorderThickness = new Thickness(2);
                errorHint.Text = "Введите число";
            }
        }

        private void BtnSend_Click(object sender, RoutedEventArgs e)
        {
            // Проверка #1: Убеждаемся, что хотя бы одно показание введено.
            if (_metersToSubmit.All(m => string.IsNullOrWhiteSpace(m.NewValueText)))
            {
                MessageBox.Show("Вы не ввели ни одного показания.", "Внимание", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Проверка #2: Убеждаемся, что все введенные данные корректны.
            foreach (var meter in _metersToSubmit)
            {
                if (!string.IsNullOrWhiteSpace(meter.NewValueText))
                {
                    if (!int.TryParse(meter.NewValueText, out int val) || val < meter.LastValue)
                    {
                        MessageBox.Show($"Ошибка в показаниях для '{meter.ResourceName}'.\nПроверьте введенные данные.", "Ошибка валидации");
                        return;
                    }
                }
            }

            // Если все проверки пройдены, сохраняем в базу.
            try
            {
                var entityConnectionString = "metadata=res://*/Models.Model1.csdl|res://*/Models.Model1.ssdl|res://*/Models.Model1.msl;provider=System.Data.SqlClient;provider connection string=\"data source=stud-mssql.sttec.yar.ru,38325;persist security info=True;user id=user182_db;password=user182;encrypt=True;trustservercertificate=True;MultipleActiveResultSets=True;App=EntityFramework\"";
                var builder = new EntityConnectionStringBuilder(entityConnectionString);
                string sqlConnectionString = builder.ProviderConnectionString;

                using (SqlConnection connection = new SqlConnection(sqlConnectionString))
                {
                    connection.Open();
                    foreach (var meter in _metersToSubmit)
                    {
                        if (!string.IsNullOrWhiteSpace(meter.NewValueText) && int.TryParse(meter.NewValueText, out int newValue))
                        {
                            string insertQuery = "INSERT INTO K_Pokazaniya (id_schetchika, znacheniye, data_snyatiya) VALUES (@meterId, @value, @date)";
                            using (SqlCommand command = new SqlCommand(insertQuery, connection))
                            {
                                command.Parameters.AddWithValue("@meterId", meter.MeterId);
                                command.Parameters.AddWithValue("@value", newValue);
                                command.Parameters.AddWithValue("@date", DateTime.Now);
                                command.ExecuteNonQuery();
                            }
                        }
                    }
                }
                MessageBox.Show("Спасибо, ваши показания приняты!", "Успешно");
                if (NavigationService.CanGoBack) NavigationService.GoBack();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка при сохранении показаний: " + ex.Message);
            }
        }

        // Вспомогательный метод для поиска родительского элемента в дереве WPF
        public static T FindParent<T>(DependencyObject child) where T : DependencyObject
        {
            DependencyObject parentObject = VisualTreeHelper.GetParent(child);
            if (parentObject == null) return null;
            T parent = parentObject as T;
            return parent ?? FindParent<T>(parentObject);
        }
    }
}