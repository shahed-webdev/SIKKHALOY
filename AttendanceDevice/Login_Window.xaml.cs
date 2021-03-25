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
            MessageSnackBar.MessageQueue = queue;

            try
            {   //Empty Error
                LocalData.Current_Error = new Setting_Error();

                LoadingPb.IsIndeterminate = true;
                LoginButton.IsEnabled = false;

                //Device List
                var deviceList = new List<DeviceConnection>();

                //Check Internet connection
                #region Check Internet
                if (await ApiUrl.IsNoNetConnection())
                {
                    using (var db = new ModelContext())
                    {
                        var user = db.Users.FirstOrDefault();

                        if (user != null)
                        {
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
                                    foreach (var device in deviceList)
                                    {
                                        var status = await Task.Run(() => device.ConnectDevice());
                                        if (status.IsSuccess)
                                        {
                                            dCheck = true;
                                            //Set server time to device
                                            await Task.Run(() => device.SetDateTime());
                                        }
                                    }

                                    if (dCheck)
                                    {
                                        var dDisplay = new DeviceDisplay(deviceList);
                                        var display = new Offline_DisplayWindow(dDisplay);
                                        display.Show();
                                        this.Close();

                                        LoadingPb.IsIndeterminate = false;
                                        LoginButton.IsEnabled = true;
                                        return;
                                    }
                                    else
                                    {
                                        queue.Enqueue("No internet connection & No Device Connected.");
                                        LoadingPb.IsIndeterminate = false;
                                        LoginButton.IsEnabled = true;
                                    }
                                }
                                else
                                {
                                    queue.Enqueue("No internet connection & Device IP not found.");
                                    LoadingPb.IsIndeterminate = false;
                                    LoginButton.IsEnabled = true;
                                }
                            }
                            else
                            {
                                queue.Enqueue("No internet connection & No device info.");
                                LoadingPb.IsIndeterminate = false;
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

                if (UserNameTextBox.Text != "" && PasswordPasswordBox.Password != "")
                {
                    var client = new RestClient(ApiUrl.EndPoint);
                    var loginRequest = new RestRequest("token", Method.POST);

                    var loginUsers = new LoginUser(UserNameTextBox.Text.Trim(), PasswordPasswordBox.Password);

                    loginRequest.AddObject(loginUsers);

                    //Login execute the request
                    var loginResponse = await client.ExecuteTaskAsync<Token>(loginRequest);

                    //API server Check
                    #region server Check
                    if (loginResponse.Data == null)
                    {
                        using (var db = new ModelContext())
                        {
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
                                        if (status.IsSuccess)
                                        {
                                            dCheck = true;
                                        }
                                    }

                                    if (dCheck)
                                    {
                                        var dDisplay = new DeviceDisplay(deviceList);
                                        var display = new Offline_DisplayWindow(dDisplay);

                                        display.Show();
                                        this.Close();

                                        LoadingPb.IsIndeterminate = false;
                                        LoginButton.IsEnabled = true;
                                    }
                                    else
                                    {
                                        queue.Enqueue(loginResponse.ErrorMessage + ". No Device Connected.");
                                        LoadingPb.IsIndeterminate = false;
                                        LoginButton.IsEnabled = true;
                                    }
                                }
                                else
                                {
                                    queue.Enqueue(loginResponse.ErrorMessage + ". Device IP not found.");
                                    LoadingPb.IsIndeterminate = false;
                                    LoginButton.IsEnabled = true;
                                }
                            }
                            else
                            {
                                queue.Enqueue(loginResponse.ErrorMessage + ". No device info.");
                                LoadingPb.IsIndeterminate = false;
                                LoginButton.IsEnabled = true;
                            }
                        }

                        return;
                    }
                    #endregion server Check

                    if (loginResponse.StatusCode == HttpStatusCode.OK)
                    {
                        var token = loginResponse.Data.access_token;
                        var schoolRequest = new RestRequest("api/school/{id}", Method.GET);

                        schoolRequest.AddUrlSegment("id", UserNameTextBox.Text.Trim());
                        schoolRequest.AddHeader("Authorization", "Bearer " + token);

                        //School info execute the request
                        var schoolResponse = await client.ExecuteTaskAsync(schoolRequest);
                        var schoolInfo = JsonConvert.DeserializeObject<Institution>(schoolResponse.Content);

                        if (schoolResponse.StatusCode == HttpStatusCode.OK && schoolInfo != null)
                        {
                            //set api date in local pc
                            var serverDatetime = schoolInfo.Current_Datetime;

                            using (var db = new ModelContext())
                            {
                                var ins = LocalData.Instance.institution;

                                if (ins == null)
                                {
                                    ins = schoolInfo;
                                    ins.Token = token;
                                    ins.Password = PasswordPasswordBox.Password;

                                    ins.PingTimeOut = 100; // default Value for device ping 
                                    db.Institutions.Add(ins);
                                    db.Entry(ins).State = EntityState.Added;
                                }
                                else
                                {
                                    ins.Token = token;
                                    ins.Password = PasswordPasswordBox.Password;
                                    ins.IsValid = schoolInfo.IsValid;
                                    ins.SettingKey = schoolInfo.SettingKey;
                                    ins.Is_Device_Attendance_Enable = schoolInfo.Is_Device_Attendance_Enable;
                                    ins.Is_Employee_Attendance_Enable = schoolInfo.Is_Employee_Attendance_Enable;
                                    ins.Is_Student_Attendance_Enable = schoolInfo.Is_Student_Attendance_Enable;
                                    ins.Is_Today_Holiday = schoolInfo.Is_Today_Holiday;
                                    ins.Holiday_NotActive = schoolInfo.Holiday_NotActive;
                                    ins.LastUpdateDate = schoolInfo.LastUpdateDate;

                                    db.Entry(ins).State = EntityState.Modified;
                                }


                                //Leave request
                                #region Leave data
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
                                        // Insert attendance records if new record
                                        if (!db.attendance_Records.Any(a => a.AttendanceDate == item.LeaveDate && a.DeviceID == item.DeviceID))
                                        {
                                            var attRecord = new Attendance_Record
                                            {
                                                AttendanceDate = item.LeaveDate,
                                                DeviceID = item.DeviceID,
                                                AttendanceStatus = "Leave"
                                            };

                                            db.attendance_Records.Add(attRecord);
                                        }
                                        db.user_Leave_Records.Add(item);
                                    }
                                }
                                #endregion Leave data

                                //Schedule Day Request
                                #region Schedule data
                                var scheduleDayRequest = new RestRequest("api/Users/{id}/schedule", Method.GET);
                                scheduleDayRequest.AddUrlSegment("id", ins.SchoolID);
                                scheduleDayRequest.AddHeader("Authorization", "Bearer " + token);

                                var scheduleDayResponse = await client.ExecuteTaskAsync<List<Attendance_Schedule_Day>>(scheduleDayRequest);

                                if (scheduleDayResponse.StatusCode == HttpStatusCode.OK && scheduleDayResponse.Data != null)
                                {
                                    //if same date Absent count remain same 
                                    if (ins.LastUpdateDate == DateTime.Today.ToShortDateString())
                                    {
                                        var schedulesUpdate = db.attendance_Schedule_Days.Where(s => s.Is_Abs_Count).Select(s => s.ScheduleID).ToList();

                                        scheduleDayResponse.Data.Where(s => schedulesUpdate.Contains(s.ScheduleID)).ToList().ForEach(s => s.Is_Abs_Count = true);
                                    }

                                    LocalData.Instance.Schedules = scheduleDayResponse.Data;

                                    db.attendance_Schedule_Days.Clear();

                                    foreach (var item in scheduleDayResponse.Data)
                                    {
                                        db.attendance_Schedule_Days.Add(item);
                                    }
                                }
                                else
                                {
                                    queue.Enqueue(loginResponse.Data.error_description);
                                    LoadingPb.IsIndeterminate = false;
                                    LoginButton.IsEnabled = true;
                                }
                                #endregion Schedule data

                                await db.SaveChangesAsync();

                                if (serverDatetime.AddMinutes(1) > DateTime.Now && serverDatetime.AddMinutes(-1) < DateTime.Now)
                                {
                                    //Device Check
                                    #region Device Check
                                    var devices = db.Devices.ToList();

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
                                                LocalData.Current_Error.Message = "No device Connected!";
                                                LocalData.Current_Error.Type = Error_Type.DeviceInfoPage;

                                                var setting = new Setting();
                                                setting.Show();
                                                this.Close();
                                            }
                                        }
                                        else
                                        {
                                            LocalData.Current_Error.Message = "Device IP Not Found or Device Switch Off!";
                                            LocalData.Current_Error.Type = Error_Type.DeviceInfoPage;

                                            var setting = new Setting();
                                            setting.Show();
                                            this.Close();
                                        }
                                    }
                                    else
                                    {
                                        LocalData.Current_Error.Message = "No Device Added In PC!";
                                        LocalData.Current_Error.Type = Error_Type.DeviceInfoPage;

                                        var setting = new Setting();
                                        setting.Show();
                                        this.Close();
                                    }
                                    #endregion Device Check
                                }
                                else
                                {
                                    var errorObj = new Error("Invalid", "Invalid PC Date Time. \n Server Time: " + serverDatetime.ToString("d MMM yy (hh:mm tt)"));
                                    var errorWindow = new Error_Window(errorObj);
                                    errorWindow.Show();
                                    this.Close();
                                }
                            }
                        }
                        else
                        {
                            queue.Enqueue(schoolResponse.StatusDescription);
                            LoadingPb.IsIndeterminate = false;
                            LoginButton.IsEnabled = true;
                        }
                    }
                    else
                    {
                        queue.Enqueue(loginResponse.Data.error_description);
                        LoadingPb.IsIndeterminate = false;
                        LoginButton.IsEnabled = true;
                    }
                }
                else
                {
                    queue.Enqueue("Username and password is required");
                    LoadingPb.IsIndeterminate = false;
                    LoginButton.IsEnabled = true;
                }
            }
            catch (Exception ex)
            {
                queue.Enqueue(ex.Message + " Inner ex: " + ex.InnerException.Message);
                LoadingPb.IsIndeterminate = false;
                LoginButton.IsEnabled = true;
            }
        }

        private void Login_Window_OnLoaded(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(LocalData.Current_Error.Message)) return;

            MessageSnackBar.Message.Content = LocalData.Current_Error.Message;
            MessageSnackBar.IsActive = true;
        }
    }
}



