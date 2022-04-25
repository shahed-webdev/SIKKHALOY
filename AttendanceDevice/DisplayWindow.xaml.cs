using AttendanceDevice.Config_Class;
using AttendanceDevice.Model;
using AttendanceDevice.Settings;
using RestSharp;
using System;
using System.ComponentModel;
using System.Data.Entity;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Windows;
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
            DataContext = LocalData.Instance.institution;
            var schoolId = LocalData.Instance.institution.SchoolID;

            var url =$"http://localhost:3326/Attendances/Online_Display/DeviceDisplay.aspx?schoolId={schoolId}";
            webView.Source = new Uri(url);

            countDevice.Badge = _deviceDisplay.Total_Devices();
            foreach (var device in _deviceDisplay.Devices)
            {
                //Data Show context pass to the device class
                device.EnrollUserCard = UserDataGrid;
            }

            //Timer-setup
            _tmr.Interval = new TimeSpan(0, 0, 10);
            _tmr.Tick += Timer_Tick;
            _tmr.Start();
            Closing += Window_Closing;
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
                var ins = LocalData.Instance.institution;
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
                    //get only Late time exceed and Abs not Count schedules
                    var schScheduleIDs = LocalData.Instance.GetCurrentOndaySchduleIds();
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

                        //reload webview after send data success
                        webView.Reload();
                    }
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

                        //reload webview after send data success
                        webView.Reload();
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

                        //reload webview after send data success
                        webView.Reload();
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

                    //reload webview after send data success
                    webView.Reload();
                }
                #endregion Employee Put

                #region SMS_Send

                var smsRequest = new RestRequest("api/Attendance/{id}/SendSms", Method.POST);

                smsRequest.AddUrlSegment("id", ins.SchoolID);
                smsRequest.AddHeader("Authorization", "Bearer " + ins.Token);

                var responseSms = await client.ExecutePostTaskAsync(smsRequest);

                #endregion
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
                            var prevLog = item.DownloadPrevLogs();
                            var todayLog = item.DownloadTodayLogs();

                            await Machine.SaveLogsOrAttendanceInPc(prevLog, todayLog, ins, item.Device);
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

                if (institution.SettingKey != SettingPasswordBox.Password)
                {
                    Error.Text = "Setting key is incorrect!";
                    SettingPasswordBox.Password = "";
                    return;
                }

                var settings = new Setting();
                SettingLoginDialog.IsOpen = false;
                settings.Show();
                this.Close();
            }
        }

        //external page link
        private void Sikkhaloy_Click(object sender, RoutedEventArgs e)
        {
            Process.Start("http://sikkhaloy.com/Attendances/Online_Display/Attendance_Slider.aspx");
        }
        private void LoopsIT_Click(object sender, RoutedEventArgs e)
        {
            Process.Start("http://loopsit.com/");
        }
    }
}
