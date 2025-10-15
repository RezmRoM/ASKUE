using ASKUE.Models;
using System;
using System.Windows;
using System.Windows.Controls;

namespace ASKUE.Pages
{
    public partial class AddReadingPage : Page
    {
        private K_Schetchiki _currentMeter;
        public AddReadingPage(K_Schetchiki meter)
        {
            InitializeComponent();
            _currentMeter = meter;
            Title += $" ({_currentMeter.seriyniy_nomer})";
        }

        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            if (int.TryParse(TBoxValue.Text, out int value))
            {
                var newReading = new K_Pokazaniya
                {
                    id_schetchika = _currentMeter.schetchik_id,
                    znacheniye = value,
                    data_snyatiya = DateTime.Now
                };
                Classes.AppContext.GetContext().K_Pokazaniya.Add(newReading);
                Classes.AppContext.GetContext().SaveChanges();
                NavigationService.GoBack();
            }
            else MessageBox.Show("Введите корректное числовое значение.");
        }
    }
}
