using AttendanceDevice.Config_Class;
using AttendanceDevice.Model;
using AttendanceDevice.Settings.Pages;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Animation;
using System.Windows.Navigation;

namespace AttendanceDevice.Settings
{
    /// <summary>
    /// Interaction logic for Setting.xaml
    /// </summary>
    public partial class Setting : Window
    {
        public Setting()
        {
            InitializeComponent();
        }
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            if (LocalData.Current_Error.Type == Error_Type.DeviceInfoPage)
            {
                var DeviceInfoPage = new DeviceInfoPage();
                FrameSetting.Content = DeviceInfoPage;
            }
            else if (LocalData.Current_Error.Type == Error_Type.UserInfoPage)
            {
                var User = new UserInfoPage();
                FrameSetting.Content = User;
            }
            else
            {
                var DeshboardPage = new Deshboard();
                FrameSetting.Content = DeshboardPage;
            }
        }
        //humburger menu
        private void ButtonManuClose_Click(object sender, RoutedEventArgs e)
        {
            ButtonManuClose.Visibility = Visibility.Collapsed;
            ButtonManuOpen.Visibility = Visibility.Visible;
        }
        private void ButtonManuOpen_Click(object sender, RoutedEventArgs e)
        {
            ButtonManuClose.Visibility = Visibility.Visible;
            ButtonManuOpen.Visibility = Visibility.Collapsed;
        }

        //web page link
        private void Sikkhaloy_Button_Click(object sender, RoutedEventArgs e)
        {
            Process.Start("http://sikkhaloy.com");
        }
        private void Loopsit_Button_Click(object sender, RoutedEventArgs e)
        {
            Process.Start("http://loopsit.com");
        }
        private void Sikkhaloy_Facebook_Click(object sender, RoutedEventArgs e)
        {
            Process.Start("https://www.facebook.com/Sikkhaloy24/");
        }

        //menu link
        private void FrameSetting_Navigating(object sender, NavigatingCancelEventArgs e)
        {
            var da = new DoubleAnimation
            {
                From = 0, To = 1, Duration = new Duration(TimeSpan.FromMilliseconds(900))
            };
            FrameSetting.BeginAnimation(OpacityProperty, da);
        }
        private async void DisplayButton_Click(object sender, RoutedEventArgs e)
        {
            PB.IsIndeterminate = true;
            btnDisplay.IsEnabled = false;

            using (var db = new ModelContext())
            {
                //User is Null
                var user = await db.Users.FirstOrDefaultAsync();
                var deviceList = new List<DeviceConnection>();
                if (user != null)
                {
                    var devices = await db.Devices.ToListAsync();
                    var ins = await db.Institutions.FirstOrDefaultAsync();

                    //Device Check
                    if (devices.Any())
                    {
                        foreach (var device in devices)
                        {
                            var checkIp = await Device_PingTest.PingHostAsync(device.DeviceIP);
                            if (checkIp)
                            {
                                deviceList.Add(new DeviceConnection(device));
                            }
                        }

                        if (deviceList.Count > 0)
                        {
                            bool D_Check = false;
                            foreach (var device in deviceList)
                            {
                                var status = await Task.Run(() => device.ConnectDevice());
                                if (status.IsSuccess)
                                {
                                    var deviceTime = device.GetDateTime();
                                    if (deviceTime.ToString("dd-MM-yyyy hh:mm tt") != DateTime.Now.ToString("dd-MM-yyyy hh:mm tt"))
                                    {
                                        //Set server time to device
                                        await Task.Run(() => device.SetDateTime());
                                    }

                                    D_Check = true;
                                    var prev_log = device.Download_Prev_Logs();
                                    var today_log = device.Download_Today_Logs();

                                    await Machine.Save_logData(prev_log, today_log, ins, device.Device);
                                }
                            }


                            if (D_Check)
                            {
                                PB.IsIndeterminate = false;
                                btnDisplay.IsEnabled = true;

                                var D_Display = new DeviceDisplay(deviceList);

                                var DisplayWindow = new DisplayWindow(D_Display);
                                DisplayWindow.Show();
                                this.Close();
                            }
                            else
                            {
                                btnDisplay.IsEnabled = true;
                                PB.IsIndeterminate = false;

                                var errorObj = new Error("Connect Device", "Device Not connected!");
                                var ErrorWindow = new Error_Window(errorObj);
                                ErrorWindow.Show();
                            }
                        }
                        else
                        {
                            btnDisplay.IsEnabled = true;
                            PB.IsIndeterminate = false;

                            var errorObj = new Error("Connect Device", "Device Not connected!");
                            var ErrorWindow = new Error_Window(errorObj);
                            ErrorWindow.Show();
                        }
                    }
                    else
                    {
                        btnDisplay.IsEnabled = true;
                        PB.IsIndeterminate = false;

                        LocalData.Current_Error.Message = "No Device Added In PC!";
                        LocalData.Current_Error.Type = Error_Type.DeviceInfoPage;

                        var setting = new Setting();
                        setting.Show();
                        this.Close();
                        return;
                    }
                }
                else
                {
                    btnDisplay.IsEnabled = true;
                    PB.IsIndeterminate = false;

                    LocalData.Current_Error.Message = "No User Found on PC!";
                    LocalData.Current_Error.Type = Error_Type.UserInfoPage;

                    var setting = new Setting();
                    setting.Show();
                    this.Close();
                    return;
                }
            }
        }
        private void UserButton_Click(object sender, RoutedEventArgs e)
        {
            var User = new UserInfoPage();
            FrameSetting.Content = User;
        }
        private void InstitutionInfo_Button_Click(object sender, RoutedEventArgs e)
        {
            var Institution = new Institution_InfoPage();
            FrameSetting.Content = Institution;
        }
        private void Device_Button_Click(object sender, RoutedEventArgs e)
        {
            var DeviceInfoPage = new DeviceInfoPage();
            FrameSetting.Content = DeviceInfoPage;
        }
        private void BtnSchedule_Click(object sender, RoutedEventArgs e)
        {
            var SchedulePage = new SchedulePage();
            FrameSetting.Content = SchedulePage;
        }
        private void AttendanceLog_Button_Click(object sender, RoutedEventArgs e)
        {
            var Log = new Attendance_LogPage();
            FrameSetting.Content = Log;
        }
        private void BackUpData_Button_Click(object sender, RoutedEventArgs e)
        {
            var BackUp = new BackUp_LogsPage();
            FrameSetting.Content = BackUp;
        }

        private void Dashboard_Button_Click(object sender, RoutedEventArgs e)
        {
            var Dashboard = new Deshboard();
            FrameSetting.Content = Dashboard;
        }

        private void FingerPrint_Button_Click(object sender, RoutedEventArgs e)
        {
            var FingerPrint = new FingerPrint_Page();
            FrameSetting.Content = FingerPrint;
        }

        private void LeaveUser_Button_Click(object sender, RoutedEventArgs e)
        {
            var LeaveUser = new LeaveUser_Page();
            FrameSetting.Content = LeaveUser;
        }
    }
}
