using AttendanceDevice.APIClass;
using AttendanceDevice.Config_Class;
using AttendanceDevice.Model;
using AttendanceDevice.Settings;
using MaterialDesignThemes.Wpf;
using Newtonsoft.Json;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Windows;

namespace AttendanceDevice
{
    /// <summary>
    /// Interaction logic for Login_Window.xaml
    /// </summary>
    public partial class Login_Window : Window
    {
        public Login_Window()
        {
            InitializeComponent();
        }
        private async void Login_Click(object sender, RoutedEventArgs e)
        {
            var queue = new SnackbarMessageQueue(TimeSpan.FromSeconds(5));
            UpdateSnackbar.MessageQueue = queue;

            try
            {   //Empty Error
                LocalData.Current_Error = new Setting_Error();

                LoadingPB.IsIndeterminate = true;
                LoginButton.IsEnabled = false;

                //Device List
                List<DeviceConnection> DeviceList = new List<DeviceConnection>();

                //Check Internet connection
                #region Check Internet
                if (await ApiUrl.CheckInterNet())
                {
                    using (var db = new ModelContext())
                    {
                        var user = db.Users.FirstOrDefault();

                        if (user != null)
                        {
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
                                    foreach (var device in DeviceList)
                                    {
                                        var status = await Task.Run(() => device.ConnectDevice());
                                        if (status.IsSuccess)
                                        {
                                            D_Check = true;
                                            //Set server time to device
                                            await Task.Run(() => device.SetDateTime());
                                        }
                                    }

                                    if (D_Check)
                                    {
                                        var D_Display = new DeviceDisplay(DeviceList);
                                        var Display = new Offline_DisplayWindow(D_Display);
                                        Display.Show();
                                        this.Close();

                                        LoadingPB.IsIndeterminate = false;
                                        LoginButton.IsEnabled = true;
                                        return;
                                    }
                                    else
                                    {
                                        queue.Enqueue("No internet connection & No Device Connected.");
                                        LoadingPB.IsIndeterminate = false;
                                        LoginButton.IsEnabled = true;
                                    }
                                }
                                else
                                {
                                    queue.Enqueue("No internet connection & Device IP not found.");
                                    LoadingPB.IsIndeterminate = false;
                                    LoginButton.IsEnabled = true;
                                }
                            }
                            else
                            {
                                queue.Enqueue("No internet connection & No device info.");
                                LoadingPB.IsIndeterminate = false;
                                LoginButton.IsEnabled = true;
                            }
                        }
                        else
                        {
                            var errorObj = new Error("No Internet", "No Internet connection!");
                            var errorWindow = new Error_Window(errorObj);
                            errorWindow.Show();
                            this.Close();
                            return;
                        }
                    }
                    return;
                }
                #endregion Check Internet

                if (!UserNameTextBox.Equals(string.Empty) && !PasswordPasswordBox.Password.Equals(string.Empty))
                {
                    var client = new RestClient(ApiUrl.EndPoint);
                    var LoginRequest = new RestRequest("token", Method.POST);

                    var LoginUsers = new LoginUser(UserNameTextBox.Text.Trim(), PasswordPasswordBox.Password);

                    LoginRequest.AddObject(LoginUsers);

                    //Login execute the request
                    IRestResponse<Token> LoginResponse = await client.ExecuteTaskAsync<Token>(LoginRequest);

                    //API server Check
                    #region server Check
                    if (LoginResponse.Data == null)
                    {
                        using (var db = new ModelContext())
                        {
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
                                        }
                                    }

                                    if (D_Check)
                                    {
                                        var D_Display = new DeviceDisplay(DeviceList);
                                        var Display = new Offline_DisplayWindow(D_Display);
                                        Display.Show();
                                        this.Close();

                                        LoadingPB.IsIndeterminate = false;
                                        LoginButton.IsEnabled = true;
                                    }
                                    else
                                    {
                                        queue.Enqueue(LoginResponse.ErrorMessage + ". No Device Connected.");
                                        LoadingPB.IsIndeterminate = false;
                                        LoginButton.IsEnabled = true;
                                    }
                                }
                                else
                                {
                                    queue.Enqueue(LoginResponse.ErrorMessage + ". Device IP not found.");
                                    LoadingPB.IsIndeterminate = false;
                                    LoginButton.IsEnabled = true;
                                }
                            }
                            else
                            {
                                queue.Enqueue(LoginResponse.ErrorMessage + ". No device info.");
                                LoadingPB.IsIndeterminate = false;
                                LoginButton.IsEnabled = true;
                            }
                        }

                        return;
                    }
                    #endregion server Check

                    if (LoginResponse.StatusCode == HttpStatusCode.OK)
                    {
                        var token = LoginResponse.Data.access_token;
                        var SchoolRequest = new RestRequest("api/school/{id}", Method.GET);

                        SchoolRequest.AddUrlSegment("id", UserNameTextBox.Text.Trim());
                        SchoolRequest.AddHeader("Authorization", "Bearer " + token);

                        //School info execute the request
                        var SchoolResponse = await client.ExecuteTaskAsync(SchoolRequest);
                        var schoolInfo = JsonConvert.DeserializeObject<Institution>(SchoolResponse.Content);
                        var Server_datetime = new DateTime();

                        if (SchoolResponse.StatusCode == HttpStatusCode.OK && schoolInfo != null)
                        {
                            using (ModelContext db = new ModelContext())
                            {
                                var Ins = LocalData.Instance.institution;

                                if (Ins == null)
                                {
                                    Ins = schoolInfo;
                                    Ins.Token = token;
                                    Ins.Password = PasswordPasswordBox.Password;

                                    db.Institutions.Add(Ins);
                                    db.Entry(Ins).State = EntityState.Added;
                                }
                                else
                                {
                                    Ins.Token = token;
                                    Ins.Password = PasswordPasswordBox.Password;
                                    Ins.IsValid = schoolInfo.IsValid;
                                    Ins.SettingKey = schoolInfo.SettingKey;
                                    Ins.Is_Device_Attendance_Enable = schoolInfo.Is_Device_Attendance_Enable;
                                    Ins.Is_Employee_Attendance_Enable = schoolInfo.Is_Employee_Attendance_Enable;
                                    Ins.Is_Student_Attendance_Enable = schoolInfo.Is_Student_Attendance_Enable;
                                    Ins.Is_Today_Holiday = schoolInfo.Is_Today_Holiday;
                                    Ins.Holiday_NotActive = schoolInfo.Holiday_NotActive;
                                    Ins.LastUpdateDate = schoolInfo.LastUpdateDate;
                                    Server_datetime = schoolInfo.Current_Datetime;

                                    db.Entry(Ins).State = EntityState.Modified;
                                }


                                //Leave request
                                #region Leave data
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
                                        // Insert attendance records if new record
                                        if (!db.attendance_Records.Any(a => a.AttendanceDate == item.LeaveDate && a.DeviceID == item.DeviceID))
                                        {
                                            var Att_record = new Attendance_Record();

                                            Att_record.AttendanceDate = item.LeaveDate;
                                            Att_record.DeviceID = item.DeviceID;
                                            Att_record.AttendanceStatus = "Leave";
                                            db.attendance_Records.Add(Att_record);
                                        }
                                        db.user_Leave_Records.Add(item);
                                    }
                                }
                                #endregion Leave data

                                //Schedule Day Request
                                #region Schedule data
                                var ScheduleDay_Request = new RestRequest("api/Users/{id}/schedule", Method.GET);
                                ScheduleDay_Request.AddUrlSegment("id", Ins.SchoolID);
                                ScheduleDay_Request.AddHeader("Authorization", "Bearer " + token);

                                IRestResponse<List<Attendance_Schedule_Day>> ScheduleDay_Response = await client.ExecuteTaskAsync<List<Attendance_Schedule_Day>>(ScheduleDay_Request);

                                if (ScheduleDay_Response.StatusCode == HttpStatusCode.OK && ScheduleDay_Response.Data != null)
                                {
                                    //if same date Absent count remain same 
                                    if (Ins.LastUpdateDate == DateTime.Today.ToShortDateString())
                                    {
                                        var Schedules_Update = db.attendance_Schedule_Days.Where(s => s.Is_Abs_Count).Select(s => s.ScheduleID).ToList();

                                        ScheduleDay_Response.Data.Where(s => Schedules_Update.Contains(s.ScheduleID)).ToList().ForEach(s => s.Is_Abs_Count = true);
                                    }

                                    LocalData.Instance.Schedules = ScheduleDay_Response.Data;

                                    db.attendance_Schedule_Days.Clear();

                                    foreach (var item in ScheduleDay_Response.Data)
                                    {
                                        db.attendance_Schedule_Days.Add(item);
                                    }
                                }
                                else
                                {
                                    queue.Enqueue(LoginResponse.Data.error_description);
                                    LoadingPB.IsIndeterminate = false;
                                    LoginButton.IsEnabled = true;
                                }
                                #endregion Schedule data

                                await db.SaveChangesAsync();

                                if (Server_datetime.AddMinutes(1) > DateTime.Now && Server_datetime.AddMinutes(-1) < DateTime.Now)
                                {
                                    //Device Check
                                    #region Device Check
                                    var Devices = db.Devices.ToList();

                                    if (Devices.Count() > 0)
                                    {
                                        foreach (var Device in Devices)
                                        {
                                            var checkIP = await Device_PingTest.PingHostAsync(Device.DeviceIP);
                                            if (checkIP)
                                            {
                                                DeviceList.Add(new DeviceConnection(Device));
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
                                                return;
                                            }
                                            else
                                            {
                                                LocalData.Current_Error.Message = "No device Connected!";
                                                LocalData.Current_Error.Type = Error_Type.DeviceInfoPage;

                                                var setting = new Setting();
                                                setting.Show();
                                                this.Close();
                                                return;
                                            }
                                        }
                                        else
                                        {
                                            LocalData.Current_Error.Message = "Device IP Not Found or Device Switch Off!";
                                            LocalData.Current_Error.Type = Error_Type.DeviceInfoPage;

                                            var setting = new Setting();
                                            setting.Show();
                                            this.Close();
                                            return;
                                        }
                                    }
                                    else
                                    {
                                        LocalData.Current_Error.Message = "No Device Added In PC!";
                                        LocalData.Current_Error.Type = Error_Type.DeviceInfoPage;

                                        var setting = new Setting();
                                        setting.Show();
                                        this.Close();
                                        return;
                                    }
                                    #endregion Device Check
                                }
                                else
                                {
                                    var errorObj = new Error("Invalid", "Invalid PC Date Time. \n Server Time: " + Server_datetime.ToString("d MMM yy (hh:mm tt)"));
                                    var errorWindow = new Error_Window(errorObj);
                                    errorWindow.Show();
                                    this.Close();
                                    return;
                                }
                            }
                        }
                        else
                        {
                            queue.Enqueue(SchoolResponse.StatusDescription);
                            LoadingPB.IsIndeterminate = false;
                            LoginButton.IsEnabled = true;
                        }
                    }
                    else
                    {
                        queue.Enqueue(LoginResponse.Data.error_description);
                        LoadingPB.IsIndeterminate = false;
                        LoginButton.IsEnabled = true;
                    }
                }
                else
                {
                    queue.Enqueue("Username and password is required");
                    LoadingPB.IsIndeterminate = false;
                    LoginButton.IsEnabled = true;
                }
            }
            catch (Exception ex)
            {
                queue.Enqueue(ex.Message);
            }
        }
    }
}



