using AttendanceDevice.APIClass;
using AttendanceDevice.Config_Class;
using AttendanceDevice.Model;
using AttendanceDevice.Settings;
using RestSharp;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

namespace AttendanceDevice
{
    /// <summary>
    /// Interaction logic for Offline_DisplayWindow.xaml
    /// </summary>
    public partial class Offline_DisplayWindow
    {
        private DispatcherTimer _tmr = new DispatcherTimer();
        private readonly DeviceDisplay _deviceDisplay;

        public Offline_DisplayWindow(DeviceDisplay deviceDisplay)
        {
            _deviceDisplay = deviceDisplay;
            InitializeComponent();
        }
        public void axCZKEM1_OnAttTransactionEx(string EnrollNumber, int IsInValid, int AttState, int VerifyMethod, int Year, int Month, int Day, int Hour, int Minute, int Second, int WorkCode)
        {
            var deviceId = Convert.ToInt32(EnrollNumber);
            var dt = new DateTime(Year, Month, Day, Hour, Minute, Second);
            var time = new TimeSpan(Hour, Minute, Second);
            var userView = LocalData.Instance.GetUserView(deviceId);

            if (userView == null)
            {
                userView = new UserView {Name = "User Not found on PC"};
                UserDataGrid.DataContext = userView;
                return;
            }

            userView.Enroll_Time = dt;
            var sDate = dt.ToShortDateString();

            UserDataGrid.DataContext = userView;
        }
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            this.DataContext = LocalData.Instance.institution;
            countDevice.Badge = _deviceDisplay.Total_Devices();

            foreach (var device in _deviceDisplay.Devices.Where(device => device.axCZKEM1.RegEvent(Machine.Number, 1)))
            {
                device.axCZKEM1.OnAttTransactionEx += axCZKEM1_OnAttTransactionEx;
                device.EnrollUser_Card = UserDataGrid;
                device.LogViewLB = StudentImageListview;
            }

            StudentImageListview.ItemsSource = Machine.GetAttendance(AttType.All);

            //Timer-setup
            _tmr.Interval = new TimeSpan(0, 2, 0);
            _tmr.Tick += Timer_Tick;
            _tmr.Start();
            this.Closing += Window_Closing;
        }

        public void Window_Closing(object sender, CancelEventArgs e)
        {
            //Clean up.
            _tmr.Stop();
            _tmr = null;
        }

        private async void Timer_Tick(object sender, EventArgs e)
        {
            #region Check Internet

            if (await ApiUrl.CheckInterNet()) return;

            using (var db = new ModelContext())
            {
                var ins = db.Institutions.FirstOrDefault();

                if (ins != null)
                {
                    var client = new RestClient(ApiUrl.EndPoint);
                    var request = new RestRequest("token", Method.POST);

                    var loginUsers = new LoginUser(ins.UserName, ins.Password);
                    request.AddObject(loginUsers);

                    // Execute the request
                    var loginResponse = await client.ExecuteTaskAsync<Token>(request);

                    if (loginResponse.StatusCode == HttpStatusCode.OK && loginResponse.Data != null)
                    {
                        var token = loginResponse.Data.access_token;

                        var schoolRequest = new RestRequest("api/school/{id}/short", Method.GET);
                        schoolRequest.AddUrlSegment("id", ins.UserName);
                        schoolRequest.AddHeader("Authorization", "Bearer " + token);

                        //School info execute the request
                        var schoolResponse = await client.ExecuteTaskAsync<Institution>(schoolRequest);

                        if (schoolResponse.StatusCode == HttpStatusCode.OK && schoolResponse.Data != null)
                        {
                            var schoolInfo = schoolResponse.Data;

                            if (schoolInfo.IsValid)
                            {
                                ins.Token = token;
                                ins.IsValid = schoolInfo.IsValid;
                                ins.SettingKey = schoolInfo.SettingKey;
                                ins.Is_Device_Attendance_Enable = schoolInfo.Is_Device_Attendance_Enable;
                                ins.LastUpdateDate = schoolInfo.LastUpdateDate;
                                ins.Holiday_NotActive = schoolInfo.Holiday_NotActive;

                                db.Entry(ins).State = EntityState.Modified;

                                //Leave request
                                #region Leave request
                                var leaveRequest = new RestRequest("api/Users/{id}/leave", Method.GET);
                                leaveRequest.AddUrlSegment("id", ins.SchoolID);
                                leaveRequest.AddHeader("Authorization", "Bearer " + token);

                                //Leave execute the request
                                var leaveResponse = await client.ExecuteTaskAsync<List<User_Leave_Record>>(leaveRequest);

                                if (leaveResponse.StatusCode == HttpStatusCode.OK && leaveResponse.Data != null)
                                {
                                    //For deleting all previous data
                                    db.user_Leave_Records.Clear();

                                    foreach (var item in leaveResponse.Data)
                                    {
                                        db.user_Leave_Records.Add(item);
                                    }
                                }
                                #endregion Leave request

                                //Schedule Day Request
                                #region Schedule Day Request
                                var scheduleDayRequest = new RestRequest("api/Users/{id}/schedule", Method.GET);
                                scheduleDayRequest.AddUrlSegment("id", ins.SchoolID);
                                scheduleDayRequest.AddHeader("Authorization", "Bearer " + token);

                                var scheduleDayResponse = await client.ExecuteTaskAsync<List<Attendance_Schedule_Day>>(scheduleDayRequest);

                                if (scheduleDayResponse.StatusCode == HttpStatusCode.OK && scheduleDayResponse.Data != null)
                                {
                                    db.attendance_Schedule_Days.Clear();

                                    var dayData = scheduleDayResponse.Data;

                                    foreach (var item in dayData)
                                    {
                                        db.attendance_Schedule_Days.Add(item);
                                    }
                                }
                                #endregion Schedule Day Request

                                await db.SaveChangesAsync();

                                //Device List
                                #region Device List
                                var deviceList = new List<DeviceConnection>();
                                var devices = await db.Devices.ToListAsync();

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
                                            if (!status.IsSuccess) continue;

                                            dCheck = true;

                                            var prevLog = item.Download_Prev_Logs();
                                            var todayLog = item.Download_Today_Logs();

                                            await Machine.Save_logData(prevLog, todayLog, ins, item.Device);
                                        }

                                        if (dCheck)
                                        {
                                            var dDisplay = new DeviceDisplay(deviceList);
                                            var display = new DisplayWindow(dDisplay);
                                            display.Show();
                                            this.Close();
                                        }
                                        else
                                        {
                                            var setting = new Setting();
                                            setting.Show();
                                            this.Close();
                                        }
                                    }
                                    else
                                    {
                                        var setting = new Setting();
                                        setting.Show();
                                        this.Close();
                                    }
                                }
                                else
                                {
                                    var setting = new Setting();
                                    setting.Show();
                                    this.Close();
                                }
                                #endregion Device List
                            }
                            else
                            {
                                var errorObj = new Error("Invalid", "Invalid Institution!");
                                var errorWindow = new Error_Window(errorObj);
                                errorWindow.Show();
                                this.Close();
                            }
                        }
                        else
                        {
                            var errorObj = new Error("Api Error", "Institution Info Not Found!");
                            var errorWindow = new Error_Window(errorObj);
                            errorWindow.Show();
                            this.Close();
                        }
                    }
                    else
                    {
                        var login = new Login_Window();
                        login.Show();
                        this.Close();
                    }
                }
                else
                {
                    var login = new Login_Window();
                    login.Show();
                    this.Close();
                }
            }
            #endregion Check Internet
        }

        //Dialog Open
        private void Setting_Button_Click(object sender, RoutedEventArgs e)
        {
            //DH.IsOpen = true;
            var settings = new Setting();
            settings.Show();
            Close();
        }

        //dialog close
        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DH.IsOpen = false;
            Error.Text = "";
        }

        private void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            if (SettingPasswordBox.Password == "") return;

            using (var db = new ModelContext())
            {
                var institution = db.Institutions.FirstOrDefault();

                if (institution == null) return;

                if (institution.SettingKey == SettingPasswordBox.Password)
                {
                    var settings = new Setting();

                    DH.IsOpen = false;
                    settings.Show();
                    Close();
                }
                else
                {
                    Error.Text = "Password is incorrect!";
                    SettingPasswordBox.Password = "";
                }
            }
        }
    }
}
