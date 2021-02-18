using AttendanceDevice.Config_Class;
using AttendanceDevice.ViewModel;
using System.Windows;
using System.Windows.Controls;

namespace AttendanceDevice.Settings.Pages
{
    /// <summary>
    /// Interaction logic for Deshboard.xaml
    /// </summary>
    public partial class Deshboard : Page
    {
        public Deshboard()
        {
            InitializeComponent();
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            var data = LocalData.Instance.GetErrors();

            if (data.Count > 0)
            {
                ActivityListView.ItemsSource = data;
            }
            else
            {
                ErrorPanel.Visibility = Visibility.Collapsed;
            }

            var Setting_Dashboard = new Setting_Dashboard_View();
            DeshboardData.DataContext = Setting_Dashboard;

        }

        private void ErrorDelete_Button_Click(object sender, RoutedEventArgs e)
        {
            LocalData.Instance.DeleteErrors();
            this.NavigationService.Refresh();
        }
    }
}
