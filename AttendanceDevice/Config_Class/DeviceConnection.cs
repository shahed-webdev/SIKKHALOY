using AttendanceDevice.Model;
using AttendanceDevice.ViewModel;
using MaterialDesignThemes.Wpf;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Controls;
using zkemkeeper;

namespace AttendanceDevice.Config_Class
{
    public enum Data_Code
    {
        Number_of_admin = 1,
        Number_of_users = 2,
        Number_of_FP = 3,
        Number_of_passwords = 4,
        Number_of_operation_records = 5,
        Attendance_Records = 6,
        FP_Capacity = 7,
        User_capacity = 8,
        Attendance_Record_Capacity = 9,
        Remaining_PF_Capacity = 10,
        Remaining_User_Capacity = 11,
        Remaining_Attendance_Record_Capacity = 12
    }
    public class DeviceConnection
    {
        public Card EnrollUserCard { get; set; }
        public DialogHost EnrollUserDialogHost { get; set; }
        public bool IsSdkFullSupported { get; set; }

        public TextBlock FpMsg { get; set; }

        public ListBox LogViewLb { get; set; }
        private const int PrevLogCountDay = 7;
        public Device Device { get; private set; }
        public CZKEM axCZKEM1 { get; private set; }
        private DeviceReturn Returns { get; set; }


        public async void axCZKEM1_OnAttTransactionEx(string EnrollNumber, int IsInValid, int AttState, int VerifyMethod, int Year, int Month, int Day, int Hour, int Minute, int Second, int WorkCode)
        {
            var deviceId = Convert.ToInt32(EnrollNumber);
            var dt = new DateTime(Year, Month, Day, Hour, Minute, Second);
            var time = new TimeSpan(Hour, Minute, Second);
            var userView = LocalData.Instance.GetUserView(deviceId);

            if (userView == null)
            {
                userView = new UserView {Name = "User Not found on PC"};
                EnrollUserCard.DataContext = userView;
                return;
            }

            EnrollUserDialogHost.IsOpen = true;

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
                await log_Backup_Insert(deviceId, dt, reason);
            }
            // Student Attendance Disable
            else if (isStuDisable)
            {
                reason = "Student Attendance Disable";
                await log_Backup_Insert(deviceId, dt, reason);
            }
            // Employee Attendance Disable
            else if (isEmpDisable)
            {
                reason = "Employee Attendance Disable";
                await log_Backup_Insert(deviceId, dt, reason);
            }
            // Today Check
            else if (sDate != DateTime.Today.ToShortDateString())
            {
                reason = "Not Current Data";
                await log_Backup_Insert(deviceId, dt, reason);
            }
            //Holiday attendance disable
            else if (LocalData.Instance.institution.Is_Today_Holiday && !LocalData.Instance.institution.Holiday_NotActive)
            {
                reason = "Holiday attendance disable";
                await log_Backup_Insert(deviceId, dt, reason);
            }
            //Schedule Off day
            else if (!schedule.Is_OnDay)
            {
                reason = "Schedule Off Day";
                await log_Backup_Insert(deviceId, dt, reason);
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

            LogViewLb.ItemsSource = Machine.GetAttendance(AttType.All);

            if (!this.IsSdkFullSupported)
            {
                this.ClearAll_Logs();
            }
        }

        public void axCZKEM1_OnFingerFeature(int score)
        {
            FpMsg.Text = "Press finger score=" + score;
        }


        public void axCZKEM1_OnEnrollFingerEx(string EnrollNumber, int FingerIndex, int ActionResult, int TemplateLength)
        {
            if (ActionResult == 0)
            {
                FpMsg.Text = "Enroll finger succeed. UserID=" + EnrollNumber.ToString() + "...FingerIndex=" + FingerIndex.ToString();
            }
            else
            {
                FpMsg.Text = "Enroll finger failed. Result=" + ActionResult.ToString();
            }
        }
        private async Task log_Backup_Insert(int DeviceID, DateTime dt, string Reason)
        {
            using (var db = new ModelContext())
            {
                var logBackup = new AttendanceLog_Backup()
                {
                    DeviceID = DeviceID,
                    Entry_Date = dt.ToShortDateString(),
                    Entry_Time = dt.ToShortTimeString(),
                    Entry_Day = dt.ToString("dddd"),
                    Backup_Reason = Reason
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

                Returns.Message = "Unable to connect the device, ErrorCode=" + idwErrorCode;
                Returns.Code = -1;
                Returns.IsSuccess = false;

                return Returns;
            }

            if (axCZKEM1.Connect_Net(this.Device.DeviceIP, this.Device.Port))
            {
                Returns.Message = $"Connected with device {this.Device.DeviceName}!";
                Returns.Code = 1;
                Returns.IsSuccess = true;

                this.IsSdkFullSupported = Clear_PrevLog();

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
                Returns.Message = "Unable to connect the device, ErrorCode=" + idwErrorCode;
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
                int idwErrorCode = 0;
                axCZKEM1.GetLastError(ref idwErrorCode);

                Returns.Message = "Unable to connect the device, ErrorCode=" + idwErrorCode;
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
                int idwErrorCode = 0;
                axCZKEM1.GetLastError(ref idwErrorCode);
                Returns.Message = "Unable to connect the device, ErrorCode=" + idwErrorCode;
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

            int a = -1;
            axCZKEM1.GetConnectStatus(ref a);
            return a == 0;
        }


        public async Task<bool> IsConnectedAsync()
        {
            var checkIp = await Device_PingTest.PingHostAsync(this.Device.DeviceIP);
            if (checkIp)
            {
                int A = -1;

                axCZKEM1.GetConnectStatus(ref A);

                return A == 0;
            }
            else
            {
                return checkIp;
            }
        }
        public string SN()
        {
            string DeviceSN;
            axCZKEM1.GetSerialNumber(Machine.Number, out DeviceSN);
            return DeviceSN;
        }

        public DateTime GetDateTime()
        {
            int idwYear = 0;
            int idwMonth = 0;
            int idwDay = 0;
            int idwHour = 0;
            int idwMinute = 0;
            int idwSecond = 0;

            axCZKEM1.GetDeviceTime(Machine.Number, ref idwYear, ref idwMonth, ref idwDay, ref idwHour, ref idwMinute, ref idwSecond);

            return new DateTime(idwYear, idwMonth, idwDay, idwHour, idwMinute, idwSecond);
        }

        public bool SetDateTime()
        {
            bool status = false;
            if (axCZKEM1.SetDeviceTime(Machine.Number))
            {
                axCZKEM1.RefreshData(Machine.Number);
                status = true;
            }
            return status;
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
                bool batchUpdate = axCZKEM1.BeginBatchUpdate(Machine.Number, 1);
                foreach (var user in users)
                {
                    axCZKEM1.SetStrCardNumber(user.RFID);
                    axCZKEM1.SSR_SetUserInfo(Machine.Number, user.DeviceID.ToString(), user.Name, "", 0, true);
                }

                if (batchUpdate)
                {
                    axCZKEM1.BatchUpdate(Machine.Number);
                    batchUpdate = false;
                }
            }
            catch
            { }
            finally
            {
                axCZKEM1.EnableDevice(Machine.Number, true);
            }
        }

        public List<User> Download_User()
        {
            if (!IsConnected()) return new List<User>();

            List<User> Users = new List<User>();

            string DeviceID = string.Empty;
            string name = string.Empty;
            string pwd = string.Empty;
            int pri = 0;
            bool enable = true;
            string RFID = null;

            axCZKEM1.EnableDevice(Machine.Number, false);
            try
            {
                axCZKEM1.ReadAllUserID(Machine.Number);

                while (axCZKEM1.SSR_GetAllUserInfo(Machine.Number, out DeviceID, out name, out pwd, out pri, out enable))
                {
                    axCZKEM1.GetStrCardNumber(out RFID);

                    //RFID = "";
                    //if (axCZKEM1.GetStrCardNumber(out RFID))
                    //{
                    if (string.IsNullOrEmpty(RFID) || RFID == "0")
                        RFID = null;
                    //}
                    //if (!string.IsNullOrEmpty(name))
                    //{
                    //    int index = name.IndexOf("\0");
                    //    if (index > 0)
                    //    {
                    //        name = name.Substring(0, index);
                    //    }
                    //}

                    var user = new User {DeviceID = Convert.ToInt32(DeviceID), Name = name, RFID = RFID};

                    Users.Add(user);
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
            return Users;
        }
        public List<LogView> Download_Prev_Logs()
        {
            if (!IsConnected()) return new List<LogView>();

            List<LogView> Logs = new List<LogView>();
            axCZKEM1.EnableDevice(Machine.Number, false);
            try
            {
                string sdwEnrollNumber = "";
                int idwVerifyMode = 0;
                int idwInOutMode = 0;
                int idwYear = 0;
                int idwMonth = 0;
                int idwDay = 0;
                int idwHour = 0;
                int idwMinute = 0;
                int idwSecond = 0;
                int idwWorkcode = 0;

                string fromTime = DateTime.Today.AddDays(-PrevLogCountDay).ToString("yyyy-MM-dd 00:00:00");

                var Device_last_update = new DateTime();

                if (DateTime.TryParse(Device.Last_Down_Log_Time, out Device_last_update))
                {
                    if ((DateTime.Today - Device_last_update).TotalDays < 7)
                    {
                        fromTime = Device.Last_Down_Log_Time;
                    }
                }
                string toTime = DateTime.Today.ToString("yyyy-MM-dd 00:00:00");


                if (axCZKEM1.ReadTimeGLogData(Machine.Number, fromTime, toTime))
                {
                    while (axCZKEM1.SSR_GetGeneralLogData(Machine.Number, out sdwEnrollNumber, out idwVerifyMode, out idwInOutMode, out idwYear, out idwMonth, out idwDay, out idwHour, out idwMinute, out idwSecond, ref idwWorkcode))//get records from the memory
                    {
                        var DeviceID = Convert.ToInt32(sdwEnrollNumber);
                        var dt = new DateTime(idwYear, idwMonth, idwDay, idwHour, idwMinute, idwSecond);
                        var time = new TimeSpan(idwHour, idwMinute, idwSecond);
                        var s_Date = dt.ToShortDateString();

                        var log = new LogView()
                        {
                            DeviceID = DeviceID,
                            Entry_Date = s_Date,
                            Entry_Time = time,
                            Entry_Day = dt.ToString("dddd"),
                            Entry_DateTime = dt
                        };
                        Logs.Add(log);
                    }
                }
                else
                {
                    if (axCZKEM1.ReadGeneralLogData(Machine.Number))
                    {
                        while (axCZKEM1.SSR_GetGeneralLogData(Machine.Number, out sdwEnrollNumber, out idwVerifyMode, out idwInOutMode, out idwYear, out idwMonth, out idwDay, out idwHour, out idwMinute, out idwSecond, ref idwWorkcode))//get records from the memory
                        {
                            var DeviceID = Convert.ToInt32(sdwEnrollNumber);
                            var dt = new DateTime(idwYear, idwMonth, idwDay, idwHour, idwMinute, idwSecond);
                            var time = new TimeSpan(idwHour, idwMinute, idwSecond);
                            var s_Date = dt.ToShortDateString();

                            if (dt.Date != DateTime.Today.Date)
                            {
                                var log = new LogView()
                                {
                                    DeviceID = DeviceID,
                                    Entry_Date = s_Date,
                                    Entry_Time = time,
                                    Entry_Day = dt.ToString("dddd"),
                                    Entry_DateTime = dt,

                                };
                                Logs.Add(log);
                            }
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
            return Logs.OrderBy(l => l.Entry_DateTime).ToList();
        }
        public List<LogView> Download_Today_Logs()
        {
            if (!IsConnected()) return new List<LogView>();

            List<LogView> Logs = new List<LogView>();
            axCZKEM1.EnableDevice(Machine.Number, false);
            try
            {
                string sdwEnrollNumber = "";
                int idwVerifyMode = 0;
                int idwInOutMode = 0;
                int idwYear = 0;
                int idwMonth = 0;
                int idwDay = 0;
                int idwHour = 0;
                int idwMinute = 0;
                int idwSecond = 0;
                int idwWorkcode = 0;

                string fromTime = DateTime.Today.ToString("yyyy-MM-dd 00:00:00");
                string toTime = DateTime.Today.AddDays(1).ToString("yyyy-MM-dd 00:00:00");

                if (axCZKEM1.ReadTimeGLogData(Machine.Number, fromTime, toTime))
                {
                    while (axCZKEM1.SSR_GetGeneralLogData(Machine.Number, out sdwEnrollNumber, out idwVerifyMode, out idwInOutMode, out idwYear, out idwMonth, out idwDay, out idwHour, out idwMinute, out idwSecond, ref idwWorkcode))//get records from the memory
                    {

                        var DeviceID = Convert.ToInt32(sdwEnrollNumber);
                        var dt = new DateTime(idwYear, idwMonth, idwDay, idwHour, idwMinute, idwSecond);
                        var time = new TimeSpan(idwHour, idwMinute, idwSecond);
                        var s_Date = dt.ToShortDateString();

                        var log = new LogView()
                        {
                            DeviceID = DeviceID,
                            Entry_Date = s_Date,
                            Entry_Time = time,
                            Entry_Day = dt.ToString("dddd"),
                            Entry_DateTime = dt

                        };
                        Logs.Add(log);
                    }

                }
                else
                {

                    if (axCZKEM1.ReadGeneralLogData(Machine.Number))
                    {
                        while (axCZKEM1.SSR_GetGeneralLogData(Machine.Number, out sdwEnrollNumber, out idwVerifyMode, out idwInOutMode, out idwYear, out idwMonth, out idwDay, out idwHour, out idwMinute, out idwSecond, ref idwWorkcode))//get records from the memory
                        {
                            var DeviceID = Convert.ToInt32(sdwEnrollNumber);
                            var dt = new DateTime(idwYear, idwMonth, idwDay, idwHour, idwMinute, idwSecond);
                            var time = new TimeSpan(idwHour, idwMinute, idwSecond);
                            var s_Date = dt.ToShortDateString();

                            var log = new LogView()
                            {
                                DeviceID = DeviceID,
                                Entry_Date = s_Date,
                                Entry_Time = time,
                                Entry_Day = dt.ToString("dddd"),
                                Entry_DateTime = dt

                            };
                            Logs.Add(log);
                        }

                    }

                }

            }
            catch
            {

            }
            finally
            {
                axCZKEM1.EnableDevice(Machine.Number, true);
            }
            return Logs.OrderBy(l => l.Entry_DateTime).ToList();
        }
        public bool ClearAllUsers()
        {
            if (!IsConnected()) return false;

            bool IsClear = false;
            axCZKEM1.EnableDevice(Machine.Number, false);

            if (axCZKEM1.ClearData(Machine.Number, 5))
            {
                axCZKEM1.RefreshData(Machine.Number);
                IsClear = true;
            }
            else
            {
                IsClear = false;
            }

            axCZKEM1.EnableDevice(Machine.Number, true);
            return IsClear;
        }
        public bool ClearAll_Logs()
        {
            if (!IsConnected()) return false;

            bool IsClear = false;

            axCZKEM1.EnableDevice(Machine.Number, false);

            IsClear = axCZKEM1.ClearGLog(Machine.Number);

            axCZKEM1.EnableDevice(Machine.Number, true);

            return IsClear;
        }
        public bool ClearAll_FPs()
        {
            if (!IsConnected()) return false;

            bool IsClear = false;
            axCZKEM1.EnableDevice(Machine.Number, false);

            if (axCZKEM1.ClearData(Machine.Number, 2))
            {
                axCZKEM1.RefreshData(Machine.Number);
                IsClear = true;
            }
            else
            {
                IsClear = false;
            }

            axCZKEM1.EnableDevice(Machine.Number, true);
            return IsClear;
        }
        public bool Clear_PrevLog()
        {
            bool IsClear = false;
            axCZKEM1.EnableDevice(Machine.Number, false);

            string fromTime = DateTime.Today.AddDays(-PrevLogCountDay).ToString("yyyy-MM-dd 00:00:00");

            if (axCZKEM1.DeleteAttlogByTime(Machine.Number, fromTime))
            {
                axCZKEM1.RefreshData(Machine.Number);
                IsClear = true;
            }
            else
            {
                IsClear = false;
            }

            axCZKEM1.EnableDevice(Machine.Number, true);
            return IsClear;

        }
        public List<LogView> DownloadLogs()
        {
            if (!IsConnected()) return new List<LogView>();

            List<LogView> Logs = new List<LogView>();
            axCZKEM1.EnableDevice(Machine.Number, false);
            try
            {
                string sdwEnrollNumber = "";
                int idwVerifyMode = 0;
                int idwInOutMode = 0;
                int idwYear = 0;
                int idwMonth = 0;
                int idwDay = 0;
                int idwHour = 0;
                int idwMinute = 0;
                int idwSecond = 0;
                int idwWorkcode = 0;

                if (axCZKEM1.ReadGeneralLogData(Machine.Number))
                {
                    while (axCZKEM1.SSR_GetGeneralLogData(Machine.Number, out sdwEnrollNumber, out idwVerifyMode, out idwInOutMode, out idwYear, out idwMonth, out idwDay, out idwHour, out idwMinute, out idwSecond, ref idwWorkcode))//get records from the memory
                    {
                        var DeviceID = Convert.ToInt32(sdwEnrollNumber);
                        var dt = new DateTime(idwYear, idwMonth, idwDay, idwHour, idwMinute, idwSecond);
                        var time = new TimeSpan(idwHour, idwMinute, idwSecond);
                        var s_Date = dt.ToShortDateString();

                        var log = new LogView()
                        {
                            DeviceID = DeviceID,
                            Entry_Date = s_Date,
                            Entry_Time = time,
                            Entry_Day = dt.ToString("dddd"),
                            Entry_DateTime = dt
                        };
                        Logs.Add(log);
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
            return Logs.OrderBy(l => l.Entry_DateTime).ToList();
        }
        public DeviceDetails GetDeviceDetails()
        {
            int userCapacity = 0;
            int userCount = 0;
            int fpCnt = 0;
            int fpCapacity = 0;
            int recordCnt = 0;
            int recordCapacity = 0;
            int duplicate_Punch_Time = 0;
            //axCZKEM1.EnableDevice(Machine.Number, false);//disable the device

            axCZKEM1.GetDeviceStatus(Machine.Number, (int)Data_Code.User_capacity, ref userCapacity);
            axCZKEM1.GetDeviceStatus(Machine.Number, (int)Data_Code.Number_of_users, ref userCount);
            axCZKEM1.GetDeviceStatus(Machine.Number, (int)Data_Code.Number_of_FP, ref fpCnt);
            axCZKEM1.GetDeviceStatus(Machine.Number, (int)Data_Code.FP_Capacity, ref fpCapacity);
            axCZKEM1.GetDeviceStatus(Machine.Number, (int)Data_Code.Attendance_Records, ref recordCnt);
            axCZKEM1.GetDeviceStatus(Machine.Number, (int)Data_Code.Attendance_Record_Capacity, ref recordCapacity);
            axCZKEM1.GetDeviceInfo(Machine.Number, 8, ref duplicate_Punch_Time);

            // axCZKEM1.EnableDevice(Machine.Number, true);//enable the device

            return new DeviceDetails(fpCapacity, fpCnt, userCapacity, userCount, recordCapacity, recordCnt, duplicate_Punch_Time);
        }


        public int FP_Add(string DeviceID, int FingerIndex)
        {
            if (axCZKEM1.RegEvent(Machine.Number, 65535))
            {
                axCZKEM1.OnFingerFeature += new zkemkeeper._IZKEMEvents_OnFingerFeatureEventHandler(axCZKEM1_OnFingerFeature);
                axCZKEM1.OnEnrollFingerEx += new zkemkeeper._IZKEMEvents_OnEnrollFingerExEventHandler(axCZKEM1_OnEnrollFingerEx);
            }

            int idwErrorCode = 0;
            int iFlag = 1;

            axCZKEM1.CancelOperation();

            //If the specified index of user's templates has existed ,delete it first
            axCZKEM1.SSR_DelUserTmpExt(Machine.Number, DeviceID, FingerIndex);

            if (axCZKEM1.StartEnrollEx(DeviceID, FingerIndex, iFlag))
            {
                FpMsg.Text = "Start to Enroll a new User,UserID=" + DeviceID + " FingerID=" + FingerIndex.ToString();

                if (axCZKEM1.StartIdentify())
                {
                    FpMsg.Text = "Enroll a new User,UserID" + DeviceID;
                }
                //After enrolling templates,you should let the device into the 1:N verification condition
            }
            else
            {
                axCZKEM1.GetLastError(ref idwErrorCode);
                FpMsg.Text = "*Operation failed,ErrorCode=" + idwErrorCode.ToString();
            }
            //axCZKEM1.OnFingerFeature -= new zkemkeeper._IZKEMEvents_OnFingerFeatureEventHandler(axCZKEM1_OnFingerFeature);
            // axCZKEM1.OnEnrollFingerEx -= new zkemkeeper._IZKEMEvents_OnEnrollFingerExEventHandler(axCZKEM1_OnEnrollFingerEx);
            return 1;
        }

        public int FP_Delete(string DeviceID, int FingerIndex)
        {
            int idwErrorCode = 0;

            axCZKEM1.CancelOperation();




            if (axCZKEM1.SSR_DelUserTmpExt(Machine.Number, DeviceID, FingerIndex))
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
        public int FP_Capacity { get; private set; }
        public int Number_of_FP { get; private set; }
        public int User_capacity { get; private set; }
        public int Number_of_users { get; private set; }
        public int Attendance_Record_Capacity { get; private set; }
        public int Attendance_Records { get; private set; }
        public int Duplicate_Punch_Time { get; private set; }
        public DeviceDetails(int fp_Capacity, int number_of_FP, int user_capacity, int number_of_users, int attendance_Record_Capacity, int attendance_Records, int duplicate_Punch_Time)
        {
            this.FP_Capacity = fp_Capacity;
            this.Number_of_FP = number_of_FP;
            this.User_capacity = user_capacity;
            this.Number_of_users = number_of_users;
            this.Attendance_Record_Capacity = attendance_Record_Capacity;
            this.Attendance_Records = attendance_Records;
            this.Duplicate_Punch_Time = duplicate_Punch_Time;
        }
    }
}
