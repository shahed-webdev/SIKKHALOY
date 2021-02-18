using AttendanceDevice.APIClass;
using AttendanceDevice.Config_Class;
using AttendanceDevice.Model;
using AttendanceDevice.Settings;
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
    public partial class Window_Startup : Window
    {
        public Window_Startup()
        {
            InitializeComponent();
        }

        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {   //Empty Error
                LocalData.Current_Error = new Setting_Error();

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
                                    }
                                    else
                                    {
                                        var errorObj = new Error("No Internet", "No Internet connection & No Device Connected!");
                                        var errorWindow = new Error_Window(errorObj);
                                        errorWindow.Show();
                                        this.Close();
                                    }
                                }
                                else
                                {
                                    var errorObj = new Error("No Internet", "No Internet connection & Device IP not found!");
                                    var errorWindow = new Error_Window(errorObj);
                                    errorWindow.Show();
                                    this.Close();
                                }
                            }
                            else
                            {
                                var errorObj = new Error("No Internet", "No Internet connection & No device info!");
                                var errorWindow = new Error_Window(errorObj);
                                errorWindow.Show();
                                this.Close();
                            }
                        }
                        else
                        {
                            var errorObj = new Error("No Internet", "No Internet connection!");
                            var errorWindow = new Error_Window(errorObj);
                            errorWindow.Show();
                            this.Close();
                        }
                    }
                    return;
                }
                #endregion Check Internet

                using (var db = new ModelContext())
                {
                    var Ins = LocalData.Instance.institution;

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

                            var Server_datetime = new DateTime();
                            if (SchoolResponse.StatusCode == HttpStatusCode.OK && SchoolResponse.Data != null)
                            {
                                var schoolInfo = SchoolResponse.Data;

                                if (schoolInfo.IsValid)
                                {
                                    Ins.Token = token;
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

                                    //Leave request and Insert into attendance records
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

                                        //Check data is exist
                                        if (LeaveResponse.Data.Count > 0)
                                        {
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
                                    }
                                    else
                                    {
                                        var errorObj = new Error("Api Leave Error", LeaveResponse.ErrorMessage);
                                        var errorWindow = new Error_Window(errorObj);
                                        errorWindow.Show();
                                        this.Close();
                                        return;
                                    }
                                    #endregion Leave request

                                    //Schedule Day Request
                                    #region Schedule Day Request
                                    var ScheduleDay_Request = new RestRequest("api/Users/{id}/schedule", Method.GET);
                                    ScheduleDay_Request.AddUrlSegment("id", Ins.SchoolID);
                                    ScheduleDay_Request.AddHeader("Authorization", "Bearer " + token);

                                    IRestResponse<List<Attendance_Schedule_Day>> ScheduleDay_Response = await client.ExecuteTaskAsync<List<Attendance_Schedule_Day>>(ScheduleDay_Request);

                                    if (ScheduleDay_Response.StatusCode == HttpStatusCode.OK && ScheduleDay_Response.Data.Count > 0)
                                    {
                                        //if same date Absent count remain same 
                                        if (Ins.LastUpdateDate == DateTime.Today.ToShortDateString())
                                        {
                                            var Schedules_Update = db.attendance_Schedule_Days.Where(s => s.Is_Abs_Count).Select(s => s.ScheduleID).ToList();

                                            ScheduleDay_Response.Data.Where(s => Schedules_Update.Contains(s.ScheduleID)).ToList().ForEach(s => s.Is_Abs_Count = true);
                                        }

                                        db.attendance_Schedule_Days.Clear();

                                        LocalData.Instance.Schedules = ScheduleDay_Response.Data;

                                        foreach (var item in LocalData.Instance.Schedules)
                                        {
                                            db.attendance_Schedule_Days.Add(item);
                                        }
                                    }
                                    else
                                    {
                                        var errorObj = new Error("Api Schedule Error", ScheduleDay_Response.ErrorMessage);
                                        var errorWindow = new Error_Window(errorObj);
                                        errorWindow.Show();
                                        this.Close();
                                        return;
                                    }
                                    #endregion Schedule Day Request

                                    await db.SaveChangesAsync();

                                    if (Server_datetime.AddMinutes(1) > DateTime.Now && Server_datetime.AddMinutes(-1) < DateTime.Now)
                                    {
                                        //DataUpdates request and Insert into DateUpdateList
                                        #region Data update list
                                        var DataUpdateRequest = new RestRequest("api/Users/{id}/updateInfo", Method.GET);
                                        DataUpdateRequest.AddUrlSegment("id", Ins.SchoolID);
                                        DataUpdateRequest.AddHeader("Authorization", "Bearer " + token);

                                        //execute the request
                                        IRestResponse<List<DataUpdateList>> DataUpdateResponse = await client.ExecuteTaskAsync<List<DataUpdateList>>(DataUpdateRequest);

                                        if (DataUpdateResponse.StatusCode == HttpStatusCode.OK && DataUpdateResponse.Data.Count > 0)
                                        {
                                            db.dataUpdateLists.AddRange(DataUpdateResponse.Data);
                                            await db.SaveChangesAsync();

                                            var setting = new Setting();
                                            setting.Show();
                                            this.Close();
                                            return;
                                        }

                                        #endregion Data update list

                                        var user = db.Users.FirstOrDefault();

                                        if (user != null)
                                        {
                                            //Device List
                                            #region Device List
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

                                                            var prev_log = device.Download_Prev_Logs();
                                                            var today_log = device.Download_Today_Logs();

                                                            await Machine.Save_logData(prev_log, today_log, Ins, device.Device);
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
                                                    LocalData.Current_Error.Message = "Device IP Not Found or Device Switch Off";
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
                                            #endregion Device List
                                        }
                                        else
                                        {
                                            LocalData.Current_Error.Message = "No User Found on PC!";
                                            LocalData.Current_Error.Type = Error_Type.UserInfoPage;

                                            var setting = new Setting();
                                            setting.Show();
                                            this.Close();
                                            return;
                                        }
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
                            return;
                        }
                    }
                    else
                    {
                        var Login = new Login_Window();
                        Login.Show();
                        this.Close();
                        return;
                    }
                }
            }
            catch (Exception ex)
            {
                var errorObj = new Error("System Error", ex.Message);
                var errorWindow = new Error_Window(errorObj);
                errorWindow.Show();
                this.Close();
            }
        }
    }
}
