using AttendanceDevice.Model;
using AttendanceDevice.ViewModel;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Media;

namespace AttendanceDevice.Config_Class
{
    class LocalData
    {
        private LocalData()
        {
            using (var db = new ModelContext())
            {
                Users = db.Users.ToList();
                institution = db.Institutions.FirstOrDefault();
                Devices = db.Devices.ToList();
                //if (Users.Count() > 0)
                //{
                //    UserViews = Users.Select(u => new UserView
                //    {
                //        DeviceID = u.DeviceID,
                //        ID = u.ID,
                //        RFID = u.RFID,
                //        Name = u.Name,
                //        Designation = u.Designation,
                //        ImgLink = institution.Image_Link + "\\" + u.ID + ".jpg",
                //        Is_Student = u.Is_Student,
                //        ScheduleID = u.ScheduleID
                //    }).ToList();
                //}
                Schedules = db.attendance_Schedule_Days.ToList();
            }
        }

        private static readonly Lazy<LocalData> lazy = new Lazy<LocalData>(() => new LocalData());
        public static LocalData Instance { get { return lazy.Value; } }
        public static Setting_Error Current_Error { get; set; } = new Setting_Error();
        public Institution institution { get; set; }
        public List<Device> Devices { get; set; } = new List<Device>();
        public List<User> Users { get; set; } = new List<User>();
        public List<UserView> UserViews
        {
            get
            {
                if (Users.Count() > 0)
                {
                    return Users.Select(u => new UserView
                    {
                        DeviceID = u.DeviceID,
                        ID = u.ID,
                        RFID = u.RFID,
                        Name = u.Name,
                        Designation = u.Designation,
                        ImgLink = institution.Image_Link + "\\" + u.ID + ".jpg",
                        Is_Student = u.Is_Student,
                        ScheduleID = u.ScheduleID
                    }).ToList();
                }
                else
                {
                    return new List<UserView>();
                }
            }
            set { UserViews = value; }
        }
        public UserView GetUserView(int deviceID)
        {
            return this.UserViews.Where(u => u.DeviceID == deviceID).FirstOrDefault();
        }

        public List<Attendance_Schedule_Day> Schedules { get; set; } = new List<Attendance_Schedule_Day>();
        public List<Attendance_Schedule_Day> Schedules_Get()
        {
            return Schedules.Select(a => new Attendance_Schedule_Day
            {
                id = a.id,
                Day = a.Day,
                Is_OnDay = a.Is_OnDay,
                ScheduleID = a.ScheduleID,
                SchoolID = a.SchoolID,
                StartTime = Convert.ToDateTime(a.StartTime).ToString("hh:mm tt"),
                EndTime = Convert.ToDateTime(a.EndTime).ToString("hh:mm tt"),
                LateEntryTime = Convert.ToDateTime(a.LateEntryTime).ToString("hh:mm tt")
            }).ToList();
        }
        public Attendance_Schedule_Day GetUserSchedule(int scheduleID)
        {
            return this.Schedules.Where(u => u.ScheduleID == scheduleID).FirstOrDefault();
        }
        public List<int> GetCurrent_Onday_SchduleIDs()
        {
            return this.Schedules.Where(s => Convert.ToDateTime(s.LateEntryTime) < DateTime.Now && s.Is_OnDay && !s.Is_Abs_Count).Select(s => s.ScheduleID).ToList();
        }

        public async Task<List<Device>> DeviceListAsync()
        {
            using (var db = new ModelContext())
            {
                return await db.Devices.ToListAsync();
            }
        }

        public async Task<List<Attendance_Record>> StudentLog_Post()
        {
            using (var db = new ModelContext())
            {
                var logs = from a in db.attendance_Records
                           join u in db.Users
                           on a.DeviceID equals u.DeviceID
                           where !a.Is_Sent && u.Is_Student
                           select a;

                return await logs.ToListAsync();
            }
        }
        public async Task<List<Attendance_Record>> StudentLog_Put()
        {
            using (var db = new ModelContext())
            {
                var logs = from a in db.attendance_Records
                           join u in db.Users
                           on a.DeviceID equals u.DeviceID
                           where !a.Is_Updated && u.Is_Student
                           select a;

                return await logs.ToListAsync();
            }
        }
        public async Task<List<Attendance_Record>> EmpLog_Post()
        {
            using (var db = new ModelContext())
            {
                var logs = from a in db.attendance_Records
                           join u in db.Users
                           on a.DeviceID equals u.DeviceID
                           where !a.Is_Sent && !u.Is_Student
                           select a;

                return await logs.ToListAsync();
            }
        }
        public async Task<List<Attendance_Record>> EmpLog_Put()
        {
            using (var db = new ModelContext())
            {
                var logs = from a in db.attendance_Records
                           join u in db.Users
                           on a.DeviceID equals u.DeviceID
                           where !a.Is_Updated && !u.Is_Student
                           select a;

                return await logs.ToListAsync();
            }
        }

        public List<Attendance_Record_View> Get_Pending_Attendance_Record()
        {
            using (var db = new ModelContext())
            {
                var logs = from a in db.attendance_Records
                           join u in db.Users
                           on a.DeviceID equals u.DeviceID
                           where !a.Is_Updated || !a.Is_Sent
                           select new Attendance_Record_View()
                           {
                               AttendanceDate = a.AttendanceDate,
                               AttendanceStatus = a.AttendanceStatus,
                               DeviceID = a.DeviceID,
                               EntryTime = a.EntryTime,
                               ExitStatus = a.ExitStatus,
                               ExitTime = a.ExitTime,
                               ID = u.ID,
                               Name = u.Name,
                               Is_Student = u.Is_Student
                           };

                return logs.ToList();
            }
        }

        public List<Log_Backups_View> Get_Log_Backup()
        {
            using (var db = new ModelContext())
            {
                var logs = from a in db.attendanceLog_Backups
                           join u in db.Users
                           on a.DeviceID equals u.DeviceID
                           select new Log_Backups_View()
                           {
                               DeviceID = a.DeviceID,
                               Entry_Date = a.Entry_Date,
                               Entry_Time = a.Entry_Time,
                               Backup_Reason = a.Backup_Reason,
                               ID = u.ID,
                               Name = u.Name,
                               Is_Student = u.Is_Student
                           };

                return logs.Distinct().ToList();
            }
        }
        public List<LeaveView> Get_Leave()
        {
            using (var db = new ModelContext())
            {
                var logs = from l in db.user_Leave_Records
                           join u in db.Users
                           on l.DeviceID equals u.DeviceID
                           select new LeaveView()
                           {
                               ID = u.ID,
                               Name = u.Name,
                               LeaveDate = l.LeaveDate
                           };

                return logs.ToList();
            }
        }

        public List<UserFP_View> Get_AllUserFP()
        {
            using (var db = new ModelContext())
            {
                var userFp = from f in db.user_FingerPrints
                             join u in db.Users
                             on f.DeviceID equals u.DeviceID
                             group u by u into g
                             select new UserFP_View()
                             {
                                 DeviceID = g.Key.DeviceID,
                                 ID = g.Key.ID,
                                 Name = g.Key.Name,
                                 Designation = g.Key.Designation,
                                 ImgLink = institution.Image_Link + "\\" + g.Key.ID + ".jpg",
                                 Is_Student = g.Key.Is_Student,
                                 FingerCount = g.Count()
                             };

                return userFp.ToList();
            }
        }

        public Finger Get_UserFP(int DeviceID)
        {
            var fingers = new Finger();

            using (var db = new ModelContext())
            {
                var Finger_Indexs = db.user_FingerPrints.Where(f => f.DeviceID == DeviceID).Select(f => f.Finger_Index).ToList();

                foreach (var item in Finger_Indexs)
                {
                    if (item == 3)
                    {
                        fingers.LeftIndex = Brushes.GreenYellow;
                    }
                    else if (item == 4)
                    {
                        fingers.LeftThamb = Brushes.GreenYellow;
                    }
                    else if (item == 5)
                    {
                        fingers.RightIndex = Brushes.GreenYellow;
                    }
                    else if (item == 6)
                    {
                        fingers.RightThamb = Brushes.GreenYellow;
                    }
                }




            }
            return fingers;
        }
        public void Delete_UserFP(int DeviceID, int Index)
        {
            using (var db = new ModelContext())
            {
                var finger = db.user_FingerPrints.Where(f => f.DeviceID == DeviceID && f.Finger_Index == Index).ToList();
                db.user_FingerPrints.RemoveRange(finger);
                db.SaveChanges();
            }
        }
        public List<ErrorData_View> GetErrors()
        {
            using (var db = new ModelContext())
            {
                var Errors = from d in db.dataUpdateLists
                             select new ErrorData_View()
                             {
                                 id = d.DateUpdateID,
                                 ErrorType = d.UpdateType,
                                 ErrorDescription = d.UpdateDescription,
                                 ErrorDate = d.UpdateDate
                             };

                return Errors.OrderByDescending(a => a.id).ToList();
            }
        }
        public void DeleteErrors()
        {
            using (var db = new ModelContext())
            {
                db.dataUpdateLists.Clear();
                db.SaveChanges();
            }
        }
        public void Delete_Log_Backup(DateTime fdate, DateTime tdate, List<int> DeviceIDs)
        {
            using (var db = new ModelContext())
            {
                var logs = db.attendanceLog_Backups.ToList().Where(a => DeviceIDs.Contains(a.DeviceID) && Convert.ToDateTime(a.Entry_Date) >= fdate && Convert.ToDateTime(a.Entry_Date) <= tdate);

                db.attendanceLog_Backups.RemoveRange(logs);
                db.SaveChanges();
            }
        }
        public void Abs_Insert(List<int> ScheduleIDs, string date, Institution Ins)
        {
            List<Attendance_Record> Records = new List<Attendance_Record>();

            var ScheduleUser = new List<int>();

            if (Ins.Is_Employee_Attendance_Enable && Ins.Is_Student_Attendance_Enable)
                ScheduleUser = Users.Where(u => ScheduleIDs.Contains(u.ScheduleID)).Select(u => u.DeviceID).ToList();
            else if (Ins.Is_Employee_Attendance_Enable)
                ScheduleUser = Users.Where(u => ScheduleIDs.Contains(u.ScheduleID) && !u.Is_Student).Select(u => u.DeviceID).ToList();
            else if (Ins.Is_Student_Attendance_Enable)
                ScheduleUser = Users.Where(u => ScheduleIDs.Contains(u.ScheduleID) && u.Is_Student).Select(u => u.DeviceID).ToList();

            using (var db = new ModelContext())
            {
                var logs = db.attendance_Records.Where(a => a.AttendanceDate == date).Select(a => a.DeviceID).ToList();

                var DeviceIDs = ScheduleUser.Where(u => !logs.Contains(u)).ToList();


                foreach (var DeviceID in DeviceIDs)
                {
                    var Att_record = new Attendance_Record();

                    Att_record.AttendanceDate = date;
                    Att_record.DeviceID = DeviceID;
                    Att_record.AttendanceStatus = "Abs";
                    Records.Add(Att_record);
                }


                //Schedules updates
                var Schs = db.attendance_Schedule_Days.Where(s => ScheduleIDs.Contains(s.ScheduleID)).ToList();
                Schs.ForEach(s => s.Is_Abs_Count = true);

                if (Records.Count() > 0)
                {
                    db.attendance_Records.AddRange(Records);
                }
                db.SaveChanges();

                this.Schedules = db.attendance_Schedule_Days.ToList();
            }
        }

        public bool IsUserExist()
        {
            using (var db = new ModelContext())
            {
                return db.Users.Any();
            }
        }

        public bool IsDeviceExist()
        {
            using (var db = new ModelContext())
            {
                return db.Devices.Any();
            }
        }
    }

    public enum Error_Type
    {
        NoError,
        DeviceInfoPage,
        UserInfoPage,
    }
    public class Setting_Error
    {
        public Error_Type Type { get; set; }
        public string Message { get; set; }
    }

    public class Finger
    {
        public SolidColorBrush LeftIndex { get; set; } = Brushes.White;
        public SolidColorBrush LeftThamb { get; set; } = Brushes.White;
        public SolidColorBrush RightIndex { get; set; } = Brushes.White;
        public SolidColorBrush RightThamb { get; set; } = Brushes.White;

    }
}
