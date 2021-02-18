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
    public partial class Offline_DisplayWindow : Window
    {
        private DispatcherTimer _tmr = new DispatcherTimer();
        private DeviceDisplay _deviceDisplay;

        public Offline_DisplayWindow(DeviceDisplay deviceDisplay)
        {
            _deviceDisplay = deviceDisplay;
            InitializeComponent();
        }
        public void axCZKEM1_OnAttTransactionEx(string EnrollNumber, int IsInValid, int AttState, int VerifyMethod, int Year, int Month, int Day, int Hour, int Minute, int Second, int WorkCode)
        {
            var DeviceID = Convert.ToInt32(EnrollNumber);
            var dt = new DateTime(Year, Month, Day, Hour, Minute, Second);
            var time = new TimeSpan(Hour, Minute, Second);
            var userView = LocalData.Instance.GetUserView(DeviceID);

            if (userView == null)
            {
                userView = new UserView();
                userView.Name = "User Not found on PC";
                UserDataGrid.DataContext = userView;
                return;
            }

            userView.Enroll_Time = dt;
            var s_Date = dt.ToShortDateString();

            UserDataGrid.DataContext = userView;
        }
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            this.DataContext = LocalData.Instance.institution;
            countDevice.Badge = _deviceDisplay.Total_Devices();

            foreach (var device in _deviceDisplay.Devices)
            {
                if (device.axCZKEM1.RegEvent(Machine.Number, 1))
                {
                    device.axCZKEM1.OnAttTransactionEx += new zkemkeeper._IZKEMEvents_OnAttTransactionExEventHandler(axCZKEM1_OnAttTransactionEx);
                }
            }


            //Timer-setup
            _tmr.Interval = new TimeSpan(0, 2, 0);
            _tmr.Tick += new EventHandler(Timer_Tick);
            _tmr.Start();
            this.Closing += new CancelEventHandler(Window_Closing);
        }

        void Window_Closing(object sender, CancelEventArgs e)
        {
            //Clean up.
            _tmr.Stop();
            _tmr = null;
        }

        async void Timer_Tick(object sender, EventArgs e)
        {
            #region Check Internet
            if (!await ApiUrl.CheckInterNet())
            {
                using (var db = new ModelContext())
                {
                    var Ins = db.Institutions.FirstOrDefault();

                    if (Ins != null)
                    {
                        var client = new RestClient(ApiUrl.EndPoint);
                        var request = new RestRequest("token", Method.POST);

                        var LoginUsers = new LoginUser(Ins.UserName, Ins.Password);
                        request.AddObject(LoginUsers);

                        // Execute the request
                        IRestResponse<Token> LoginResponse = await client.ExecuteTaskAsync<Token>(request);

                        if (LoginResponse.StatusCode == HttpStatusCode.OK && LoginResponse.Data != null)
                        {
                            var token = LoginResponse.Data.access_token;

                            var SchoolRequest = new RestRequest("api/school/{id}/short", Method.GET);
                            SchoolRequest.AddUrlSegment("id", Ins.UserName);
                            SchoolRequest.AddHeader("Authorization", "Bearer " + token);

                            //School info execute the request
                            IRestResponse<Institution> SchoolResponse = await client.ExecuteTaskAsync<Institution>(SchoolRequest);

                            if (SchoolResponse.StatusCode == HttpStatusCode.OK && SchoolResponse.Data != null)
                            {
                                var schoolInfo = SchoolResponse.Data;

                                if (schoolInfo.IsValid)
                                {
                                    Ins.Token = token;
                                    Ins.IsValid = schoolInfo.IsValid;
                                    Ins.SettingKey = schoolInfo.SettingKey;
                                    Ins.Is_Device_Attendance_Enable = schoolInfo.Is_Device_Attendance_Enable;
                                    Ins.LastUpdateDate = schoolInfo.LastUpdateDate;
                                    Ins.Holiday_NotActive = schoolInfo.Holiday_NotActive;

                                    db.Entry(Ins).State = EntityState.Modified;

                                    //Leave request
                                    #region Leave request
                                    var LeaveRequest = new RestRequest("api/Users/{id}/leave", Method.GET);
                                    LeaveRequest.AddUrlSegment("id", Ins.SchoolID);
                                    LeaveRequest.AddHeader("Authorization", "Bearer " + token);

                                    //Leave execute the request
                                    IRestResponse<List<User_Leave_Record>> LeaveResponse = await client.ExecuteTaskAsync<List<User_Leave_Record>>(LeaveRequest);

                                    if (LeaveResponse.StatusCode == HttpStatusCode.OK && LeaveResponse.Data != null)
                                    {
                                        //For deleting all previous data
                                        db.user_Leave_Records.Clear();

                                        foreach (var item in LeaveResponse.Data)
                                        {
                                            db.user_Leave_Records.Add(item);
                                        }
                                    }
                                    #endregion Leave request

                                    //Schedule Day Request
                                    #region Schedule Day Request
                                    var ScheduleDay_Request = new RestRequest("api/Users/{id}/schedule", Method.GET);
                                    ScheduleDay_Request.AddUrlSegment("id", Ins.SchoolID);
                                    ScheduleDay_Request.AddHeader("Authorization", "Bearer " + token);

                                    IRestResponse<List<Attendance_Schedule_Day>> ScheduleDay_Response = await client.ExecuteTaskAsync<List<Attendance_Schedule_Day>>(ScheduleDay_Request);

                                    if (ScheduleDay_Response.StatusCode == HttpStatusCode.OK && ScheduleDay_Response.Data != null)
                                    {
                                        db.attendance_Schedule_Days.Clear();

                                        var DayData = ScheduleDay_Response.Data;

                                        foreach (var item in DayData)
                                        {
                                            db.attendance_Schedule_Days.Add(item);
                                        }
                                    }
                                    #endregion Schedule Day Request

                                    await db.SaveChangesAsync();

                                    //Device List
                                    #region Device List
                                    List<DeviceConnection> DeviceList = new List<DeviceConnection>();
                                    var Devices = await db.Devices.ToListAsync();

                                    if (Devices.Count() > 0)
                                    {
                                        foreach (var device in Devices)
                                        {
                                            var checkIP = await Device_PingTest.PingHostAsync(device.DeviceIP);
                                            if (checkIP)
                                            {
                                                DeviceList.Add(new DeviceConnection(device));
                                            }
                                        }

                                        if (DeviceList.Count > 0)
                                        {
                                            bool D_Check = false;
                                            foreach (var item in DeviceList)
                                            {
                                                var status = await Task.Run(() => item.ConnectDevice());
                                                if (status.IsSuccess)
                                                {
                                                    D_Check = true;

                                                    var prev_log = item.Download_Prev_Logs();
                                                    var today_log = item.Download_Today_Logs();

                                                    await Machine.Save_logData(prev_log, today_log, Ins, item.Device);
                                                }
                                            }

                                            if (D_Check)
                                            {
                                                var D_Display = new DeviceDisplay(DeviceList);
                                                var Display = new DisplayWindow(D_Display);
                                                Display.Show();
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
                                    return;
                                }
                            }
                            else
                            {
                                var errorObj = new Error("Api Error", "Institution Info Not Found!");
                                var errorWindow = new Error_Window(errorObj);
                                errorWindow.Show();
                                this.Close();
                                return;
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
                        var Login = new Login_Window();
                        Login.Show();
                        this.Close();
                    }
                }

            }
            #endregion Check Internet
        }

        //Dialog Open
        private void Setting_Button_Click(object sender, RoutedEventArgs e)
        {
            //DH.IsOpen = true;
            var Settings = new Setting();
            Settings.Show();
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
                var Institution = db.Institutions.FirstOrDefault();

                if (Institution != null)
                {
                    if (Institution.SettingKey == SettingPasswordBox.Password)
                    {
                        var Settings = new Setting();

                        DH.IsOpen = false;
                        Settings.Show();
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
}
