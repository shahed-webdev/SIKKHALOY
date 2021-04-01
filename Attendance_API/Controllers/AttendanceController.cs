using Attendance_API.DB_Model;
using Attendance_API.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web.Http;

namespace Attendance_API.Controllers
{
    [Authorize]
    public class AttendanceController : ApiController
    {
        private readonly TimeSpan _clearTime = new TimeSpan(0, 0, 0);

        [Route("api/Attendance/{id}/Students")]
        [HttpPost]
        public IHttpActionResult PostStudents(int id, [FromBody]List<AttendanceRecordAPI> logRecord)
        {
            if (logRecord == null) return NotFound();
            if (logRecord.Count < 1) return NotFound();

            using (var db = new EduContext())
            {
                var today = DateTime.Now;
                var timeToday = DateTime.Now;

                var attSetting = db.Attendance_Device_Settings.FirstOrDefault(s => s.SchoolID == id);
                var schoolName = db.SchoolInfos.Find(id)?.SchoolName;

                if (attSetting != null && attSetting.Is_Device_Attendance_Enable && attSetting.Is_Student_Attendance_Enable)
                {
                    var attendanceRecords = db.VW_Attendance_Stus.Where(a => a.SchoolID == id).ToList();

                    var newRecord = from s in attendanceRecords
                                    join a in logRecord
                                    on s.DeviceID equals a.DeviceID
                                    where s.SchoolID == id
                                    select new Attendance_Record
                                    {
                                        SchoolID = s.SchoolID,
                                        ClassID = s.ClassID,
                                        EducationYearID = s.EducationYearID,
                                        StudentID = s.StudentID,
                                        StudentClassID = s.StudentClassID,
                                        Attendance = a.AttendanceStatus,
                                        AttendanceDate = a.AttendanceDate,
                                        EntryTime = a.EntryTime == _clearTime ? (TimeSpan?)null : a.EntryTime,
                                        ExitStatus = a.ExitStatus,
                                        ExitTime = a.ExitTime == _clearTime ? (TimeSpan?)null : a.ExitTime,
                                        Is_OUT = a.Is_OUT
                                    };

                    var newAttendanceRecords = newRecord.Where(na => !db.Attendance_Records.Any(sa => na.SchoolID == sa.SchoolID & na.StudentID == sa.StudentID & na.AttendanceDate == sa.AttendanceDate)).ToList();

                    var smsList = new List<Attendance_SMS>();

                    if (newAttendanceRecords.Count > 0)
                    {
                        if (attSetting.Is_All_SMS_On && attSetting.Is_Student_All_SMS_Active)
                        {
                            ////------Abs & LateAbs SMS
                            var stuAbsSetting = db.VW_Attendance_Stu_Settings.Where(a => a.SchoolID == id && a.Is_Abs_SMS).ToList();
                            if (stuAbsSetting.Count > 0 && attSetting.Is_Student_Abs_SMS_ON)
                            {
                                var attRecordAbs = newAttendanceRecords.Where(a => a.Attendance == "Abs").ToList();
                                if (attRecordAbs.Count > 0)
                                {
                                    var stuList = from a in attRecordAbs
                                                   join s in stuAbsSetting
                                                    on a.StudentID equals s.StudentID
                                                   where s.Is_Abs_SMS && a.AttendanceDate.Date == today.Date
                                                   select new Attendance_SMS
                                                   {
                                                       SchoolID = id,
                                                       StudentID = a.StudentID,
                                                       ScheduleTime = s.StartTime,
                                                       AttendanceDate = a.AttendanceDate,
                                                       SMS_Text = attSetting.Is_English_SMS ? $"Respected guardian, {s.StudentsName} today({a.AttendanceDate:d MMM yy}) absent, please send to class regularly. {schoolName}"
                                                           : $"সম্মানিত অভিভাবক, {s.StudentsName} আজ({a.AttendanceDate:d MMM yy}) অনুপস্থিত, অনুগ্রহ করে নিয়মিত ক্লাসে পাঠান {schoolName}",
                                                       MobileNo = s.SMSPhoneNo,
                                                       AttendanceStatus = a.Attendance,
                                                       SMS_TimeOut = attSetting.SMS_TimeOut_Minute
                                                   };

                                    smsList.AddRange(stuList);
                                }

                                var attRecordLatAbs = newAttendanceRecords.Where(a => a.Attendance == "Late Abs").ToList();
                                if (attRecordLatAbs.Count > 0)
                                {
                                    var stuList = from a in attRecordLatAbs
                                                   join s in stuAbsSetting
                                                    on a.StudentID equals s.StudentID
                                                   where s.Is_Abs_SMS && a.AttendanceDate.Date == today.Date
                                                   select new Attendance_SMS
                                                   {
                                                       SchoolID = id,
                                                       StudentID = a.StudentID,
                                                       ScheduleTime = s.StartTime,
                                                       AttendanceDate = a.AttendanceDate,
                                                       SMS_Text = attSetting.Is_English_SMS ? $"Respected guardian, {s.StudentsName} today({a.AttendanceDate:d MMM yy}) late absent. entry time {timeToday.Add(a.EntryTime.GetValueOrDefault()):h:mm tt}. {schoolName}"
                                                           : $"সম্মানিত অভিভাবক, {s.StudentsName} আজ({a.AttendanceDate:d MMM yy}) বিলম্বে (অনুপস্থিত হিসাবে), প্রবেশ করেছে। প্রবেশ সময় {timeToday.Add(a.EntryTime.GetValueOrDefault()):h:mm tt}. {schoolName}",
                                                       MobileNo = s.SMSPhoneNo,
                                                       AttendanceStatus = a.Attendance,
                                                       SMS_TimeOut = attSetting.SMS_TimeOut_Minute
                                                   };

                                    smsList.AddRange(stuList);
                                }
                            }

                            //------Entry SMS
                            var stuEntrySetting = db.VW_Attendance_Stu_Settings.Where(a => a.SchoolID == id && a.Entry_Confirmation).ToList();
                            if (stuEntrySetting.Count > 0 && attSetting.Is_Student_Entry_SMS_ON)
                            {
                                var attRecordPre = newAttendanceRecords.Where(a => a.Attendance == "Pre").ToList();
                                if (attRecordPre.Count > 0)
                                {
                                    var stuList = from a in attRecordPre
                                                   join s in stuEntrySetting
                                                   on a.StudentID equals s.StudentID
                                                   where s.Entry_Confirmation && a.AttendanceDate.Date == today.Date
                                                   select new Attendance_SMS
                                                   {
                                                       SchoolID = id,
                                                       StudentID = a.StudentID,
                                                       ScheduleTime = s.StartTime,
                                                       AttendanceDate = a.AttendanceDate,
                                                       SMS_Text = attSetting.Is_English_SMS ? $"Respected guardian, {s.StudentsName} has reached {schoolName} at {DateTime.Today.Add(a.EntryTime.GetValueOrDefault()):hh:mm tt}"
                                                           : $"সম্মানিত অভিভাবক, {s.StudentsName} নিরাপদে {schoolName} এ ({DateTime.Today.Add(a.EntryTime.GetValueOrDefault()):hh:mm tt}) প্রবেশ করেছে",
                                                       MobileNo = s.SMSPhoneNo,
                                                       AttendanceStatus = a.Attendance,
                                                       SMS_TimeOut = attSetting.SMS_TimeOut_Minute
                                                   };

                                    smsList.AddRange(stuList);
                                }
                            }

                            ////------Late SMS
                            var stuLateSetting = db.VW_Attendance_Stu_Settings.Where(a => a.SchoolID == id && a.Is_Late_SMS).ToList();
                            if (stuLateSetting.Count > 0 && attSetting.Is_Student_Late_SMS_ON)
                            {
                                var attRecordLate = newAttendanceRecords.Where(a => a.Attendance == "Late").ToList();
                                if (attRecordLate.Count > 0)
                                {
                                    var stuList = from a in attRecordLate
                                                   join s in stuLateSetting
                                                    on a.StudentID equals s.StudentID
                                                   where s.Is_Late_SMS && a.AttendanceDate.Date == today.Date
                                                   select new Attendance_SMS
                                                   {
                                                       SchoolID = id,
                                                       StudentID = a.StudentID,
                                                       ScheduleTime = s.StartTime,
                                                       AttendanceDate = a.AttendanceDate,
                                                       SMS_Text = attSetting.Is_English_SMS ? $"Respected guardian, {s.StudentsName} today({a.AttendanceDate:d MMM yy}) late {(a.EntryTime.GetValueOrDefault() - s.StartTime).Minutes} min, entry time {timeToday.Add(a.EntryTime.GetValueOrDefault()):h:mm tt}. {schoolName}"
                                                           : $"সম্মানিত অভিভাবক, {s.StudentsName} আজ({a.AttendanceDate:d MMM yy}) {(a.EntryTime.GetValueOrDefault() - s.StartTime).Minutes} মি: বিলম্বে, প্রবেশ করেছে। প্রবেশ সময় {timeToday.Add(a.EntryTime.GetValueOrDefault()):h:mm tt}. {schoolName}",
                                                       MobileNo = s.SMSPhoneNo,
                                                       AttendanceStatus = a.Attendance,
                                                       SMS_TimeOut = attSetting.SMS_TimeOut_Minute
                                                   };

                                    smsList.AddRange(stuList);
                                }
                            }

                            ////------Exit SMS
                            var stuExitSetting = db.VW_Attendance_Stu_Settings.Where(a => a.SchoolID == id && a.Exit_Confirmation).ToList();
                            if (stuExitSetting.Count > 0 && attSetting.Is_Student_Exit_SMS_ON)
                            {
                                var attRecordExit = newAttendanceRecords.Where(a => a.ExitStatus == "Exit").ToList();
                                if (attRecordExit.Count > 0)
                                {
                                    var stuList = from a in attRecordExit
                                                   join s in stuExitSetting
                                                       on a.StudentID equals s.StudentID
                                                   where s.Exit_Confirmation && a.AttendanceDate.Date == today.Date
                                                   select new Attendance_SMS
                                                   {
                                                       SchoolID = id,
                                                       StudentID = a.StudentID,
                                                       ScheduleTime = s.StartTime,
                                                       AttendanceDate = a.AttendanceDate,
                                                       SMS_Text = attSetting.Is_English_SMS ? $"Respected guardian, {s.StudentsName} has exited from {schoolName} at {timeToday.Add(a.ExitTime.GetValueOrDefault()):h:mm tt}"
                                                           : $"সম্মানিত অভিভাবক, {s.StudentsName}, {schoolName} থেকে {timeToday.Add(a.ExitTime.GetValueOrDefault()):h:mm tt} প্রস্থান করেছে",
                                                       MobileNo = s.SMSPhoneNo,
                                                       AttendanceStatus = a.Attendance,
                                                       SMS_TimeOut = attSetting.SMS_TimeOut_Minute
                                                   };
                                    smsList.AddRange(stuList);
                                }
                            }
                        }

                        if (smsList.Count > 0) db.Attendance_sms.AddRange(smsList);
                        db.Attendance_Records.AddRange(newAttendanceRecords);
                        db.SaveChanges();
                    }
                }
            }

            return Ok();
        }

        [Route("api/Attendance/{id}/StudentsUpdate")]
        [HttpPost]
        public IHttpActionResult PutStudents(int id, [FromBody]List<AttendanceRecordAPI> logRecord)
        {
            if (logRecord == null) return NotFound();
            if (logRecord.Count < 1) return NotFound();

            var smsList = new List<Attendance_SMS>();
            using (var db = new EduContext())
            {
                var today = DateTime.Now;
                var timeToday = DateTime.Now;

                var attSetting = db.Attendance_Device_Settings.FirstOrDefault(s => s.SchoolID == id);
                var schoolName = db.SchoolInfos.Find(id)?.SchoolName;

                var logForSMS = new List<Attendance_Record>();
                if (attSetting != null && attSetting.Is_Device_Attendance_Enable && attSetting.Is_Student_Attendance_Enable)
                {
                    var studentList = db.VW_Attendance_Stus.Where(a => a.SchoolID == id).ToList();

                    var updatedRecords = from s in studentList
                                         join a in logRecord
                                         on s.DeviceID equals a.DeviceID
                                         where s.SchoolID == id
                                         select new Attendance_Record
                                         {
                                             SchoolID = s.SchoolID,
                                             ClassID = s.ClassID,
                                             EducationYearID = s.EducationYearID,
                                             StudentID = s.StudentID,
                                             StudentClassID = s.StudentClassID,
                                             Attendance = a.AttendanceStatus,
                                             AttendanceDate = a.AttendanceDate,
                                             EntryTime = a.EntryTime == _clearTime ? (TimeSpan?)null : a.EntryTime,
                                             ExitStatus = a.ExitStatus,
                                             ExitTime = a.ExitTime == _clearTime ? (TimeSpan?)null : a.ExitTime,
                                             Is_OUT = a.Is_OUT
                                         };

                    foreach (var log in updatedRecords)
                    {
                        var updateLog = db.Attendance_Records.FirstOrDefault(u => u.SchoolID == log.SchoolID & u.StudentID == log.StudentID & u.AttendanceDate == log.AttendanceDate);

                        if (updateLog == null) continue;

                        updateLog.Attendance = log.Attendance;
                        updateLog.ExitStatus = log.ExitStatus;

                        updateLog.ExitTime = log.ExitTime;
                        updateLog.Is_OUT = log.Is_OUT;
                        updateLog.EntryTime = log.EntryTime;

                        db.Entry(updateLog).State = EntityState.Modified;
                        db.SaveChanges();
                        logForSMS.Add(updateLog);
                    }
                }


                if (logForSMS.Count > 0)
                {
                    if (attSetting != null && attSetting.Is_All_SMS_On && attSetting.Is_Student_All_SMS_Active)
                    {
                        ////------LateAbs SMS
                        if (attSetting.Is_Student_Abs_SMS_ON)
                        {
                            var attRecordLatAbs = logForSMS.Where(a => a.Attendance == "Late Abs").ToList();

                            if (attRecordLatAbs.Count > 0)
                            {
                                foreach (var log in attRecordLatAbs)
                                {
                                    var ifSMSCreated = db.Attendance_sms.FirstOrDefault(a => a.SchoolID == id && a.StudentID == log.StudentID && a.AttendanceDate == log.AttendanceDate && a.AttendanceStatus == "Late Abs");
                                    var stu = db.VW_Attendance_Stu_Settings.FirstOrDefault(s => s.SchoolID == id && s.Is_Abs_SMS && s.StudentID == log.StudentID);

                                    if (ifSMSCreated == null || log.AttendanceDate.Date != today.Date || stu == null) continue;

                                    var stuList = new Attendance_SMS
                                    {
                                        SchoolID = id,
                                        StudentID = log.StudentID,
                                        ScheduleTime = stu.StartTime,
                                        AttendanceDate = log.AttendanceDate,
                                        SMS_Text = attSetting.Is_English_SMS ? $"Respected guardian, {stu.StudentsName} today({log.AttendanceDate:d MMM yy}) late absent. Entry time {timeToday.Add(log.EntryTime.GetValueOrDefault()):h:mm tt}. {schoolName}"
                                            : $"সম্মানিত অভিভাবক, {stu.StudentsName} আজ({log.AttendanceDate:d MMM yy}) বিলম্বে (অনুপস্থিত হিসাবে), প্রবেশ করেছে। প্রবেশ সময় {timeToday.Add(log.EntryTime.GetValueOrDefault()):h:mm tt}. {schoolName}",
                                        MobileNo = stu.SMSPhoneNo,
                                        AttendanceStatus = log.Attendance,
                                        SMS_TimeOut = attSetting.SMS_TimeOut_Minute
                                    };

                                    smsList.Add(stuList);
                                    db.SaveChanges();
                                }
                            }
                        }


                        ////------Exit SMS
                        var stuExitSetting = db.VW_Attendance_Stu_Settings.Where(a => a.SchoolID == id && a.Exit_Confirmation).ToList();
                        if (attSetting.Is_Student_Exit_SMS_ON)
                        {
                            var attRecordExit = logForSMS.Where(a => a.Attendance == "Exit").ToList();
                            if (attRecordExit.Count > 0)
                            {
                                foreach (var log in attRecordExit)
                                {
                                    var ifSMSCreated = db.Attendance_sms.FirstOrDefault(a => a.SchoolID == id && a.StudentID == log.StudentID && a.AttendanceDate == log.AttendanceDate && a.AttendanceStatus == "Exit");
                                    var stu = db.VW_Attendance_Stu_Settings.FirstOrDefault(s => s.SchoolID == id && s.Exit_Confirmation && s.StudentID == log.StudentID);

                                    if (ifSMSCreated == null || log.AttendanceDate.Date != today.Date || stu == null) continue;
                                    
                                    var stuList = new Attendance_SMS
                                    {
                                        SchoolID = id,
                                        StudentID = log.StudentID,
                                        ScheduleTime = stu.StartTime,
                                        AttendanceDate = log.AttendanceDate,
                                        SMS_Text = attSetting.Is_English_SMS ? $"Respected guardian, {stu.StudentsName} has exited from {schoolName} at {timeToday.Add(log.ExitTime.GetValueOrDefault()):h:mm tt}"
                                            : $"সম্মানিত অভিভাবক, {stu.StudentsName}, {schoolName} থেকে {timeToday.Add(log.ExitTime.GetValueOrDefault()):h:mm tt} প্রস্থান করেছে",
                                        MobileNo = stu.SMSPhoneNo,
                                        AttendanceStatus = log.Attendance,
                                        SMS_TimeOut = attSetting.SMS_TimeOut_Minute
                                    };
                                    smsList.Add(stuList);
                                    db.SaveChanges();
                                }
                            }
                        }
                    }
                }
            }

            return Ok();
        }

        [Route("api/Attendance/{id}/Employees")]
        [HttpPost]
        public IHttpActionResult PostEmployees(int id, [FromBody]List<AttendanceRecordAPI> logRecord)
        {
            if (logRecord == null) return NotFound();
            if (logRecord.Count < 1) return NotFound();

            using (var db = new EduContext())
            {
                var today = DateTime.Now;
                var timeToday = DateTime.Now;

                var attSetting = db.Attendance_Device_Settings.FirstOrDefault(s => s.SchoolID == id);
                var schoolName = db.SchoolInfos.Find(id)?.SchoolName;

                if (attSetting != null && attSetting.Is_Device_Attendance_Enable && attSetting.Is_Employee_Attendance_Enable)
                {
                    var empList = db.VW_Emp_Infos.Where(a => a.SchoolID == id).ToList();

                    var newRecord = from e in empList
                                    join a in logRecord
                                    on e.DeviceID equals a.DeviceID
                                    where e.SchoolID == id
                                    select new Employee_Attendance_Record
                                    {
                                        SchoolID = e.SchoolID,
                                        EmployeeID = e.EmployeeID,
                                        RegistrationID = 0,
                                        AttendanceStatus = a.AttendanceStatus,
                                        AttendanceDate = a.AttendanceDate,
                                        EntryTime = a.EntryTime == _clearTime ? (TimeSpan?)null : a.EntryTime,
                                        ExitStatus = a.ExitStatus,
                                        ExitTime = a.ExitTime == _clearTime ? (TimeSpan?)null : a.ExitTime,
                                        Is_OUT = a.Is_OUT
                                    };

                    var newAttendanceRecord = newRecord.Where(na => !db.Employee_Attendance_Records.Any(sa => na.SchoolID == sa.SchoolID & na.EmployeeID == sa.EmployeeID & na.AttendanceDate == sa.AttendanceDate)).ToList();

                    var smsList = new List<Attendance_SMS>();

                    if (newAttendanceRecord.Count > 0)
                    {
                        if (attSetting.Is_All_SMS_On && attSetting.Is_Employee_SMS_Active)
                        {
                            ////------Abs & LateAbs SMS
                            var empAbsSetting = db.VW_Attendance_Emp_Settings.Where(a => a.SchoolID == id && a.Is_Abs_SMS).ToList();
                            if (empAbsSetting.Count > 0 && attSetting.Is_Employee_SMS_Active)
                            {
                                var attRecordAbs = newAttendanceRecord.Where(a => a.AttendanceStatus == "Abs").ToList();
                                if (attRecordAbs.Count > 0)
                                {
                                    var Emp_List = from a in attRecordAbs
                                                   join e in empAbsSetting
                                                   on a.EmployeeID equals e.EmployeeID
                                                   where e.Is_Abs_SMS && a.AttendanceDate.Date == today.Date
                                                   select new Attendance_SMS
                                                   {
                                                       SchoolID = id,
                                                       EmployeeID = a.EmployeeID,
                                                       ScheduleTime = e.StartTime,
                                                       AttendanceDate = a.AttendanceDate,
                                                       SMS_Text = attSetting.Is_English_SMS ? $"{e.Name} Today({a.AttendanceDate:d MMM yy}) absent"
                                                           : $"{e.Name} আজ({a.AttendanceDate:d MMM yy}) অনুপস্থিত",
                                                       MobileNo = attSetting.Is_Employee_SMS_OwnNumber ? e.Phone : attSetting.Employee_SMS_Number,
                                                       AttendanceStatus = a.AttendanceStatus,
                                                       SMS_TimeOut = attSetting.SMS_TimeOut_Minute
                                                   };

                                    smsList.AddRange(Emp_List);
                                }

                                var attRecordLatAbs = newAttendanceRecord.Where(a => a.AttendanceStatus == "Late Abs").ToList();
                                if (attRecordLatAbs.Count > 0)
                                {
                                    var stuList = from a in attRecordLatAbs
                                                   join e in empAbsSetting
                                                    on a.EmployeeID equals e.EmployeeID
                                                   where e.Is_Abs_SMS && a.AttendanceDate.Date == today.Date
                                                   select new Attendance_SMS
                                                   {
                                                       SchoolID = id,
                                                       EmployeeID = a.EmployeeID,
                                                       ScheduleTime = e.StartTime,
                                                       AttendanceDate = a.AttendanceDate,
                                                       SMS_Text = attSetting.Is_English_SMS ? $"{e.Name} Today({a.AttendanceDate:d MMM yy}) late absent. Entry time {timeToday.Add(a.EntryTime.GetValueOrDefault()):h:mm tt}"
                                                           : $"{e.Name} আজ({a.AttendanceDate:d MMM yy}) বিলম্বে (অনুপস্থিত হিসাবে), প্রবেশ করেছে। প্রবেশ সময় {timeToday.Add(a.EntryTime.GetValueOrDefault()):h:mm tt}",
                                                       MobileNo = attSetting.Is_Employee_SMS_OwnNumber ? e.Phone : attSetting.Employee_SMS_Number,
                                                       AttendanceStatus = a.AttendanceStatus,
                                                       SMS_TimeOut = attSetting.SMS_TimeOut_Minute
                                                   };

                                    smsList.AddRange(stuList);
                                }
                            }


                            ////------Late SMS
                            var empLateSetting = db.VW_Attendance_Emp_Settings.Where(a => a.SchoolID == id && a.Is_Late_SMS).ToList();
                            if (empLateSetting.Count > 0 && attSetting.Is_Employee_Late_SMS_ON)
                            {
                                var attRecordLate = newAttendanceRecord.Where(a => a.AttendanceStatus == "Late").ToList();
                                if (attRecordLate.Count > 0)
                                {
                                    var stuList = from a in attRecordLate
                                                   join e in empLateSetting
                                                    on a.EmployeeID equals e.EmployeeID
                                                   where e.Is_Late_SMS && a.AttendanceDate.Date == today.Date
                                                   select new Attendance_SMS
                                                   {
                                                       SchoolID = id,
                                                       EmployeeID = a.EmployeeID,
                                                       ScheduleTime = e.StartTime,
                                                       AttendanceDate = a.AttendanceDate,
                                                       SMS_Text = attSetting.Is_English_SMS ? $"{e.Name} Today({a.AttendanceDate:d MMM yy}) late {(a.EntryTime.GetValueOrDefault() - e.StartTime).Minutes} min. Entry time {timeToday.Add(a.EntryTime.GetValueOrDefault()):h:mm tt}"
                                                           : $"{e.Name} আজ({a.AttendanceDate:d MMM yy}) {(a.EntryTime.GetValueOrDefault() - e.StartTime).Minutes} মি: বিলম্বে প্রবেশ করেছে। প্রবেশ সময় {timeToday.Add(a.EntryTime.GetValueOrDefault()):h:mm tt}",
                                                       MobileNo = attSetting.Is_Employee_SMS_OwnNumber ? e.Phone : attSetting.Employee_SMS_Number,
                                                       AttendanceStatus = a.AttendanceStatus,
                                                       SMS_TimeOut = attSetting.SMS_TimeOut_Minute
                                                   };

                                    smsList.AddRange(stuList);
                                }
                            }
                        }

                        if (smsList.Count > 0) db.Attendance_sms.AddRange(smsList);
                        db.Employee_Attendance_Records.AddRange(newAttendanceRecord);
                        db.SaveChanges();
                    }
                }
            }

            return Ok();
        }

        [Route("api/Attendance/{id}/EmployeesUpdate")]
        [HttpPost]
        public IHttpActionResult PutEmployees(int id, [FromBody]List<AttendanceRecordAPI> logRecord)
        {
            if (logRecord == null) return NotFound();
            if (logRecord.Count < 1) return NotFound();

            using (var db = new EduContext())
            {
                var today = DateTime.Now;
                var timeToday = DateTime.Now;

                var attSetting = db.Attendance_Device_Settings.FirstOrDefault(s => s.SchoolID == id);
                var schoolName = db.SchoolInfos.Find(id)?.SchoolName;

                if (attSetting != null && attSetting.Is_Device_Attendance_Enable && attSetting.Is_Employee_Attendance_Enable)
                {
                    var empList = db.VW_Emp_Infos.Where(a => a.SchoolID == id).ToList();

                    var updatedRecords = from e in empList
                                         join a in logRecord
                                         on e.DeviceID equals a.DeviceID
                                         where e.SchoolID == id
                                         select new Employee_Attendance_Record
                                         {
                                             SchoolID = e.SchoolID,
                                             EmployeeID = e.EmployeeID,
                                             RegistrationID = 0,
                                             AttendanceStatus = a.AttendanceStatus,
                                             AttendanceDate = a.AttendanceDate,
                                             EntryTime = a.EntryTime == _clearTime ? (TimeSpan?)null : a.EntryTime,
                                             ExitStatus = a.ExitStatus,
                                             ExitTime = a.ExitTime == _clearTime ? (TimeSpan?)null : a.ExitTime,
                                             Is_OUT = a.Is_OUT
                                         };

                    foreach (var log in updatedRecords)
                    {
                        var updateLog = db.Employee_Attendance_Records.FirstOrDefault(u => u.SchoolID == log.SchoolID & u.EmployeeID == log.EmployeeID & u.AttendanceDate == log.AttendanceDate);
                        if (updateLog == null) continue;

                        updateLog.AttendanceStatus = log.AttendanceStatus;
                        updateLog.ExitStatus = log.ExitStatus;

                        updateLog.ExitTime = log.ExitTime;
                        updateLog.Is_OUT = log.Is_OUT;
                        updateLog.EntryTime = log.EntryTime;

                        db.Entry(updateLog).State = EntityState.Modified;
                        db.SaveChanges();
                    }
                }
            }

            return Ok();
        }


        [Route("api/Attendance/{id}/backup_data")]
        [HttpPost]
        public IHttpActionResult PostBackupData(int id, [FromBody]List<BackupDataAPI> backupData)
        {
            if (backupData == null) return NotFound();
            if (backupData.Count < 1) return NotFound();

            //using (var db = new EduContext())
            //{
            //    var institution = db.Attendance_Device_Settings.Where(a => a.SchoolID == id).FirstOrDefault();


            //    foreach (var data in BackupData)
            //    {
            //        var DeviceID = data.DeviceID;
            //        var dt = Convert.ToDateTime(data.AttendanceDate);
            //        var time = new TimeSpan(Hour, Minute, Second);
            //        var userView = LocalData.Instance.GetUserView(DeviceID);

            //        if (userView == null)
            //        {
            //            userView = new UserView();
            //            userView.Name = "User Not found on PC";
            //            EnrollUser_Card.DataContext = userView;
            //            return;
            //        }

            //        userView.Enroll_Time = dt;
            //        var s_Date = dt.ToShortDateString();



            //        var Is_stu_Disable = userView.Is_Student && !institution.Is_Student_Attendance_Enable;
            //        var Is_Emp_Disable = !userView.Is_Student && !institution.Is_Employee_Attendance_Enable;
            //        var Schedule = LocalData.Instance.GetUserSchedule(userView.ScheduleID);

            //        var Reason = "";
            //        // Device Attendance Disable
            //        if (!institution.Is_Device_Attendance_Enable)
            //        {
            //            Reason = "Device Attendance Disable";
            //        }
            //        // Student Attendance Disable
            //        else if (Is_stu_Disable)
            //        {
            //            Reason = "Student Attendance Disable";
            //        }
            //        // Employee Attendance Disable
            //        else if (Is_Emp_Disable)
            //        {
            //            Reason = "Employee Attendance Disable";
            //        }
            //        // Today Check
            //        else if (s_Date != DateTime.Today.ToShortDateString())
            //        {
            //            Reason = "Not Current Data";
            //        }
            //        //Hodiday attendance disable
            //        else if (institution.Is_Today_Holiday && !institution.Holiday_NotActive)
            //        {
            //            Reason = "Hodiday attendance disable";
            //        }
            //        //Schedule Off day
            //        else if (!Schedule.Is_OnDay)
            //        {
            //            Reason = "Schedule Off Day";
            //        }
            //        // Insert or Update Attendance Records
            //        else
            //        {

            //            var Att_record = db.attendance_Records.Where(a => a.DeviceID == DeviceID && a.AttendanceDate == s_Date).FirstOrDefault();
            //            var S_startTime = TimeSpan.Parse(Schedule.StartTime);
            //            var S_LateTime = TimeSpan.Parse(Schedule.LateEntryTime);
            //            var S_EndTime = TimeSpan.Parse(Schedule.EndTime);

            //            if (Att_record == null)
            //            {
            //                Att_record = new Attendance_Record();

            //                if (time > S_EndTime)
            //                {
            //                    //Enroll after end time (as frist enroll)
            //                }
            //                else
            //                {
            //                    Att_record.AttendanceDate = s_Date;
            //                    Att_record.DeviceID = DeviceID;
            //                    Att_record.EntryTime = time.ToString();

            //                    if (time <= S_startTime)
            //                    {
            //                        Att_record.AttendanceStatus = "Pre";
            //                    }
            //                    else if (time <= S_LateTime)
            //                    {
            //                        Att_record.AttendanceStatus = "Late";
            //                    }
            //                    else if (time <= S_EndTime)
            //                    {
            //                        Att_record.AttendanceStatus = "Late Abs";
            //                    }

            //                    db.attendance_Records.Add(Att_record);
            //                    db.Entry(Att_record).State = EntityState.Added;
            //                }
            //            }
            //            else
            //            {
            //                if (Att_record.AttendanceStatus == "Abs")
            //                {
            //                    if (time < S_EndTime)
            //                    {
            //                        Att_record.AttendanceStatus = "Late Abs";
            //                    }

            //                    Att_record.EntryTime = time.ToString();
            //                    Att_record.Is_Sent = false;
            //                }
            //                else
            //                {
            //                    if (time > S_LateTime && time < S_EndTime && !Att_record.Is_OUT && TimeSpan.Parse(Att_record.EntryTime).TotalMinutes + 10 < time.TotalMinutes)
            //                    {
            //                        Att_record.ExitStatus = "Early Leave";
            //                        Att_record.Is_OUT = true;
            //                        Att_record.ExitTime = time.ToString();
            //                    }
            //                    else if (time > S_LateTime && time < S_EndTime && Att_record.Is_OUT && TimeSpan.Parse(Att_record.ExitTime).TotalMinutes + 10 < time.TotalMinutes)
            //                    {
            //                        Att_record.ExitStatus = "Early Leave";
            //                        Att_record.Is_OUT = true;
            //                        Att_record.ExitTime = time.ToString();
            //                        Att_record.Is_Updated = false;
            //                    }
            //                    else if (time > S_EndTime)
            //                    {
            //                        Att_record.ExitStatus = "Out";
            //                        Att_record.Is_OUT = true;
            //                        Att_record.ExitTime = time.ToString();
            //                        Att_record.Is_Updated = false;
            //                    }
            //                }

            //                db.Entry(Att_record).State = EntityState.Modified;
            //            }

            //           db.SaveChanges();

            //        }

            //    }

            //}

            return Ok();
        }
    }
}
