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
        private readonly DeviceConnection _deviceCon;
        private string _deviceId = "";
        public DeviceDetailsPage(DeviceConnection d1)
        {
            _deviceCon = d1;
            InitializeComponent();
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            if (!_deviceCon.IsConnected()) return;

            DeviceName.Text = _deviceCon.Device.DeviceName;
            DeviceIP.Text = _deviceCon.Device.DeviceIP;
            DeviceTime.Text = _deviceCon.GetDateTime().ToString("d MMM yy (hh:mm.tt)");

            var deDetails = _deviceCon.GetDeviceDetails();

            CapacityTB.Text = deDetails.User_capacity + "/" + deDetails.Number_of_users;
            LogCapacity.Text = deDetails.Attendance_Record_Capacity + "/" + deDetails.Attendance_Records;
            FP_Capacity.Text = deDetails.FP_Capacity + "/" + deDetails.Number_of_FP;

            //duplicate time
            btnDuplicateTime.Content = "Set Duplicate Punch Time (" + deDetails.Duplicate_Punch_Time + " min)";

            //PC new user
            PcNewUser();
        }

        private async void BtnSyncPcTimetoDevice_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            if (!await _deviceCon.IsConnectedAsync()) return;

            await Task.Run(() => _deviceCon.SetDateTime());
            DeviceTime.Text = _deviceCon.GetDateTime().ToString("d MMM yyyy (hh:mm tt)");
        }

        private void BtnRestartDevice_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            if (!_deviceCon.IsConnected()) return;

            if (!_deviceCon.Restart()) return;

            var deviceInfo = new DeviceInfoPage();
            NavigationService?.Navigate(deviceInfo);
        }
        private void BtnPowerOffDevice_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            if (!_deviceCon.IsConnected()) return;

            if (!_deviceCon.PowerOff()) return;

            var deviceInfo = new DeviceInfoPage();
            NavigationService?.Navigate(deviceInfo);
        }

        private async void BtnUploadUserDevice_Click(object sender, RoutedEventArgs e)
        {
            if (!await _deviceCon.IsConnectedAsync()) return;

            LoadingDH.IsOpen = true;
            btnUploadUsers.IsEnabled = false;

            using (var db = new ModelContext())
            {
                var user = db.Users.ToList();
                if (user.Count > 0)
                {
                    await Task.Run(() => _deviceCon.Upload_User(user));
                }
            }

            LoadingDH.IsOpen = false;
            btnUploadUsers.IsEnabled = true;
            NavigationService?.Refresh();
        }
        private async void BtnDownloadUsersDevice_Click(object sender, RoutedEventArgs e)
        {
            LoadingDH.IsOpen = true;
            btnDownloadUsers.IsEnabled = false;

            var result = await Task.Run(() => _deviceCon.Download_User().OrderBy(x => x.RFID));

            if (result != null)
            {
                DeviceUserDG.ItemsSource = result;
            }

            btnDownloadUsers.IsEnabled = true;
            LoadingDH.IsOpen = false;

            NavigationService?.Refresh();
        }
        private async void PcNewUser()
        {
            LoadingDH.IsOpen = true;
            var deviceUsers = await Task.Run(() => _deviceCon.Download_User().OrderBy(x => x.RFID));
            var dUsers = deviceUsers.Select(x => new { x.DeviceID, x.RFID }).ToList();

            List<User> pcUsers;
            using (var db = new ModelContext())
            {
                pcUsers = db.Users.OrderBy(x => x.RFID).ToList();
            }

            var newUser = pcUsers.Where(pu => !dUsers.Any(du =>  du.RFID.ToInt(0) == pu.RFID.ToInt(0) && du.DeviceID == pu.DeviceID)).ToList();

            if (newUser.Count > 0)
            {
                PCNewUserDG.ItemsSource = newUser;
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
            var deleted = await Task.Run(() => _deviceCon.ClearAllUsers());

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

            var DeviceUsers = await Task.Run(() => _deviceCon.Download_User().OrderBy(x => x.DeviceID));

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
            var deleted = await Task.Run(() => _deviceCon.ClearAll_FPs());

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
            var deleted = await Task.Run(() => _deviceCon.ClearAll_Logs());

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
            var logs = await Task.Run(() => _deviceCon.DownloadLogs());

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
            var d_Reset = await Task.Run(() => _deviceCon.Duplicate_Punch_Time_Reset());

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
                _deviceId = Info.DeviceID.ToString();
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
                _deviceId = "";
                Userpanel.Visibility = Visibility.Collapsed;
                UserInfoError.Text = "User Not Found!";
            }
        }

        private void FingerButton_Click(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(_deviceId))
            {
                Button button = sender as Button;
                _deviceCon.FP_Msg = MessageTB;
                var index = Convert.ToInt32(button.CommandParameter);

                if (button.Background != Brushes.GreenYellow)
                {
                    _deviceCon.FP_Add(_deviceId, index);
                }
                else
                {
                    MessageBoxResult messageBoxResult = MessageBox.Show("Delete Fingerprint?", "Finger Delete", MessageBoxButton.YesNo);
                    if (messageBoxResult == MessageBoxResult.Yes)
                    {
                        var ErrorCode = _deviceCon.FP_Delete(_deviceId, index);
                        if (ErrorCode == 0)
                        {
                            LocalData.Instance.Delete_UserFP(Convert.ToInt32(_deviceId), index);
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
