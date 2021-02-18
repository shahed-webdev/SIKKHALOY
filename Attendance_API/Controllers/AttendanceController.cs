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
        private TimeSpan Emtpy_Time = new TimeSpan(0, 0, 0);

        [Route("api/Attendance/{id}/Students")]
        [HttpPost]
        public IHttpActionResult PostStudents(int id, [FromBody]List<AttendanceRecordAPI> LogRecord)
        {
            if (LogRecord == null) return NotFound();
            if (LogRecord.Count < 1) return NotFound();

            using (var db = new EduContext())
            {
                var Att_Setting = new Attendance_Device_Setting();
                var today = DateTime.Now;
                var timeToday = DateTime.Now;

                Att_Setting = db.Attendance_Device_Settings.Where(s => s.SchoolID == id).FirstOrDefault();

                var SchoolName = db.SchoolInfos.Find(id).SchoolName;


                if (Att_Setting.Is_Device_Attendance_Enable && Att_Setting.Is_Student_Attendance_Enable)
                {
                    var AttendanceRecords = db.VW_Attendance_Stus.Where(a => a.SchoolID == id).ToList();

                    var NewRecord = from s in AttendanceRecords
                                    join a in LogRecord
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
                                        EntryTime = a.EntryTime == Emtpy_Time ? (TimeSpan?)null : a.EntryTime,
                                        ExitStatus = a.ExitStatus,
                                        ExitTime = a.ExitTime == Emtpy_Time ? (TimeSpan?)null : a.ExitTime,
                                        Is_OUT = a.Is_OUT
                                    };

                    var NewAtts_Recoad = NewRecord.Where(Na => !db.Attendance_Records.Any(Sa => Na.SchoolID == Sa.SchoolID & Na.StudentID == Sa.StudentID & Na.AttendanceDate == Sa.AttendanceDate)).ToList();

                    var sms_list = new List<Attendance_SMS>();

                    if (NewAtts_Recoad.Count > 0)
                    {
                        if (Att_Setting.Is_All_SMS_On && Att_Setting.Is_Student_All_SMS_Active)
                        {
                            ////------Abs & LateAbs SMS
                            var Stu_Abs_Setting = db.VW_Attendance_Stu_Settings.Where(a => a.SchoolID == id && a.Is_Abs_SMS).ToList();
                            if (Stu_Abs_Setting.Count > 0 && Att_Setting.Is_Student_Abs_SMS_ON)
                            {
                                var Att_Record_Abs = NewAtts_Recoad.Where(a => a.Attendance == "Abs").ToList();
                                if (Att_Record_Abs.Count > 0)
                                {
                                    var Stu_List = from a in Att_Record_Abs
                                                   join s in Stu_Abs_Setting
                                                    on a.StudentID equals s.StudentID
                                                   where s.Is_Abs_SMS && a.AttendanceDate.Date == today.Date
                                                   select new Attendance_SMS
                                                   {
                                                       SchoolID = id,
                                                       StudentID = a.StudentID,
                                                       ScheduleTime = s.StartTime,
                                                       AttendanceDate = a.AttendanceDate,
                                                       SMS_Text = Att_Setting.Is_English_SMS ?
                                                       "Respected guardian, " + s.StudentsName + " today(" + a.AttendanceDate.ToString("d MMM yy") + ") absent, please send to class regularly. " + SchoolName :
                                                       "সম্মানিত অভিভাবক, " + s.StudentsName + " আজ(" + a.AttendanceDate.ToString("d MMM yy") + ") অনুপস্থিত, অনুগ্রহ করে নিয়মিত ক্লাসে পাঠান " + SchoolName,
                                                       MobileNo = s.SMSPhoneNo,
                                                       AttendanceStatus = a.Attendance,
                                                       SMS_TimeOut = Att_Setting.SMS_TimeOut_Minute
                                                   };

                                    sms_list.AddRange(Stu_List);
                                }

                                var Att_Record_LatAbs = NewAtts_Recoad.Where(a => a.Attendance == "Late Abs").ToList();
                                if (Att_Record_LatAbs.Count > 0)
                                {
                                    var Stu_List = from a in Att_Record_LatAbs
                                                   join s in Stu_Abs_Setting
                                                    on a.StudentID equals s.StudentID
                                                   where s.Is_Abs_SMS && a.AttendanceDate.Date == today.Date
                                                   select new Attendance_SMS
                                                   {
                                                       SchoolID = id,
                                                       StudentID = a.StudentID,
                                                       ScheduleTime = s.StartTime,
                                                       AttendanceDate = a.AttendanceDate,
                                                       SMS_Text = Att_Setting.Is_English_SMS ?
                                                       "Respected guardian, " + s.StudentsName + " today(" + a.AttendanceDate.ToString("d MMM yy") + ") late absent. entry time " + timeToday.Add(a.EntryTime.GetValueOrDefault()).ToString("h:mm tt") + ". " + SchoolName :
                                                       "সম্মানিত অভিভাবক, " + s.StudentsName + " আজ(" + a.AttendanceDate.ToString("d MMM yy") + ") বিলম্বে (অনুপস্থিত হিসাবে), প্রবেশ করেছে। প্রবেশ সময় " + timeToday.Add(a.EntryTime.GetValueOrDefault()).ToString("h:mm tt") + ". " + SchoolName,
                                                       MobileNo = s.SMSPhoneNo,
                                                       AttendanceStatus = a.Attendance,
                                                       SMS_TimeOut = Att_Setting.SMS_TimeOut_Minute
                                                   };

                                    sms_list.AddRange(Stu_List);
                                }
                            }

                            //------Entry SMS
                            var Stu_Entry_Setting = db.VW_Attendance_Stu_Settings.Where(a => a.SchoolID == id && a.Entry_Confirmation).ToList();
                            if (Stu_Entry_Setting.Count > 0 && Att_Setting.Is_Student_Entry_SMS_ON)
                            {
                                var Att_Record_Pre = NewAtts_Recoad.Where(a => a.Attendance == "Pre").ToList();
                                if (Att_Record_Pre.Count > 0)
                                {
                                    var Stu_List = from a in Att_Record_Pre
                                                   join s in Stu_Entry_Setting
                                                   on a.StudentID equals s.StudentID
                                                   where s.Entry_Confirmation && a.AttendanceDate.Date == today.Date
                                                   select new Attendance_SMS
                                                   {
                                                       SchoolID = id,
                                                       StudentID = a.StudentID,
                                                       ScheduleTime = s.StartTime,
                                                       AttendanceDate = a.AttendanceDate,
                                                       SMS_Text = Att_Setting.Is_English_SMS ?
                                                      "Respected guardian, " + s.StudentsName + " has reached " + SchoolName + " at " + DateTime.Today.Add(a.EntryTime.GetValueOrDefault()).ToString("hh:mm tt") :
                                                      "সম্মানিত অভিভাবক, " + s.StudentsName + " নিরাপদে " + SchoolName + " এ (" + DateTime.Today.Add(a.EntryTime.GetValueOrDefault()).ToString("hh:mm tt") + ") প্রবেশ করেছে",
                                                       MobileNo = s.SMSPhoneNo,
                                                       AttendanceStatus = a.Attendance,
                                                       SMS_TimeOut = Att_Setting.SMS_TimeOut_Minute
                                                   };

                                    sms_list.AddRange(Stu_List);
                                }
                            }

                            ////------Late SMS
                            var Stu_Late_Setting = db.VW_Attendance_Stu_Settings.Where(a => a.SchoolID == id && a.Is_Late_SMS).ToList();
                            if (Stu_Late_Setting.Count > 0 && Att_Setting.Is_Student_Late_SMS_ON)
                            {
                                var Att_Record_Late = NewAtts_Recoad.Where(a => a.Attendance == "Late").ToList();
                                if (Att_Record_Late.Count > 0)
                                {
                                    var Stu_List = from a in Att_Record_Late
                                                   join s in Stu_Late_Setting
                                                    on a.StudentID equals s.StudentID
                                                   where s.Is_Late_SMS && a.AttendanceDate.Date == today.Date
                                                   select new Attendance_SMS
                                                   {
                                                       SchoolID = id,
                                                       StudentID = a.StudentID,
                                                       ScheduleTime = s.StartTime,
                                                       AttendanceDate = a.AttendanceDate,
                                                       SMS_Text = Att_Setting.Is_English_SMS ?
                                                       "Respected guardian, " + s.StudentsName + " today(" + a.AttendanceDate.ToString("d MMM yy") + ") late " + (a.EntryTime.GetValueOrDefault() - s.StartTime).Minutes + " min, entry time " + timeToday.Add(a.EntryTime.GetValueOrDefault()).ToString("h:mm tt") + ". " + SchoolName :
                                                       "সম্মানিত অভিভাবক, " + s.StudentsName + " আজ(" + a.AttendanceDate.ToString("d MMM yy") + ") " + (a.EntryTime.GetValueOrDefault() - s.StartTime).Minutes + " মি: বিলম্বে, প্রবেশ করেছে। প্রবেশ সময় " + timeToday.Add(a.EntryTime.GetValueOrDefault()).ToString("h:mm tt") + ". " + SchoolName,
                                                       MobileNo = s.SMSPhoneNo,
                                                       AttendanceStatus = a.Attendance,
                                                       SMS_TimeOut = Att_Setting.SMS_TimeOut_Minute
                                                   };

                                    sms_list.AddRange(Stu_List);
                                }
                            }

                            ////------Exit SMS
                            var Stu_Exit_Setting = db.VW_Attendance_Stu_Settings.Where(a => a.SchoolID == id && a.Exit_Confirmation).ToList();
                            if (Stu_Exit_Setting.Count > 0 && Att_Setting.Is_Student_Exit_SMS_ON)
                            {
                                var Att_Record_Exit = NewAtts_Recoad.Where(a => a.ExitStatus == "Exit").ToList();
                                if (Att_Record_Exit.Count > 0)
                                {
                                    var Stu_List = from a in Att_Record_Exit
                                                   join s in Stu_Exit_Setting
                                                       on a.StudentID equals s.StudentID
                                                   where s.Exit_Confirmation && a.AttendanceDate.Date == today.Date
                                                   select new Attendance_SMS
                                                   {
                                                       SchoolID = id,
                                                       StudentID = a.StudentID,
                                                       ScheduleTime = s.StartTime,
                                                       AttendanceDate = a.AttendanceDate,
                                                       SMS_Text = Att_Setting.Is_English_SMS ?
                                                       "Respected guardian, " + s.StudentsName + " has exited from " + SchoolName + " at " + timeToday.Add(a.ExitTime.GetValueOrDefault()).ToString("h:mm tt") :
                                                       "সম্মানিত অভিভাবক, " + s.StudentsName + ", " + SchoolName + " থেকে " + timeToday.Add(a.ExitTime.GetValueOrDefault()).ToString("h:mm tt") + " প্রস্থান করেছে",
                                                       MobileNo = s.SMSPhoneNo,
                                                       AttendanceStatus = a.Attendance,
                                                       SMS_TimeOut = Att_Setting.SMS_TimeOut_Minute
                                                   };
                                    sms_list.AddRange(Stu_List);
                                }
                            }
                        }

                        if (sms_list.Count > 0) db.Attendance_sms.AddRange(sms_list);
                        db.Attendance_Records.AddRange(NewAtts_Recoad);
                        db.SaveChanges();
                    }
                }
            }

            return Ok();
        }

        [Route("api/Attendance/{id}/StudentsUpdate")]
        [HttpPost]
        public IHttpActionResult PutStudents(int id, [FromBody]List<AttendanceRecordAPI> LogRecord)
        {
            if (LogRecord == null) return NotFound();
            if (LogRecord.Count < 1) return NotFound();

            using (var db = new EduContext())
            {
                var Att_Setting = new Attendance_Device_Setting();
                var today = DateTime.Now;
                var timeToday = DateTime.Now;

                Att_Setting = db.Attendance_Device_Settings.Where(s => s.SchoolID == id).FirstOrDefault();

                var SchoolName = db.SchoolInfos.Find(id).SchoolName;

                var LogforSMS = new List<Attendance_Record>();
                if (Att_Setting.Is_Device_Attendance_Enable && Att_Setting.Is_Student_Attendance_Enable)
                {
                    var StudentList = db.VW_Attendance_Stus.Where(a => a.SchoolID == id).ToList();

                    var UpdatedRecords = from s in StudentList
                                         join a in LogRecord
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
                                             EntryTime = a.EntryTime == Emtpy_Time ? (TimeSpan?)null : a.EntryTime,
                                             ExitStatus = a.ExitStatus,
                                             ExitTime = a.ExitTime == Emtpy_Time ? (TimeSpan?)null : a.ExitTime,
                                             Is_OUT = a.Is_OUT
                                         };

                    foreach (var log in UpdatedRecords)
                    {
                        var UpdateLog = db.Attendance_Records.Where(u => u.SchoolID == log.SchoolID & u.StudentID == log.StudentID & u.AttendanceDate == log.AttendanceDate).FirstOrDefault();

                        if (UpdateLog != null)
                        {
                            UpdateLog.Attendance = log.Attendance;
                            UpdateLog.ExitStatus = log.ExitStatus;

                            UpdateLog.ExitTime = log.ExitTime;
                            UpdateLog.Is_OUT = log.Is_OUT;
                            UpdateLog.EntryTime = log.EntryTime;

                            db.Entry(UpdateLog).State = EntityState.Modified;
                            db.SaveChanges();
                            LogforSMS.Add(UpdateLog);
                        }
                    }
                }


                if (LogforSMS.Count > 0)
                {
                    var sms_list = new List<Attendance_SMS>();

                    if (Att_Setting.Is_All_SMS_On && Att_Setting.Is_Student_All_SMS_Active)
                    {
                        ////------LateAbs SMS
                        if (Att_Setting.Is_Student_Abs_SMS_ON)
                        {
                            var Att_Record_LatAbs = LogforSMS.Where(a => a.Attendance == "Late Abs").ToList();

                            if (Att_Record_LatAbs.Count > 0)
                            {
                                foreach (var log in Att_Record_LatAbs)
                                {
                                    var If_SMS_Created = db.Attendance_sms.Where(a => a.SchoolID == id && a.StudentID == log.StudentID && a.AttendanceDate == log.AttendanceDate && a.AttendanceStatus == "Late Abs").FirstOrDefault();
                                    var stu = db.VW_Attendance_Stu_Settings.Where(s => s.SchoolID == id && s.Is_Abs_SMS && s.StudentID == log.StudentID).FirstOrDefault();

                                    if (If_SMS_Created != null && log.AttendanceDate.Date == today.Date && stu != null)
                                    {
                                        var Stu_List = new Attendance_SMS
                                        {
                                            SchoolID = id,
                                            StudentID = log.StudentID,
                                            ScheduleTime = stu.StartTime,
                                            AttendanceDate = log.AttendanceDate,
                                            SMS_Text = Att_Setting.Is_English_SMS ?
                                            "Respected guardian, " + stu.StudentsName + " today(" + log.AttendanceDate.ToString("d MMM yy") + ") late absent. Entry time " + timeToday.Add(log.EntryTime.GetValueOrDefault()).ToString("h:mm tt") + ". " + SchoolName :
                                            "সম্মানিত অভিভাবক, " + stu.StudentsName + " আজ(" + log.AttendanceDate.ToString("d MMM yy") + ") বিলম্বে (অনুপস্থিত হিসাবে), প্রবেশ করেছে। প্রবেশ সময় " + timeToday.Add(log.EntryTime.GetValueOrDefault()).ToString("h:mm tt") + ". " + SchoolName,
                                            MobileNo = stu.SMSPhoneNo,
                                            AttendanceStatus = log.Attendance,
                                            SMS_TimeOut = Att_Setting.SMS_TimeOut_Minute
                                        };

                                        sms_list.Add(Stu_List);
                                        db.SaveChanges();
                                    }
                                }
                            }
                        }


                        ////------Exit SMS
                        var Stu_Exit_Setting = db.VW_Attendance_Stu_Settings.Where(a => a.SchoolID == id && a.Exit_Confirmation).ToList();
                        if (Att_Setting.Is_Student_Exit_SMS_ON)
                        {
                            var Att_Record_Exit = LogforSMS.Where(a => a.Attendance == "Exit").ToList();
                            if (Att_Record_Exit.Count > 0)
                            {
                                foreach (var log in Att_Record_Exit)
                                {

                                    var If_SMS_Created = db.Attendance_sms.Where(a => a.SchoolID == id && a.StudentID == log.StudentID && a.AttendanceDate == log.AttendanceDate && a.AttendanceStatus == "Exit").FirstOrDefault();

                                    var stu = db.VW_Attendance_Stu_Settings.Where(s => s.SchoolID == id && s.Exit_Confirmation && s.StudentID == log.StudentID).FirstOrDefault();

                                    if (If_SMS_Created != null && log.AttendanceDate.Date == today.Date && stu != null)
                                    {
                                        var Stu_List = new Attendance_SMS
                                        {
                                            SchoolID = id,
                                            StudentID = log.StudentID,
                                            ScheduleTime = stu.StartTime,
                                            AttendanceDate = log.AttendanceDate,
                                            SMS_Text = Att_Setting.Is_English_SMS ?
                                            "Respected guardian, " + stu.StudentsName + " has exited from " + SchoolName + " at " + timeToday.Add(log.ExitTime.GetValueOrDefault()).ToString("h:mm tt") :
                                            "সম্মানিত অভিভাবক, " + stu.StudentsName + ", " + SchoolName + " থেকে " + timeToday.Add(log.ExitTime.GetValueOrDefault()).ToString("h:mm tt") + " প্রস্থান করেছে",
                                            MobileNo = stu.SMSPhoneNo,
                                            AttendanceStatus = log.Attendance,
                                            SMS_TimeOut = Att_Setting.SMS_TimeOut_Minute
                                        };
                                        sms_list.Add(Stu_List);
                                        db.SaveChanges();
                                    }
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
        public IHttpActionResult PostEmployees(int id, [FromBody]List<AttendanceRecordAPI> LogRecord)
        {
            if (LogRecord == null) return NotFound();
            if (LogRecord.Count < 1) return NotFound();

            using (var db = new EduContext())
            {
                var Att_Setting = new Attendance_Device_Setting();
                var today = DateTime.Now;
                var timeToday = DateTime.Now;

                Att_Setting = db.Attendance_Device_Settings.Where(s => s.SchoolID == id).FirstOrDefault();

                var SchoolName = db.SchoolInfos.Find(id).SchoolName;


                if (Att_Setting.Is_Device_Attendance_Enable && Att_Setting.Is_Employee_Attendance_Enable)
                {
                    var EmpList = db.VW_Emp_Infos.Where(a => a.SchoolID == id).ToList();

                    var NewRecord = from e in EmpList
                                    join a in LogRecord
                                    on e.DeviceID equals a.DeviceID
                                    where e.SchoolID == id
                                    select new Employee_Attendance_Record
                                    {
                                        SchoolID = e.SchoolID,
                                        EmployeeID = e.EmployeeID,
                                        RegistrationID = 0,
                                        AttendanceStatus = a.AttendanceStatus,
                                        AttendanceDate = a.AttendanceDate,
                                        EntryTime = a.EntryTime == Emtpy_Time ? (TimeSpan?)null : a.EntryTime,
                                        ExitStatus = a.ExitStatus,
                                        ExitTime = a.ExitTime == Emtpy_Time ? (TimeSpan?)null : a.ExitTime,
                                        Is_OUT = a.Is_OUT
                                    };

                    var NewAtts_Recoad = NewRecord.Where(Na => !db.Employee_Attendance_Records.Any(Sa => Na.SchoolID == Sa.SchoolID & Na.EmployeeID == Sa.EmployeeID & Na.AttendanceDate == Sa.AttendanceDate)).ToList();

                    var sms_list = new List<Attendance_SMS>();

                    if (NewAtts_Recoad.Count > 0)
                    {
                        if (Att_Setting.Is_All_SMS_On && Att_Setting.Is_Employee_SMS_Active)
                        {
                            ////------Abs & LateAbs SMS
                            var Emp_Abs_Setting = db.VW_Attendance_Emp_Settings.Where(a => a.SchoolID == id && a.Is_Abs_SMS).ToList();
                            if (Emp_Abs_Setting.Count > 0 && Att_Setting.Is_Employee_SMS_Active)
                            {
                                var Att_Record_Abs = NewAtts_Recoad.Where(a => a.AttendanceStatus == "Abs").ToList();
                                if (Att_Record_Abs.Count > 0)
                                {
                                    var Emp_List = from a in Att_Record_Abs
                                                   join e in Emp_Abs_Setting
                                                   on a.EmployeeID equals e.EmployeeID
                                                   where e.Is_Abs_SMS && a.AttendanceDate.Date == today.Date
                                                   select new Attendance_SMS
                                                   {
                                                       SchoolID = id,
                                                       EmployeeID = a.EmployeeID,
                                                       ScheduleTime = e.StartTime,
                                                       AttendanceDate = a.AttendanceDate,
                                                       SMS_Text = Att_Setting.Is_English_SMS ?
                                                       e.Name + " Today(" + a.AttendanceDate.ToString("d MMM yy") + ") absent" :
                                                       e.Name + " আজ(" + a.AttendanceDate.ToString("d MMM yy") + ") অনুপস্থিত",
                                                       MobileNo = Att_Setting.Is_Employee_SMS_OwnNumber ? e.Phone : Att_Setting.Employee_SMS_Number,
                                                       AttendanceStatus = a.AttendanceStatus,
                                                       SMS_TimeOut = Att_Setting.SMS_TimeOut_Minute
                                                   };

                                    sms_list.AddRange(Emp_List);
                                }

                                var Att_Record_LatAbs = NewAtts_Recoad.Where(a => a.AttendanceStatus == "Late Abs").ToList();
                                if (Att_Record_LatAbs.Count > 0)
                                {
                                    var Stu_List = from a in Att_Record_LatAbs
                                                   join e in Emp_Abs_Setting
                                                    on a.EmployeeID equals e.EmployeeID
                                                   where e.Is_Abs_SMS && a.AttendanceDate.Date == today.Date
                                                   select new Attendance_SMS
                                                   {
                                                       SchoolID = id,
                                                       EmployeeID = a.EmployeeID,
                                                       ScheduleTime = e.StartTime,
                                                       AttendanceDate = a.AttendanceDate,
                                                       SMS_Text = Att_Setting.Is_English_SMS ?
                                                       e.Name + " Today(" + a.AttendanceDate.ToString("d MMM yy") + ") late absent. Entry time " + timeToday.Add(a.EntryTime.GetValueOrDefault()).ToString("h:mm tt") :
                                                       e.Name + " আজ(" + a.AttendanceDate.ToString("d MMM yy") + ") বিলম্বে (অনুপস্থিত হিসাবে), প্রবেশ করেছে। প্রবেশ সময় " + timeToday.Add(a.EntryTime.GetValueOrDefault()).ToString("h:mm tt"),
                                                       MobileNo = Att_Setting.Is_Employee_SMS_OwnNumber ? e.Phone : Att_Setting.Employee_SMS_Number,
                                                       AttendanceStatus = a.AttendanceStatus,
                                                       SMS_TimeOut = Att_Setting.SMS_TimeOut_Minute
                                                   };

                                    sms_list.AddRange(Stu_List);
                                }
                            }


                            ////------Late SMS
                            var Emp_Late_Setting = db.VW_Attendance_Emp_Settings.Where(a => a.SchoolID == id && a.Is_Late_SMS).ToList();
                            if (Emp_Late_Setting.Count > 0 && Att_Setting.Is_Employee_Late_SMS_ON)
                            {
                                var Att_Record_Late = NewAtts_Recoad.Where(a => a.AttendanceStatus == "Late").ToList();
                                if (Att_Record_Late.Count > 0)
                                {
                                    var Stu_List = from a in Att_Record_Late
                                                   join e in Emp_Late_Setting
                                                    on a.EmployeeID equals e.EmployeeID
                                                   where e.Is_Late_SMS && a.AttendanceDate.Date == today.Date
                                                   select new Attendance_SMS
                                                   {
                                                       SchoolID = id,
                                                       EmployeeID = a.EmployeeID,
                                                       ScheduleTime = e.StartTime,
                                                       AttendanceDate = a.AttendanceDate,
                                                       SMS_Text = Att_Setting.Is_English_SMS ?
                                                       e.Name + " Today(" + a.AttendanceDate.ToString("d MMM yy") + ") late " + (a.EntryTime.GetValueOrDefault() - e.StartTime).Minutes + " min. Entry time " + timeToday.Add(a.EntryTime.GetValueOrDefault()).ToString("h:mm tt") :
                                                       e.Name + " আজ(" + a.AttendanceDate.ToString("d MMM yy") + ") " + (a.EntryTime.GetValueOrDefault() - e.StartTime).Minutes + " মি: বিলম্বে প্রবেশ করেছে। প্রবেশ সময় " + timeToday.Add(a.EntryTime.GetValueOrDefault()).ToString("h:mm tt"),
                                                       MobileNo = Att_Setting.Is_Employee_SMS_OwnNumber ? e.Phone : Att_Setting.Employee_SMS_Number,
                                                       AttendanceStatus = a.AttendanceStatus,
                                                       SMS_TimeOut = Att_Setting.SMS_TimeOut_Minute
                                                   };

                                    sms_list.AddRange(Stu_List);
                                }
                            }
                        }

                        if (sms_list.Count > 0) db.Attendance_sms.AddRange(sms_list);
                        db.Employee_Attendance_Records.AddRange(NewAtts_Recoad);
                        db.SaveChanges();
                    }
                }
            }


            return Ok();
        }

        [Route("api/Attendance/{id}/EmployeesUpdate")]
        [HttpPost]
        public IHttpActionResult PutEmployees(int id, [FromBody]List<AttendanceRecordAPI> LogRecord)
        {
            if (LogRecord == null) return NotFound();
            if (LogRecord.Count < 1) return NotFound();

            using (var db = new EduContext())
            {
                var Att_Setting = new Attendance_Device_Setting();
                var today = DateTime.Now;
                var timeToday = DateTime.Now;

                Att_Setting = db.Attendance_Device_Settings.Where(s => s.SchoolID == id).FirstOrDefault();

                var SchoolName = db.SchoolInfos.Find(id).SchoolName;


                if (Att_Setting.Is_Device_Attendance_Enable && Att_Setting.Is_Employee_Attendance_Enable)
                {
                    var EmpList = db.VW_Emp_Infos.Where(a => a.SchoolID == id).ToList();

                    var UpdatedRecords = from e in EmpList
                                         join a in LogRecord
                                         on e.DeviceID equals a.DeviceID
                                         where e.SchoolID == id
                                         select new Employee_Attendance_Record
                                         {
                                             SchoolID = e.SchoolID,
                                             EmployeeID = e.EmployeeID,
                                             RegistrationID = 0,
                                             AttendanceStatus = a.AttendanceStatus,
                                             AttendanceDate = a.AttendanceDate,
                                             EntryTime = a.EntryTime == Emtpy_Time ? (TimeSpan?)null : a.EntryTime,
                                             ExitStatus = a.ExitStatus,
                                             ExitTime = a.ExitTime == Emtpy_Time ? (TimeSpan?)null : a.ExitTime,
                                             Is_OUT = a.Is_OUT
                                         };

                    foreach (var log in UpdatedRecords)
                    {
                        var UpdateLog = db.Employee_Attendance_Records.Where(u => u.SchoolID == log.SchoolID & u.EmployeeID == log.EmployeeID & u.AttendanceDate == log.AttendanceDate).FirstOrDefault();
                        if (UpdateLog != null)
                        {
                            UpdateLog.AttendanceStatus = log.AttendanceStatus;
                            UpdateLog.ExitStatus = log.ExitStatus;

                            UpdateLog.ExitTime = log.ExitTime;
                            UpdateLog.Is_OUT = log.Is_OUT;
                            UpdateLog.EntryTime = log.EntryTime;

                            db.Entry(UpdateLog).State = EntityState.Modified;
                            db.SaveChanges();
                        }
                    }
                }
            }

            return Ok();
        }


        [Route("api/Attendance/{id}/backup_data")]
        [HttpPost]
        public IHttpActionResult PostBackupData(int id, [FromBody]List<BackupDataAPI> BackupData)
        {
            if (BackupData == null) return NotFound();
            if (BackupData.Count < 1) return NotFound();

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
