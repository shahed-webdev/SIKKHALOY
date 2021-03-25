using AttendanceDevice.Config_Class;
using AttendanceDevice.Model;
using AttendanceDevice.Settings;
using RestSharp;
using System;
using System.ComponentModel;
using System.Data.Entity;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Threading;

namespace AttendanceDevice
{
    /// <summary>
    /// Interaction logic for Display_Window.xaml
    /// </summary>

    public partial class DisplayWindow : Window
    {
        private DispatcherTimer _tmr = new DispatcherTimer();
        private readonly DeviceDisplay _deviceDisplay;

        public DisplayWindow(DeviceDisplay deviceDisplay)
        {
            _deviceDisplay = deviceDisplay;
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            this.DataContext = LocalData.Instance.institution;

            countDevice.Badge = _deviceDisplay.Total_Devices();

            foreach (var device in _deviceDisplay.Devices)
            {
                //Data Show context pass to the device class
                device.EnrollUserCard = UserDataGrid;
                device.EnrollUserDialogHost = EnrollUserDialog;
                device.LogViewLb = StudentImageListBox;
            }

            StudentImageListBox.ItemsSource = Machine.GetAttendance(AttType.All);


            //DoubleAnimation doubleAnimation = new DoubleAnimation();
            //doubleAnimation.From = -this.ActualWidth;
            //doubleAnimation.To = this.ActualWidth;
            //doubleAnimation.RepeatBehavior = RepeatBehavior.Forever;
            //doubleAnimation.AutoReverse = true;
            //doubleAnimation.Duration = new Duration(TimeSpan.FromSeconds((this.ActualWidth) * 0.010));

            //StudentImageListview.BeginAnimation(canvas, doubleAnimation);

            //var dt = this.Resources["myFirstItemTemplate"] as DataTemplate;
            //Storyboard sb = this.Resources["sb"] as Storyboard;
            //sb.Begin();


            //Timer-setup
            _tmr.Interval = new TimeSpan(0, 0, 10);
            _tmr.Tick += Timer_Tick;
            _tmr.Start();
            this.Closing += Window_Closing;
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            //Clean up.
            _tmr.Stop();
            _tmr = null;
        }

        private async void Timer_Tick(object sender, EventArgs e)
        {
            var totalDeviceConnected = _deviceDisplay.Total_Devices();

            if (totalDeviceConnected == 0)
            {
                LocalData.Current_Error.Message = "No Device Connected!";
                LocalData.Current_Error.Type = Error_Type.DeviceInfoPage;

                var setting = new Setting();
                setting.Show();
                this.Close();
                return;
            }

            using (var db = new ModelContext())
            {
                var ins = await db.Institutions.FirstOrDefaultAsync();
                var client = new RestClient(ApiUrl.EndPoint);
                var allDevices = db.Devices.Count();

                var currentDateTime = DateTime.Now;
                var currentDate = currentDateTime.ToShortDateString();
                DeviceError.Text = "";

                //Device Attendance Disabled
                if (ins != null && !ins.Is_Device_Attendance_Enable)
                {
                    DeviceError.Text = "Device Attendance Disabled";
                }

                //Holiday attendance disable
                else if (ins != null && ins.Is_Today_Holiday && !ins.Holiday_NotActive)
                {
                    DeviceError.Text = "Today is Holiday And attendance disable";
                }

                //All Device not Connected 
                else if (allDevices != totalDeviceConnected)
                {
                    DeviceError.Text = "All device are not connected";
                    countDevice.Badge = totalDeviceConnected;
                }
                else
                {
                    var schScheduleIDs = LocalData.Instance.GetCurrent_Onday_SchduleIDs();
                    if (schScheduleIDs.Any())
                    {
                        LocalData.Instance.Abs_Insert(schScheduleIDs, currentDate, ins);
                    }
                }

                if (ins != null && !ins.Is_Employee_Attendance_Enable)
                {
                    DeviceError.Text = "Employee Attendance Disabled";
                }

                if (ins != null && !ins.Is_Student_Attendance_Enable)
                {
                    DeviceError.Text = "Student Attendance Disabled";
                }


                //check internet
                var internet = await ApiUrl.IsNoNetConnection();
                if (internet) return;

                #region Student Post
                var studentLog = await LocalData.Instance.StudentLog_Post();

                if (studentLog.Count > 0)
                {
                    var request = new RestRequest("api/Attendance/{id}/Students", Method.POST);

                    request.AddUrlSegment("id", ins.SchoolID);
                    request.AddHeader("Authorization", "Bearer " + ins.Token);
                    request.AddJsonBody(studentLog);

                    var response = await client.ExecutePostTaskAsync(request);

                    if (response.StatusCode == HttpStatusCode.OK)
                    {
                        foreach (var log in studentLog)
                        {
                            log.Is_Sent = true;
                            log.Is_Updated = true;
                            db.Entry(log).State = EntityState.Modified;
                            await db.SaveChangesAsync();
                        }
                    }

                    // MessageBox.Show("Student post: " + response.StatusCode);
                }

                #endregion Student Post

                #region Student Update
                var studentLogUpdate = await LocalData.Instance.StudentLog_Put();

                if (studentLogUpdate.Count > 0)
                {
                    var request = new RestRequest("api/Attendance/{id}/StudentsUpdate", Method.POST);

                    request.AddUrlSegment("id", ins.SchoolID);
                    request.AddHeader("Authorization", "Bearer " + ins.Token);
                    request.AddJsonBody(studentLogUpdate);

                    var response = await client.ExecutePostTaskAsync(request);

                    if (response.StatusCode == HttpStatusCode.OK)
                    {
                        foreach (var log in studentLogUpdate)
                        {
                            log.Is_Updated = true;
                            db.Entry(log).State = EntityState.Modified;
                            await db.SaveChangesAsync();
                        }
                    }

                    //MessageBox.Show("Student put " + response.StatusCode.ToString());
                }
                #endregion Student Update

                #region Employee Post
                var empLog = await LocalData.Instance.EmpLog_Post();

                if (empLog.Count > 0)
                {
                    var request = new RestRequest("api/Attendance/{id}/Employees", Method.POST);

                    if (ins != null)
                    {
                        request.AddUrlSegment("id", ins.SchoolID);
                        request.AddHeader("Authorization", "Bearer " + ins.Token);
                    }

                    request.AddJsonBody(empLog);

                    var response = await client.ExecutePostTaskAsync(request);

                    if (response.StatusCode == HttpStatusCode.OK)
                    {
                        foreach (var log in empLog)
                        {
                            log.Is_Sent = true;
                            log.Is_Updated = true;
                            db.Entry(log).State = EntityState.Modified;
                            await db.SaveChangesAsync();
                        }
                    }
                }
                #endregion Employee Post

                #region Employees Update
                var empLogUpdate = await LocalData.Instance.EmpLog_Put();

                if (empLogUpdate.Count > 0)
                {
                    var request = new RestRequest("api/Attendance/{id}/EmployeesUpdate", Method.POST);

                    request.AddUrlSegment("id", ins.SchoolID);
                    request.AddHeader("Authorization", "Bearer " + ins.Token);
                    request.AddJsonBody(empLogUpdate);

                    var response = await client.ExecutePostTaskAsync(request);

                    if (response.StatusCode != HttpStatusCode.OK) return;

                    foreach (var log in empLogUpdate)
                    {
                        log.Is_Updated = true;
                        db.Entry(log).State = EntityState.Modified;
                        await db.SaveChangesAsync();
                    }
                }
                #endregion Employee Put
            }
        }

        //Re-connect device
        private async void BtnReConnect_Device_Click(object sender, RoutedEventArgs e)
        {
            btnReconnect.IsEnabled = false;
            btnSetting.IsEnabled = false;

            var deviceList = _deviceDisplay.Devices;
            deviceList.Clear();

            var devices = LocalData.Instance.Devices;
            var ins = LocalData.Instance.institution;

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
                    var dCheck = false;
                    foreach (var item in deviceList)
                    {
                        var status = await Task.Run(() => item.ConnectDevice());
                        if (status.IsSuccess)
                        {
                            dCheck = true;
                            var prevLog = item.Download_Prev_Logs();
                            var todayLog = item.Download_Today_Logs();

                            await Machine.Save_logData(prevLog, todayLog, ins, item.Device);
                        }
                    }


                    if (dCheck)
                    {
                        var dDisplay = new DeviceDisplay(deviceList);

                        var displayWindow = new DisplayWindow(dDisplay);
                        displayWindow.Show();
                        this.Close();
                    }
                    else
                    {
                        var errorObj = new Error("Connect Device", "Device Not connected");
                        var errorWindow = new Error_Window(errorObj);
                        errorWindow.Show();
                    }
                }
                else
                {
                    var errorObj = new Error("Connect Device", "Device Not connected");
                    var errorWindow = new Error_Window(errorObj);
                    errorWindow.Show();
                }
            }
            else
            {
                var errorObj = new Error("Add Device", "Add Device Info");
                var errorWindow = new Error_Window(errorObj);
                errorWindow.Show();
            }

            btnReconnect.IsEnabled = true;
            btnSetting.IsEnabled = true;
        }

        //Setting Dialog
        private void Setting_Button_Click(object sender, RoutedEventArgs e)
        {
            SettingLoginDialog.IsOpen = true;

            //LocalData.Current_Error = new Setting_Error();
            //var settings = new Setting();
            //settings.Show();
            //Close();
        }
        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            SettingLoginDialog.IsOpen = false;
            Error.Text = "";
        }
        private void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            if (SettingPasswordBox.Password.Trim() == "") return;

            using (var db = new ModelContext())
            {
                var institution = db.Institutions.FirstOrDefault();

                if (institution == null) return;

                if (institution.SettingKey == SettingPasswordBox.Password)
                {
                    Error.Text = "Password is incorrect!";
                    SettingPasswordBox.Password = "";
                    return;
                }

                var settings = new Setting();
                SettingLoginDialog.IsOpen = false;
                settings.Show();
                Close();
            }
        }

        //external page link
        private void Sikkhaloy_Click(object sender, RoutedEventArgs e)
        {
            Process.Start("http://sikkhaloy.com/");
        }
        private void LoopsIT_Click(object sender, RoutedEventArgs e)
        {
            Process.Start("http://loopsit.com/");
        }
    }
}
