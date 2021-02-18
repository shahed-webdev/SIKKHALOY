using AttendanceDevice.Config_Class;
using AttendanceDevice.Model;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;

namespace AttendanceDevice.Settings.Pages
{
    /// <summary>
    /// Interaction logic for DeviceDetailsPage.xaml
    /// </summary>
    public partial class DeviceDetailsPage : Page
    {
        private DeviceConnection DeviceCon;
        private string DeviceID = "";
        public DeviceDetailsPage(DeviceConnection D1)
        {
            DeviceCon = D1;
            InitializeComponent();
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            if (!DeviceCon.IsConnected()) return;

            DeviceName.Text = DeviceCon.Device.DeviceName;
            DeviceIP.Text = DeviceCon.Device.DeviceIP;
            DeviceTime.Text = DeviceCon.GetDateTime().ToString("d MMM yy (hh:mm.tt)");

            var De_Details = DeviceCon.GetDeviceDetails();

            CapacityTB.Text = De_Details.User_capacity.ToString() + "/" + De_Details.Number_of_users.ToString();
            LogCapacity.Text = De_Details.Attendance_Record_Capacity.ToString() + "/" + De_Details.Attendance_Records.ToString();
            FP_Capacity.Text = De_Details.FP_Capacity.ToString() + "/" + De_Details.Number_of_FP.ToString();

            //duplicate time
            btnDuplicateTime.Content = "Set Duplicate Punch Time (" + De_Details.Duplicate_Punch_Time.ToString() + " min)";

            //PC new user
            PCNewUser();
        }

        private async void BtnSyncPcTimetoDevice_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            if (!DeviceCon.IsConnected()) return;

            await Task.Run(() => DeviceCon.SetDateTime());
            DeviceTime.Text = DeviceCon.GetDateTime().ToString("d MMM yyyy (hh:mm tt)");
        }

        private void BtnRestartDevice_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            if (!DeviceCon.IsConnected()) return;

            if (DeviceCon.Restart())
            {
                var DeviceInfo = new DeviceInfoPage();
                NavigationService.Navigate(DeviceInfo);
            }
        }
        private void BtnPowerOffDevice_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            if (!DeviceCon.IsConnected()) return;

            if (DeviceCon.PowerOff())
            {
                var DeviceInfo = new DeviceInfoPage();
                NavigationService.Navigate(DeviceInfo);
            }
        }

        private async void BtnUploadUserDevice_Click(object sender, RoutedEventArgs e)
        {
            if (!DeviceCon.IsConnected()) return;

            LoadingDH.IsOpen = true;
            btnUploadUsers.IsEnabled = false;

            using (var db = new ModelContext())
            {
                var user = db.Users.ToList();
                if (user.Count > 0)
                {
                    await Task.Run(() => DeviceCon.Upload_User(user));
                }
            }

            LoadingDH.IsOpen = false;
            btnUploadUsers.IsEnabled = true;
            NavigationService.Refresh();
        }
        private async void BtnDownloadUsersDevice_Click(object sender, RoutedEventArgs e)
        {
            LoadingDH.IsOpen = true;
            btnDownloadUsers.IsEnabled = false;

            var result = await Task.Run(() => DeviceCon.Download_User().OrderBy(x => x.RFID));

            if (result != null)
            {
                DeviceUserDG.ItemsSource = result;
            }

            btnDownloadUsers.IsEnabled = true;
            LoadingDH.IsOpen = false;

            NavigationService.Refresh();
        }
        private async void PCNewUser()
        {
            LoadingDH.IsOpen = true;
            var DeviceUsers = await Task.Run(() => DeviceCon.Download_User().OrderBy(x => x.RFID));
            var D_Users = DeviceUsers.Select(x => new { x.DeviceID, x.RFID }).ToList();

            List<User> PcUsers = new List<User>();
            using (var db = new ModelContext())
            {
                PcUsers = db.Users.OrderBy(x => x.RFID).ToList();
            }

            var NewUser = PcUsers.Where(Pu => !D_Users.Any(du => Convert.ToInt32(du.RFID) == Convert.ToInt32(Pu.RFID) && du.DeviceID == Pu.DeviceID)).ToList();

            if (NewUser.Count > 0)
            {
                PCNewUserDG.ItemsSource = NewUser;
            }
            else
            {
                PCNewUserDG.Visibility = Visibility.Collapsed;
                Utb.Text = "No new User Found!";
                btnUploadUsers.IsEnabled = false;
            }

            LoadingDH.IsOpen = false;
        }

        //Device Setting
        private async void BtnClearAllUsera_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            LoadingDH.IsOpen = true;
            var deleted = await Task.Run(() => DeviceCon.ClearAllUsers());

            if (deleted)
            {
                LoadingDH.IsOpen = false;
            }
            else
            {
                LoadingDH.IsOpen = false;
                MessageBox.Show("System error", "Error!");
            }
            NavigationService.Refresh();
        }
        private async void BtnAdditionalUserDevice_Click(object sender, RoutedEventArgs e)
        {
            LoadingDH.IsOpen = true;

            var DeviceUsers = await Task.Run(() => DeviceCon.Download_User().OrderBy(x => x.DeviceID));

            List<User> PcUsers = new List<User>();
            using (var db = new ModelContext())
            {
                PcUsers = db.Users.OrderBy(x => x.RFID).ToList();
            }

            var NewUser = DeviceUsers.Where(Pu => !PcUsers.Any(du => du.DeviceID == Pu.DeviceID)).ToList();

            DeviceAddiUserDG.ItemsSource = NewUser;

            LoadingDH.IsOpen = false;
        }
        private async void BtnClearAllfingerprint_Click(object sender, RoutedEventArgs e)
        {
            LoadingDH.IsOpen = true;
            var deleted = await Task.Run(() => DeviceCon.ClearAll_FPs());

            if (deleted)
            {
                LoadingDH.IsOpen = false;
            }
            else
            {
                LoadingDH.IsOpen = false;
                MessageBox.Show("Fingerprint not deleted", "Error!");
            }
            NavigationService.Refresh();
        }
        private async void BtnClearAllLogs_Click(object sender, RoutedEventArgs e)
        {
            LoadingDH.IsOpen = true;
            var deleted = await Task.Run(() => DeviceCon.ClearAll_Logs());

            if (deleted)
            {
                LoadingDH.IsOpen = false;
                AttenLogDG.ItemsSource = null;
            }
            else
            {
                LoadingDH.IsOpen = false;
                MessageBox.Show("log not deleted", "Error!");
            }

            NavigationService.Refresh();
        }
        private async void BtnDownloadLogs_Click(object sender, RoutedEventArgs e)
        {
            LoadingDH.IsOpen = true;
            var logs = await Task.Run(() => DeviceCon.DownloadLogs());

            if (logs.Count > 0)
            {
                AttenLogDG.ItemsSource = logs;
            }

            LoadingDH.IsOpen = false;
            NavigationService.Refresh();
        }
        private void BtnFindUser_Click(object sender, RoutedEventArgs e)
        {
            string filterText = findDeviceidTextBox.Text;
            ICollectionView cv = CollectionViewSource.GetDefaultView(DeviceUserDG.ItemsSource);

            if (!string.IsNullOrEmpty(filterText))
            {
                cv.Filter = o =>
                {
                    User User = o as User;
                    return (User.DeviceID.ToString() == filterText || User.Name.ToUpper().StartsWith(filterText.ToUpper()));
                };
            }
        }

        private async void BtnDuplicateTime_Click(object sender, RoutedEventArgs e)
        {
            LoadingDH.IsOpen = true;
            var d_Reset = await Task.Run(() => DeviceCon.Duplicate_Punch_Time_Reset());

            if (d_Reset)
            {
                LoadingDH.IsOpen = false;
            }
            else
            {
                LoadingDH.IsOpen = false;
                MessageBox.Show("Duplicate time not set", "Error!");
            }
            NavigationService.Refresh();
        }

        private void FindUserButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(UserIDTextbox.Text)) return;

            var Info = LocalData.Instance.UserViews.Where(u => u.ID == UserIDTextbox.Text.Trim()).FirstOrDefault();

            if (Info != null)
            {
                UserInfoGrid.DataContext = Info;
                DeviceID = Info.DeviceID.ToString();
                Userpanel.Visibility = Visibility.Visible;
                UserInfoError.Text = "";

                var FP = LocalData.Instance.Get_UserFP(Info.DeviceID);

                LI.Background = FP.LeftIndex;
                LT.Background = FP.LeftThamb;
                RI.Background = FP.RightIndex;
                RT.Background = FP.RightThamb;
            }
            else
            {
                DeviceID = "";
                Userpanel.Visibility = Visibility.Collapsed;
                UserInfoError.Text = "User Not Found!";
            }
        }

        private void FingerButton_Click(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(DeviceID))
            {
                Button button = sender as Button;
                DeviceCon.FP_Msg = MessageTB;
                var index = Convert.ToInt32(button.CommandParameter);

                if (button.Background != Brushes.GreenYellow)
                {
                    DeviceCon.FP_Add(DeviceID, index);
                }
                else
                {
                    MessageBoxResult messageBoxResult = MessageBox.Show("Delete Fingerprint?", "Finger Delete", MessageBoxButton.YesNo);
                    if (messageBoxResult == MessageBoxResult.Yes)
                    {
                        var ErrorCode = DeviceCon.FP_Delete(DeviceID, index);
                        if (ErrorCode == 0)
                        {
                            LocalData.Instance.Delete_UserFP(Convert.ToInt32(DeviceID), index);
                            button.Background = Brushes.White;
                        }
                        else
                        {
                            //"Operation failed,ErrorCode=" + ErrorCode.ToString();
                        }
                    }
                }
            }
        }
    }
}
