using AttendanceDevice.Config_Class;
using AttendanceDevice.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace AttendanceDevice.Settings.Pages
{
    /// <summary>
    /// Interaction logic for BackUp_LogsPage.xaml
    /// </summary>
    public partial class BackUp_LogsPage : Page
    {
        private List<Log_Backups_View> _log_Backups = new List<Log_Backups_View>();
        private string user_type = "";
        private DateTime fdate = new DateTime(2000, 1, 1);
        private DateTime tdate = new DateTime(3000, 1, 1);
        public BackUp_LogsPage()
        {
            _log_Backups = LocalData.Instance.Get_Log_Backup();
            InitializeComponent();
        }
        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            LogDG.ItemsSource = _log_Backups;
        }


        private List<Log_Backups_View> data_filter()
        {

            var sd = string.IsNullOrEmpty(FromDate.Text) ? "1/1/2000" : FromDate.Text;
            var td = string.IsNullOrEmpty(ToDate.Text) ? "1/1/3000" : ToDate.Text;

            DateTime.TryParse(sd, out fdate);
            DateTime.TryParse(td, out tdate);

            if (user_type == "Student")
            {
                return _log_Backups.Where(a => a.dtEntry_Date >= fdate && a.dtEntry_Date <= tdate && a.Is_Student).ToList();
            }
            else if (user_type == "Employee")
            {
                return _log_Backups.Where(a => a.dtEntry_Date >= fdate && a.dtEntry_Date <= tdate && !a.Is_Student).ToList();
            }
            else
            {
                return _log_Backups.Where(a => a.dtEntry_Date >= fdate && a.dtEntry_Date <= tdate).ToList();
            }
        }
        private void RadioButton_Checked(object sender, RoutedEventArgs e)
        {
            var UserRadioButton = (sender as RadioButton);

            user_type = UserRadioButton.Content.ToString();
            LogDG.ItemsSource = data_filter();


        }

        private void BtnFind_Click(object sender, RoutedEventArgs e)
        {
            LogDG.ItemsSource = data_filter();
        }

        private void UploadRecord_Button_Click(object sender, RoutedEventArgs e)
        {

        }

        private void DeleteRecord_Button_Click(object sender, RoutedEventArgs e)
        {
            var IDs = data_filter().Select(l => l.DeviceID).ToList();

            if (IDs.Count > 0)
            {
                LocalData.Instance.Delete_Log_Backup(fdate, tdate, IDs);
                this.NavigationService.Refresh();
            }
        }
    }
}
