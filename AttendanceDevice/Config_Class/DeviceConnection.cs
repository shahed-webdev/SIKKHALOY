using AttendanceDevice.Model;
using AttendanceDevice.ViewModel;
using MaterialDesignThemes.Wpf;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Threading;
using zkemkeeper;

namespace AttendanceDevice.Config_Class
{
    public enum DataCode
    {
        NumberOfAdmin = 1,
        NumberOfUsers = 2,
        NumberOfFp = 3,
        NumberOfPasswords = 4,
        NumberOfOperationRecords = 5,
        AttendanceRecords = 6,
        FpCapacity = 7,
        UserCapacity = 8,
        AttendanceRecordCapacity = 9,
        RemainingPfCapacity = 10,
        RemainingUserCapacity = 11,
        RemainingAttendanceRecordCapacity = 12
    }
    public class DeviceConnection
    {
        private const int PrevDayLogCountable = 7;
        public Card EnrollUserCard { get; set; }
        public DialogHost EnrollUserDialogHost { get; set; }
        public bool IsSdkFullSupported { get; set; }
        public TextBlock FingerprintMessage { get; set; }
        public ListBox LogViewListBox { get; set; }
        public Device Device { get; private set; }
        private DeviceReturn Returns { get; set; }
        public CZKEM axCZKEM1 { get; private set; }

        private DispatcherTimer _dialogTimer = new DispatcherTimer();
        private void _dialogTimer_Tick(object sender, EventArgs e)
        {
            EnrollUserDialogHost.IsOpen = false;
        }
        private void EnrollUserDialog_OnDialogOpened(object sender, DialogOpenedEventArgs eventargs)
        {
            //Timer-setup
            _dialogTimer.Interval = TimeSpan.FromSeconds(5);
            _dialogTimer.Tick += _dialogTimer_Tick;
            _dialogTimer.Start();
        }

        private void EnrollUserDialog_OnDialogClosing(object sender, DialogClosingEventArgs eventargs)
        {
            _dialogTimer.Stop();
            _dialogTimer = null;
        }

        public async void axCZKEM1_OnAttTransactionEx(string enrollNumber, int isInValid, int attState, int verifyMethod, int year, int month, int day, int hour, int minute, int second, int workCode)
        {
            var deviceId = Convert.ToInt32(enrollNumber);
            var dt = new DateTime(year, month, day, hour, minute, second);
            var time = new TimeSpan(hour, minute, second);
            var userView = LocalData.Instance.GetUserView(deviceId);

            if (userView == null)
            {
                userView = new UserView { Name = "User Not found on PC" };
                EnrollUserCard.DataContext = userView;
                return;
            }

            //popup for Show the user 
            if (EnrollUserDialogHost != null)
            {
                EnrollUserDialogHost.DialogOpened += EnrollUserDialog_OnDialogOpened;
                EnrollUserDialogHost.DialogClosing += EnrollUserDialog_OnDialogClosing;

                if (EnrollUserDialogHost.CurrentSession == null)
                    EnrollUserDialogHost.IsOpen = true;

                _dialogTimer.Interval = TimeSpan.FromSeconds(5);
            }



            userView.Enroll_Time = dt;
            var sDate = dt.ToShortDateString();

            EnrollUserCard.DataContext = userView;


            var isStuDisable = userView.Is_Student && !LocalData.Instance.institution.Is_Student_Attendance_Enable;
            var isEmpDisable = !userView.Is_Student && !LocalData.Instance.institution.Is_Employee_Attendance_Enable;
            var schedule = LocalData.Instance.GetUserSchedule(userView.ScheduleID);

            string reason;
            // Device Attendance Disable
            if (!LocalData.Instance.institution.Is_Device_Attendance_Enable)
            {
                reason = "Device Attendance Disable";
                await LogBackupInsert(deviceId, dt, reason);
            }
            // Student Attendance Disable
            else if (isStuDisable)
            {
                reason = "Student Attendance Disable";
                await LogBackupInsert(deviceId, dt, reason);
            }
            // Employee Attendance Disable
            else if (isEmpDisable)
            {
                reason = "Employee Attendance Disable";
                await LogBackupInsert(deviceId, dt, reason);
            }
            // Today Check
            else if (sDate != DateTime.Today.ToShortDateString())
            {
                reason = "Not Current Data";
                await LogBackupInsert(deviceId, dt, reason);
            }
            //Holiday attendance disable
            else if (LocalData.Instance.institution.Is_Today_Holiday && !LocalData.Instance.institution.Holiday_NotActive)
            {
                reason = "Holiday attendance disable";
                await LogBackupInsert(deviceId, dt, reason);
            }
            //Schedule Off day
            else if (!schedule.Is_OnDay)
            {
                reason = "Schedule Off Day";
                await LogBackupInsert(deviceId, dt, reason);
            }
            // Insert or Update Attendance Records
            else
            {
                using (var db = new ModelContext())
                {
                    var attRecord = await db.attendance_Records.Where(a => a.DeviceID == deviceId && a.AttendanceDate == sDate).FirstOrDefaultAsync();
                    var sStartTime = TimeSpan.Parse(schedule.StartTime);
                    var sLateTime = TimeSpan.Parse(schedule.LateEntryTime);
                    var sEndTime = TimeSpan.Parse(schedule.EndTime);

                    if (attRecord == null)
                    {
                        attRecord = new Attendance_Record();

                        if (time > sEndTime)
                        {
                            //Enroll after end time (as first enroll)
                        }
                        else
                        {
                            attRecord.AttendanceDate = sDate;
                            attRecord.DeviceID = deviceId;
                            attRecord.EntryTime = time.ToString();

                            if (time <= sStartTime)
                            {
                                attRecord.AttendanceStatus = "Pre";
                            }
                            else if (time <= sLateTime)
                            {
                                attRecord.AttendanceStatus = "Late";
                            }
                            else if (time <= sEndTime)
                            {
                                attRecord.AttendanceStatus = "Late Abs";
                            }

                            db.attendance_Records.Add(attRecord);
                            db.Entry(attRecord).State = EntityState.Added;
                        }
                    }
                    else
                    {
                        if (attRecord.AttendanceStatus == "Abs")
                        {
                            if (time < sEndTime)
                            {
                                attRecord.AttendanceStatus = "Late Abs";
                            }

                            attRecord.EntryTime = time.ToString();
                            attRecord.Is_Sent = false;
                        }
                        else
                        {
                            if (time > sLateTime && time < sEndTime && !attRecord.Is_OUT && TimeSpan.Parse(attRecord.EntryTime).TotalMinutes + 10 < time.TotalMinutes)
                            {
                                attRecord.ExitStatus = "Early Leave";
                                attRecord.Is_OUT = true;
                                attRecord.ExitTime = time.ToString();
                            }
                            else if (time > sLateTime && time < sEndTime && attRecord.Is_OUT && TimeSpan.Parse(attRecord.ExitTime).TotalMinutes + 10 < time.TotalMinutes)
                            {
                                attRecord.ExitStatus = "Early Leave";
                                attRecord.Is_OUT = true;
                                attRecord.ExitTime = time.ToString();
                                attRecord.Is_Updated = false;
                            }
                            else if (time > sEndTime)
                            {
                                attRecord.ExitStatus = "Out";
                                attRecord.Is_OUT = true;
                                attRecord.ExitTime = time.ToString();
                                attRecord.Is_Updated = false;
                            }
                        }

                        db.Entry(attRecord).State = EntityState.Modified;
                    }

                    await db.SaveChangesAsync();
                }
            }

            //string fromTime = DateTime.Today.ToString("yyyy-MM-dd 00:00:00");
            //string toTime = DateTime.Today.ToString("yyyy-MM-dd 23:00:00");

            LogViewListBox.ItemsSource = Machine.GetDailyAttendanceRecords(AttType.All);

            if (!this.IsSdkFullSupported)
            {
                this.ClearAll_Logs();
            }
        }

        public void axCZKEM1_OnFingerFeature(int score)
        {
            FingerprintMessage.Text = "Press finger score=" + score;
        }


        public void axCZKEM1_OnEnrollFingerEx(string enrollNumber, int fingerIndex, int actionResult, int templateLength)
        {
            FingerprintMessage.Text = actionResult == 0 ? $"Enroll finger succeed. UserId: ${enrollNumber}. Finger Index: ${fingerIndex}" : $"Enroll finger failed. Result: ${actionResult}";
        }

        private async Task LogBackupInsert(int deviceId, DateTime dt, string reason)
        {
            using (var db = new ModelContext())
            {
                var logBackup = new AttendanceLog_Backup()
                {
                    DeviceID = deviceId,
                    Entry_Date = dt.ToShortDateString(),
                    Entry_Time = dt.ToShortTimeString(),
                    Entry_Day = dt.ToString("dddd"),
                    Backup_Reason = reason
                };


                this.Device.Last_Down_Log_Time = dt.ToString("yyyy-MM-dd HH:mm:ss");
                db.Devices.Add(Device);
                db.Entry(Device).State = EntityState.Modified;

                db.attendanceLog_Backups.Add(logBackup);
                await db.SaveChangesAsync();
            }
        }

        public DeviceConnection(Device device)
        {


            axCZKEM1 = new CZKEM();

            this.Device = device;
            Returns = new DeviceReturn();
        }

        public DeviceReturn ConnectDevice()
        {
            if (this.Device.DeviceIP == "")
            {
                Returns.Message = "IP cannot be null!";
                Returns.Code = -1;
                Returns.IsSuccess = false;

                return Returns;
            }

            if (this.Device.Port <= 0 || this.Device.Port > 65535)
            {
                Returns.Message = "Port illegal!";
                Returns.Code = -1;
                Returns.IsSuccess = false;

                return Returns;
            }

            if (this.Device.CommKey < 0 || this.Device.CommKey > 999999)
            {
                Returns.Message = "CommKey illegal!";
                Returns.Code = -1;
                Returns.IsSuccess = false;

                return Returns;
            }

            if (IsConnected())
            {
                Returns.Message = "Device is already Connected!";
                Returns.Code = 0;
                Returns.IsSuccess = true;
            }


            if (!axCZKEM1.SetCommPassword(this.Device.CommKey))
            {
                var idwErrorCode = 0;
                axCZKEM1.GetLastError(ref idwErrorCode);

                Returns.Message = $"Unable to connect the device, Error Code: ${idwErrorCode}";
                Returns.Code = -1;
                Returns.IsSuccess = false;

                return Returns;
            }

            if (axCZKEM1.Connect_Net(this.Device.DeviceIP, this.Device.Port))
            {
                Returns.Message = $"Connected with device {this.Device.DeviceName}!";
                Returns.Code = 1;
                Returns.IsSuccess = true;

                this.IsSdkFullSupported = ClearPrevLog();

                if (axCZKEM1.RegEvent(Machine.Number, 1))
                {
                    axCZKEM1.OnAttTransactionEx += axCZKEM1_OnAttTransactionEx;
                }

                return Returns;
            }
            else
            {
                var idwErrorCode = 0;
                axCZKEM1.GetLastError(ref idwErrorCode);
                Returns.Message = $"Unable to connect the device, Error Code: ${idwErrorCode}";
                Returns.Code = -1;
                Returns.IsSuccess = false;

                return Returns;
            }
        }
        public DeviceReturn ConnectDeviceWithoutEvent()
        {
            if (this.Device.DeviceIP == "")
            {
                Returns.Message = "IP cannot be null!";
                Returns.Code = -1;
                Returns.IsSuccess = false;

                return Returns;
            }

            if (this.Device.Port <= 0 || this.Device.Port > 65535)
            {
                Returns.Message = "Port illegal!";
                Returns.Code = -1;
                Returns.IsSuccess = false;

                return Returns;
            }

            if (this.Device.CommKey < 0 || this.Device.CommKey > 999999)
            {
                Returns.Message = "CommKey illegal!";
                Returns.Code = -1;
                Returns.IsSuccess = false;

                return Returns;
            }

            if (IsConnected())
            {
                Returns.Message = "Device is already Connected !";
                Returns.Code = 0;
                Returns.IsSuccess = true;
            }


            if (!axCZKEM1.SetCommPassword(this.Device.CommKey))
            {
                var idwErrorCode = 0;
                axCZKEM1.GetLastError(ref idwErrorCode);

                Returns.Message = $"Unable to connect the device, Error Code: {idwErrorCode}";
                Returns.Code = -1;
                Returns.IsSuccess = false;

                return Returns;
            }

            if (axCZKEM1.Connect_Net(this.Device.DeviceIP, this.Device.Port))
            {
                Returns.Message = $"Connected with device {this.Device.DeviceName}!";
                Returns.Code = 1;
                Returns.IsSuccess = true;

                return Returns;
            }
            else
            {
                var idwErrorCode = 0;
                axCZKEM1.GetLastError(ref idwErrorCode);
                Returns.Message = $"Unable to connect the device, ErrorCode: {idwErrorCode}";
                Returns.Code = -1;
                Returns.IsSuccess = false;

                return Returns;
            }
        }
        public DeviceReturn DisconnectDevice()
        {
            if (IsConnected())
            {
                axCZKEM1.Disconnect();
                //sta_UnRegRealTime();

                Returns.Message = "Disconnect with device !";
                Returns.Code = -2;
                Returns.IsSuccess = true;
            }
            else
            {
                Returns.Message = "Device is already Disconnected !";
                Returns.Code = -2;
                Returns.IsSuccess = false;
            }

            return Returns;
        }

        public bool IsConnected()
        {
            var pingCheck = Device_PingTest.PingHost(this.Device.DeviceIP);

            if (!pingCheck) return false;

            var a = -1;
            axCZKEM1.GetConnectStatus(ref a);
            return a == 0;
        }


        public async Task<bool> IsConnectedAsync()
        {
            var checkIp = await Device_PingTest.PingHostAsync(this.Device.DeviceIP);
            if (!checkIp) return false;

            var dwErrorCode = -1;
            axCZKEM1.GetConnectStatus(ref dwErrorCode);
            return dwErrorCode == 0;
        }

        public string DeviceSerialNumber()
        {
            axCZKEM1.GetSerialNumber(Machine.Number, out var deviceSn);
            return deviceSn;
        }

        public DateTime GetDateTime()
        {
            var idwYear = 0;
            var idwMonth = 0;
            var idwDay = 0;
            var idwHour = 0;
            var idwMinute = 0;
            var idwSecond = 0;

            axCZKEM1.GetDeviceTime(Machine.Number, ref idwYear, ref idwMonth, ref idwDay, ref idwHour, ref idwMinute, ref idwSecond);

            return new DateTime(idwYear, idwMonth, idwDay, idwHour, idwMinute, idwSecond);
        }

        public bool SetDateTime()
        {
            if (!axCZKEM1.SetDeviceTime(Machine.Number)) return false;

            axCZKEM1.RefreshData(Machine.Number);
            return true;
        }
        public bool Restart()
        {
            return axCZKEM1.RestartDevice(Machine.Number);
        }
        public bool PowerOff()
        {
            return axCZKEM1.PowerOffDevice(Machine.Number);
        }
        public bool Duplicate_Punch_Time_Reset()
        {
            //Duplicate time 5min 
            return axCZKEM1.SetDeviceInfo(Machine.Number, 8, 5);
        }

        public void Upload_User(List<User> users)
        {
            axCZKEM1.EnableDevice(Machine.Number, false);
            try
            {
                var batchUpdate = axCZKEM1.BeginBatchUpdate(Machine.Number, 1);
                foreach (var user in users)
                {
                    axCZKEM1.SetStrCardNumber(user.RFID);
                    axCZKEM1.SSR_SetUserInfo(Machine.Number, user.DeviceID.ToString(), user.Name, "", 0, true);
                }

                if (!batchUpdate) return;
                axCZKEM1.BatchUpdate(Machine.Number);
            }
            catch
            {
                // ignored
            }
            finally
            {
                axCZKEM1.EnableDevice(Machine.Number, true);
            }
        }

        public List<User> Download_User()
        {
            if (!IsConnected()) return new List<User>();

            var users = new List<User>();

            axCZKEM1.EnableDevice(Machine.Number, false);
            try
            {
                axCZKEM1.ReadAllUserID(Machine.Number);

                while (axCZKEM1.SSR_GetAllUserInfo(Machine.Number, out var deviceId, out var name, out _, out _, out _))
                {
                    axCZKEM1.GetStrCardNumber(out var rfId);

                    //RFID = "";
                    //if (axCZKEM1.GetStrCardNumber(out RFID))
                    //{
                    if (string.IsNullOrEmpty(rfId) || rfId == "0")
                        rfId = null;
                    //}
                    //if (!string.IsNullOrEmpty(name))
                    //{
                    //    int index = name.IndexOf("\0");
                    //    if (index > 0)
                    //    {
                    //        name = name.Substring(0, index);
                    //    }
                    //}

                    var user = new User { DeviceID = Convert.ToInt32(deviceId), Name = name, RFID = rfId };

                    users.Add(user);
                }
            }
            catch
            {
                // ignored
            }
            finally
            {
                axCZKEM1.EnableDevice(Machine.Number, true);
            }
            return users;
        }
        public List<LogView> DownloadPrevLogs()
        {
            if (!IsConnected()) return new List<LogView>();

            var logs = new List<LogView>();
            axCZKEM1.EnableDevice(Machine.Number, false);
            try
            {
                var sdwEnrollNumber = "";
                var idwYear = 0;
                var idwMonth = 0;
                var idwDay = 0;
                var idwHour = 0;
                var idwMinute = 0;
                var idwSecond = 0;
                var idWorkCode = 0;

                var fromTime = DateTime.Today.AddDays(-PrevDayLogCountable).ToString("yyyy-MM-dd 00:00:00");

                if (DateTime.TryParse(Device.Last_Down_Log_Time, out var deviceLastUpdate))
                {
                    if ((DateTime.Today - deviceLastUpdate).TotalDays < PrevDayLogCountable)
                    {
                        fromTime = Device.Last_Down_Log_Time;
                    }
                }

                var toTime = DateTime.Today.ToString("yyyy-MM-dd 00:00:00");

                if (axCZKEM1.ReadTimeGLogData(Machine.Number, fromTime, toTime))
                {
                    while (axCZKEM1.SSR_GetGeneralLogData(Machine.Number, out sdwEnrollNumber, out _, out _, out idwYear, out idwMonth, out idwDay, out idwHour, out idwMinute, out idwSecond, ref idWorkCode))//get records from the memory
                    {
                        var deviceId = Convert.ToInt32(sdwEnrollNumber);
                        var dt = new DateTime(idwYear, idwMonth, idwDay, idwHour, idwMinute, idwSecond);
                        var time = new TimeSpan(idwHour, idwMinute, idwSecond);
                        var sDate = dt.ToShortDateString();

                        var log = new LogView
                        {
                            DeviceId = deviceId,
                            EntryDate = sDate,
                            EntryTime = time,
                            EntryDay = dt.ToString("dddd"),
                            EntryDateTime = dt
                        };
                        logs.Add(log);
                    }
                }
                else
                {
                    if (axCZKEM1.ReadGeneralLogData(Machine.Number))
                    {
                        while (axCZKEM1.SSR_GetGeneralLogData(Machine.Number, out sdwEnrollNumber, out _, out _, out idwYear, out idwMonth, out idwDay, out idwHour, out idwMinute, out idwSecond, ref idWorkCode))//get records from the memory
                        {
                            var deviceId = Convert.ToInt32(sdwEnrollNumber);
                            var dt = new DateTime(idwYear, idwMonth, idwDay, idwHour, idwMinute, idwSecond);
                            var time = new TimeSpan(idwHour, idwMinute, idwSecond);
                            var sDate = dt.ToShortDateString();

                            if (dt.Date == DateTime.Today.Date) continue;

                            var log = new LogView
                            {
                                DeviceId = deviceId,
                                EntryDate = sDate,
                                EntryTime = time,
                                EntryDay = dt.ToString("dddd"),
                                EntryDateTime = dt,
                            };

                            logs.Add(log);
                        }
                    }

                }
            }
            catch
            {
                // ignored
            }
            finally
            {
                axCZKEM1.EnableDevice(Machine.Number, true);
            }

            return logs.OrderBy(l => l.EntryDateTime).ToList();
        }

        public List<LogView> DownloadTodayLogs()
        {
            if (!IsConnected()) return new List<LogView>();

            var logs = new List<LogView>();
            axCZKEM1.EnableDevice(Machine.Number, false);
            try
            {
                var sdwEnrollNumber = "";
                var idwYear = 0;
                var idwMonth = 0;
                var idwDay = 0;
                var idwHour = 0;
                var idwMinute = 0;
                var idwSecond = 0;
                var idWorkCode = 0;

                var fromTime = DateTime.Today.ToString("yyyy-MM-dd 00:00:00");
                var toTime = DateTime.Today.AddDays(1).ToString("yyyy-MM-dd 00:00:00");

                if (axCZKEM1.ReadTimeGLogData(Machine.Number, fromTime, toTime))
                {
                    while (axCZKEM1.SSR_GetGeneralLogData(Machine.Number, out sdwEnrollNumber, out _, out _, out idwYear, out idwMonth, out idwDay, out idwHour, out idwMinute, out idwSecond, ref idWorkCode))//get records from the memory
                    {
                        var deviceId = Convert.ToInt32(sdwEnrollNumber);
                        var dt = new DateTime(idwYear, idwMonth, idwDay, idwHour, idwMinute, idwSecond);
                        var time = new TimeSpan(idwHour, idwMinute, idwSecond);
                        var sDate = dt.ToShortDateString();

                        var log = new LogView()
                        {
                            DeviceId = deviceId,
                            EntryDate = sDate,
                            EntryTime = time,
                            EntryDay = dt.ToString("dddd"),
                            EntryDateTime = dt
                        };

                        logs.Add(log);
                    }
                }
                else
                {
                    if (axCZKEM1.ReadGeneralLogData(Machine.Number))
                    {
                        while (axCZKEM1.SSR_GetGeneralLogData(Machine.Number, out sdwEnrollNumber, out _, out _, out idwYear, out idwMonth, out idwDay, out idwHour, out idwMinute, out idwSecond, ref idWorkCode))//get records from the memory
                        {
                            var deviceId = Convert.ToInt32(sdwEnrollNumber);
                            var dt = new DateTime(idwYear, idwMonth, idwDay, idwHour, idwMinute, idwSecond);
                            var time = new TimeSpan(idwHour, idwMinute, idwSecond);
                            var sDate = dt.ToShortDateString();

                            var log = new LogView()
                            {
                                DeviceId = deviceId,
                                EntryDate = sDate,
                                EntryTime = time,
                                EntryDay = dt.ToString("dddd"),
                                EntryDateTime = dt
                            };

                            logs.Add(log);
                        }

                    }
                }
            }
            catch
            {
                // ignored
            }
            finally
            {
                axCZKEM1.EnableDevice(Machine.Number, true);
            }
            return logs.OrderBy(l => l.EntryDateTime).ToList();
        }

        public bool ClearAllUsers()
        {
            if (!IsConnected()) return false;

            bool isClear;
            axCZKEM1.EnableDevice(Machine.Number, false);

            if (axCZKEM1.ClearData(Machine.Number, 5))
            {
                axCZKEM1.RefreshData(Machine.Number);
                isClear = true;
            }
            else
            {
                isClear = false;
            }

            axCZKEM1.EnableDevice(Machine.Number, true);
            return isClear;
        }

        public bool ClearAll_Logs()
        {
            if (!IsConnected()) return false;

            axCZKEM1.EnableDevice(Machine.Number, false);

            var isClear = axCZKEM1.ClearGLog(Machine.Number);

            axCZKEM1.EnableDevice(Machine.Number, true);

            return isClear;
        }
        public bool ClearAll_FPs()
        {
            if (!IsConnected()) return false;

            bool isClear;
            axCZKEM1.EnableDevice(Machine.Number, false);

            if (axCZKEM1.ClearData(Machine.Number, 2))
            {
                axCZKEM1.RefreshData(Machine.Number);
                isClear = true;
            }
            else
            {
                isClear = false;
            }

            axCZKEM1.EnableDevice(Machine.Number, true);
            return isClear;
        }

        public bool ClearPrevLog()
        {
            bool isClear;
            axCZKEM1.EnableDevice(Machine.Number, false);

            var fromTime = DateTime.Today.AddDays(-PrevDayLogCountable).ToString("yyyy-MM-dd 00:00:00");

            if (axCZKEM1.DeleteAttlogByTime(Machine.Number, fromTime))
            {
                axCZKEM1.RefreshData(Machine.Number);
                isClear = true;
            }
            else
            {
                isClear = false;
            }

            axCZKEM1.EnableDevice(Machine.Number, true);
            return isClear;
        }

        public List<LogView> DownloadLogs()
        {
            if (!IsConnected()) return new List<LogView>();

            var logs = new List<LogView>();
            axCZKEM1.EnableDevice(Machine.Number, false);
            try
            {
                var idWorkCode = 0;

                if (axCZKEM1.ReadGeneralLogData(Machine.Number))
                {
                    while (axCZKEM1.SSR_GetGeneralLogData(Machine.Number, out var sdwEnrollNumber, out _, out _, out var idwYear, out var idwMonth, out var idwDay, out var idwHour, out var idwMinute, out var idwSecond, ref idWorkCode))//get records from the memory
                    {
                        var deviceId = Convert.ToInt32(sdwEnrollNumber);
                        var dt = new DateTime(idwYear, idwMonth, idwDay, idwHour, idwMinute, idwSecond);
                        var time = new TimeSpan(idwHour, idwMinute, idwSecond);
                        var sDate = dt.ToShortDateString();

                        var log = new LogView()
                        {
                            DeviceId = deviceId,
                            EntryDate = sDate,
                            EntryTime = time,
                            EntryDay = dt.ToString("dddd"),
                            EntryDateTime = dt
                        };
                        logs.Add(log);
                    }
                }
            }
            catch
            {
                // ignored
            }
            finally
            {
                axCZKEM1.EnableDevice(Machine.Number, true);
            }
            return logs.OrderBy(l => l.EntryDateTime).ToList();
        }
        public DeviceDetails GetDeviceDetails()
        {
            var userCapacity = 0;
            var userCount = 0;
            var fpCnt = 0;
            var fpCapacity = 0;
            var recordCnt = 0;
            var recordCapacity = 0;
            var duplicatePunchTime = 0;

            //axCZKEM1.EnableDevice(Machine.Number, false);//disable the device

            axCZKEM1.GetDeviceStatus(Machine.Number, (int)DataCode.UserCapacity, ref userCapacity);
            axCZKEM1.GetDeviceStatus(Machine.Number, (int)DataCode.NumberOfUsers, ref userCount);
            axCZKEM1.GetDeviceStatus(Machine.Number, (int)DataCode.NumberOfFp, ref fpCnt);
            axCZKEM1.GetDeviceStatus(Machine.Number, (int)DataCode.FpCapacity, ref fpCapacity);
            axCZKEM1.GetDeviceStatus(Machine.Number, (int)DataCode.AttendanceRecords, ref recordCnt);
            axCZKEM1.GetDeviceStatus(Machine.Number, (int)DataCode.AttendanceRecordCapacity, ref recordCapacity);
            axCZKEM1.GetDeviceInfo(Machine.Number, 8, ref duplicatePunchTime);

            // axCZKEM1.EnableDevice(Machine.Number, true);//enable the device

            return new DeviceDetails(fpCapacity, fpCnt, userCapacity, userCount, recordCapacity, recordCnt, duplicatePunchTime);
        }


        public int FP_Add(string deviceId, int fingerIndex)
        {
            if (axCZKEM1.RegEvent(Machine.Number, 65535))
            {
                axCZKEM1.OnFingerFeature += new zkemkeeper._IZKEMEvents_OnFingerFeatureEventHandler(axCZKEM1_OnFingerFeature);
                axCZKEM1.OnEnrollFingerEx += new zkemkeeper._IZKEMEvents_OnEnrollFingerExEventHandler(axCZKEM1_OnEnrollFingerEx);
            }

            var idwErrorCode = 0;
            const int iFlag = 1;

            axCZKEM1.CancelOperation();

            //If the specified index of user's templates has existed ,delete it first
            axCZKEM1.SSR_DelUserTmpExt(Machine.Number, deviceId, fingerIndex);

            if (axCZKEM1.StartEnrollEx(deviceId, fingerIndex, iFlag))
            {
                FingerprintMessage.Text = $"Start to Enroll a new User,UserId: {deviceId} FingerId: {fingerIndex}";

                if (axCZKEM1.StartIdentify())
                {
                    FingerprintMessage.Text = $"Enroll a new User,UserId: {deviceId}";
                }
                //After enrolling templates,you should let the device into the 1:N verification condition
            }
            else
            {
                axCZKEM1.GetLastError(ref idwErrorCode);
                FingerprintMessage.Text = $"Operation failed, Error Code: {idwErrorCode}";
            }
            //axCZKEM1.OnFingerFeature -= new zkemkeeper._IZKEMEvents_OnFingerFeatureEventHandler(axCZKEM1_OnFingerFeature);
            // axCZKEM1.OnEnrollFingerEx -= new zkemkeeper._IZKEMEvents_OnEnrollFingerExEventHandler(axCZKEM1_OnEnrollFingerEx);
            return 1;
        }

        public int FP_Delete(string deviceId, int fingerIndex)
        {
            var idwErrorCode = 0;

            axCZKEM1.CancelOperation();

            if (axCZKEM1.SSR_DelUserTmpExt(Machine.Number, deviceId, fingerIndex))
            {
                axCZKEM1.RefreshData(Machine.Number);//the data in the device should be refreshed
            }
            else
            {
                axCZKEM1.GetLastError(ref idwErrorCode);
            }
            return idwErrorCode;
        }
    }
    public class DeviceDetails
    {
        public DeviceDetails(int fpCapacity, int numberOfFp, int userCapacity, int numberOfUsers, int attendanceRecordCapacity, int attendanceRecords, int duplicatePunchTime)
        {
            this.FpCapacity = fpCapacity;
            this.NumberOfFp = numberOfFp;
            this.UserCapacity = userCapacity;
            this.NumberOfUsers = numberOfUsers;
            this.AttendanceRecordCapacity = attendanceRecordCapacity;
            this.AttendanceRecords = attendanceRecords;
            this.DuplicatePunchTime = duplicatePunchTime;
        }

        public int FpCapacity { get; private set; }
        public int NumberOfFp { get; private set; }
        public int UserCapacity { get; private set; }
        public int NumberOfUsers { get; private set; }
        public int AttendanceRecordCapacity { get; private set; }
        public int AttendanceRecords { get; private set; }
        public int DuplicatePunchTime { get; private set; }
    }


}
