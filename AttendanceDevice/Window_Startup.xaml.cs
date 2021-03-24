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
using System.Windows.Input;

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
                var deviceList = new List<DeviceConnection>();

                //Check Internet connection
                #region Check Internet
                if (await ApiUrl.CheckInterNet())
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
                    var ins = LocalData.Instance.institution;

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
                                    ins.Is_Employee_Attendance_Enable = schoolInfo.Is_Employee_Attendance_Enable;
                                    ins.Is_Student_Attendance_Enable = schoolInfo.Is_Student_Attendance_Enable;
                                    ins.Is_Today_Holiday = schoolInfo.Is_Today_Holiday;
                                    ins.Holiday_NotActive = schoolInfo.Holiday_NotActive;
                                    ins.LastUpdateDate = schoolInfo.LastUpdateDate;
                                   
                                    var serverDatetime = schoolInfo.Current_Datetime;
                                    db.Entry(ins).State = EntityState.Modified;

                                    //Leave request and Insert into attendance records
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

                                        //Check data is exist
                                        if (leaveResponse.Data.Count > 0)
                                        {
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
                                    }
                                    else
                                    {
                                        var errorObj = new Error("Api Leave Error", leaveResponse.ErrorMessage);
                                        var errorWindow = new Error_Window(errorObj);
                                        errorWindow.Show();
                                        this.Close();
                                        return;
                                    }
                                    #endregion Leave request

                                    //Schedule Day Request
                                    #region Schedule Day Request
                                    var scheduleDayRequest = new RestRequest("api/Users/{id}/schedule", Method.GET);
                                    scheduleDayRequest.AddUrlSegment("id", ins.SchoolID);
                                    scheduleDayRequest.AddHeader("Authorization", "Bearer " + token);

                                    var scheduleDayResponse = await client.ExecuteTaskAsync<List<Attendance_Schedule_Day>>(scheduleDayRequest);

                                    if (scheduleDayResponse.StatusCode == HttpStatusCode.OK && scheduleDayResponse.Data.Count > 0)
                                    {
                                        //if same date Absent count remain same 
                                        if (ins.LastUpdateDate == DateTime.Today.ToShortDateString())
                                        {
                                            var schedulesUpdate = db.attendance_Schedule_Days.Where(s => s.Is_Abs_Count).Select(s => s.ScheduleID).ToList();

                                            scheduleDayResponse.Data.Where(s => schedulesUpdate.Contains(s.ScheduleID)).ToList().ForEach(s => s.Is_Abs_Count = true);
                                        }

                                        db.attendance_Schedule_Days.Clear();

                                        LocalData.Instance.Schedules = scheduleDayResponse.Data;

                                        foreach (var item in LocalData.Instance.Schedules)
                                        {
                                            db.attendance_Schedule_Days.Add(item);
                                        }
                                    }
                                    else
                                    {
                                        var errorObj = new Error("Api Schedule Error", "No Schedule Added In Sikkhaloy");
                                        var errorWindow = new Error_Window(errorObj);
                                        errorWindow.Show();
                                        //this.Close();
                                        //return;
                                    }
                                    #endregion Schedule Day Request

                                    await db.SaveChangesAsync();

                                    if (serverDatetime.AddMinutes(1) > DateTime.Now && serverDatetime.AddMinutes(-1) < DateTime.Now)
                                    {
                                        //DataUpdates request and Insert into DateUpdateList
                                        #region Data update list
                                        var dataUpdateRequest = new RestRequest("api/Users/{id}/updateInfo", Method.GET);
                                        dataUpdateRequest.AddUrlSegment("id", ins.SchoolID);
                                        dataUpdateRequest.AddHeader("Authorization", "Bearer " + token);

                                        //execute the request
                                        var dataUpdateResponse = await client.ExecuteTaskAsync<List<DataUpdateList>>(dataUpdateRequest);

                                        if (dataUpdateResponse.StatusCode == HttpStatusCode.OK && dataUpdateResponse.Data.Count > 0)
                                        {
                                            db.dataUpdateLists.AddRange(dataUpdateResponse.Data);
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
                                                        if (!status.IsSuccess) continue;

                                                        dCheck = true;

                                                        //Set server time to device
                                                        await Task.Run(() => device.SetDateTime());

                                                        var prevLog = device.Download_Prev_Logs();
                                                        var todayLog = device.Download_Today_Logs();

                                                        await Machine.Save_logData(prevLog, todayLog, ins, device.Device);
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
                                                    LocalData.Current_Error.Message = "Device IP Not Found or Device Switch Off";
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
                                            #endregion Device List
                                        }
                                        else
                                        {
                                            LocalData.Current_Error.Message = "No User Found on PC!";
                                            LocalData.Current_Error.Type = Error_Type.UserInfoPage;

                                            var setting = new Setting();
                                            setting.Show();
                                            this.Close();
                                        }
                                    }
                                    else
                                    {
                                        var errorObj = new Error("Invalid", "Invalid PC Date Time. \n Server Time: " + serverDatetime.ToString("d MMM yy (hh:mm tt)"));
                                        var errorWindow = new Error_Window(errorObj);
                                        errorWindow.Show();
                                        this.Close();
                                    }
                                }
                                else
                                {
                                    var errorObj = new Error("Deactivate", "Institution Deactivate From Authority!");
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
            }
            catch (Exception ex)
            {
                var errorObj = new Error("System Error", ex.Message);
                var errorWindow = new Error_Window(errorObj);
                errorWindow.Show();
                this.Close();
            }
        }

        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
                this.DragMove();
        }
    }
}
