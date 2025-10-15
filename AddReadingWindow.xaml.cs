using ASKUE.Models;
using System;
using System.Windows;

namespace ASKUE
{
    // !!! ИСПРАВЛЕНО: Убрана лишняя вложенность классов !!!
    public partial class AddReadingWindow : Window
    {
        private K_Schetchiki _currentMeter;
        public AddReadingWindow(K_Schetchiki meter)
        {
            InitializeComponent(); // Теперь эта строка будет работать
            _currentMeter = meter;
            Title += $" ({_currentMeter.seriyniy_nomer})";
        }

        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            // TBoxValue теперь будет виден
            if (int.TryParse(TBoxValue.Text, out int value))
            {
                var newReading = new K_Pokazaniya
                {
                    id_schetchika = _currentMeter.schetchik_id,
                    znacheniye = value,
                    data_snyatiya = DateTime.Now
                };
                // Используем полный путь к AppContext во избежание конфликтов
                Classes.AppContext.GetContext().K_Pokazaniya.Add(newReading);
                Classes.AppContext.GetContext().SaveChanges();
                DialogResult = true;
                Close();
            }
            else MessageBox.Show("Введите корректное числовое значение.");
        }
    }
}