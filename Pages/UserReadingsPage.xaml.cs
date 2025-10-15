using ASKUE.Classes;
using ASKUE.Models;
using System.Linq;
using System.Windows.Controls;

namespace ASKUE.Pages
{
    public partial class UserReadingsPage : Page
    {
        public UserReadingsPage()
        {
            InitializeComponent();
            // !!! ИСПРАВЛЕНО: Находим ID квартиры через связующую таблицу K_Prozhivayushchiye !!!
            var userLink = ASKUE.Classes.AppContext.GetContext().K_Prozhivayushchiye
                .FirstOrDefault(p => p.id_polzovatelya == ASKUE.Classes.AppContext.CurrentUser.polzovatel_id);

            if (userLink != null)
            {
                var apartmentId = userLink.id_kvartiry;
                ComboMeters.ItemsSource = ASKUE.Classes.AppContext.GetContext().K_Schetchiki.Where(s => s.id_kvartiry == apartmentId).ToList();
            }
            NavigationService.Navigated += NavigationService_Navigated;
        }

        private void NavigationService_Navigated(object sender, System.Windows.Navigation.NavigationEventArgs e)
        {
            ComboMeters_SelectionChanged(null, null);
        }

        private void ComboMeters_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ComboMeters.SelectedItem is K_Schetchiki selectedMeter)
            {
                ReadingsGrid.ItemsSource = ASKUE.Classes.AppContext.GetContext().K_Pokazaniya.Where(p => p.id_schetchika == selectedMeter.schetchik_id).OrderByDescending(p => p.data_snyatiya).ToList();
            }
        }

        private void BtnAddReading_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            if (ComboMeters.SelectedItem is K_Schetchiki selectedMeter)
            {
                NavigationService.Navigate(new AddReadingPage(selectedMeter));
            }
            else
            {
                System.Windows.MessageBox.Show("Пожалуйста, выберите счетчик.", "Внимание");
            }
        }
    }
}