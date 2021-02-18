using AttendanceDevice.Model;
using AttendanceDevice.ViewModel;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;

namespace AttendanceDevice.Config_Class
{
    public enum Att_Type
    {
        All,
        All_Sutdent,
        All_Employee,
        Studnet_IN,
        Studnet_OUT,
        Employee_IN,
        Employee_OUT
    }

    public static class Machine
    {
        public static readonly int Number = 1;

        public static async Task Save_logData(List<LogView> prev_log, List<LogView> today_log, Institution institution, Device device)
        {
            using (var db = new ModelContext())
            {
                var P_log = prev_log.Select(a => new AttendanceLog_Backup
                {
                    DeviceID = a.DeviceID,
                    Entry_Date = a.Entry_Date,
                    Entry_Time = a.Entry_Time.ToString(),
                    Entry_Day = a.Entry_Day,
                    Backup_Reason = "Not Current Data"
                });

                db.attendanceLog_Backups.AddRange(P_log);

                if (!institution.Is_Device_Attendance_Enable)
                {
                    var T_log = today_log.Select(a => new AttendanceLog_Backup
                    {
                        DeviceID = a.DeviceID,
                        Entry_Date = a.Entry_Date,
                        Entry_Time = a.Entry_Time.ToString(),
                        Entry_Day = a.Entry_Day,
                        Backup_Reason = "Device Attendance Disable"
                    });

                    db.attendanceLog_Backups.AddRange(T_log);
                }
                else
                {
                    foreach (var log in today_log)
                    {
                        var dt = log.Entry_DateTime;
                        var time = log.Entry_Time;

                        if (log.Entry_Date == DateTime.Today.ToShortDateString())
                        {
                            var user = await db.Users.FirstOrDefaultAsync(u => u.DeviceID == log.DeviceID);
                            var Schedule = await db.attendance_Schedule_Days.FirstOrDefaultAsync(u => u.ScheduleID == user.ScheduleID);

                            var Is_stu_Disable = user.Is_Student && !institution.Is_Student_Attendance_Enable;
                            var Is_Emp_Disable = !user.Is_Student && !institution.Is_Employee_Attendance_Enable;

                            // Student Attendance Disable
                            if (Is_stu_Disable)
                            {
                                var log_backup = new AttendanceLog_Backup()
                                {
                                    DeviceID = log.DeviceID,
                                    Entry_Date = log.Entry_Date,
                                    Entry_Time = dt.ToShortTimeString(),
                                    Entry_Day = dt.ToString("dddd"),
                                    Backup_Reason = "Student Attendance Disable"
                                };

                                db.attendanceLog_Backups.Add(log_backup);
                            }
                            // Employee Attendance Disable
                            else if (Is_Emp_Disable)
                            {
                                var log_backup = new AttendanceLog_Backup()
                                {
                                    DeviceID = log.DeviceID,
                                    Entry_Date = log.Entry_Date,
                                    Entry_Time = dt.ToShortTimeString(),
                                    Entry_Day = dt.ToString("dddd"),
                                    Backup_Reason = "Employee Attendance Disable"
                                };

                                db.attendanceLog_Backups.Add(log_backup);
                            }
                            //Hodiday attendance disable
                            else if (institution.Is_Today_Holiday && !institution.Holiday_NotActive)
                            {
                                var log_backup = new AttendanceLog_Backup()
                                {
                                    DeviceID = log.DeviceID,
                                    Entry_Date = log.Entry_Date,
                                    Entry_Time = dt.ToShortTimeString(),
                                    Entry_Day = dt.ToString("dddd"),
                                    Backup_Reason = "Hodiday attendance disable"
                                };

                                db.attendanceLog_Backups.Add(log_backup);

                            }
                            //Schedule Off day
                            else if (!Schedule.Is_OnDay)
                            {
                                var log_backup = new AttendanceLog_Backup()
                                {
                                    DeviceID = log.DeviceID,
                                    Entry_Date = log.Entry_Date,
                                    Entry_Time = dt.ToShortTimeString(),
                                    Entry_Day = dt.ToString("dddd"),
                                    Backup_Reason = "Schedule Off Day"
                                };

                                db.attendanceLog_Backups.Add(log_backup);
                            }
                            // Insert or Update Attendance Records
                            else
                            {
                                var Att_record = await db.attendance_Records.Where(a => a.DeviceID == log.DeviceID && a.AttendanceDate == log.Entry_Date).FirstOrDefaultAsync();
                                var S_startTime = TimeSpan.Parse(Schedule.StartTime);
                                var S_LateTime = TimeSpan.Parse(Schedule.LateEntryTime);
                                var S_EndTime = TimeSpan.Parse(Schedule.EndTime);

                                if (Att_record == null)
                                {
                                    Att_record = new Attendance_Record();

                                    if (time > S_EndTime)
                                    {
                                        //Enroll after end time (as frist enroll)
                                    }
                                    else
                                    {
                                        Att_record.AttendanceDate = log.Entry_Date;
                                        Att_record.DeviceID = log.DeviceID;
                                        Att_record.EntryTime = time.ToString();

                                        if (time <= S_startTime)
                                        {
                                            Att_record.AttendanceStatus = "Pre";
                                        }
                                        else if (time <= S_LateTime)
                                        {
                                            Att_record.AttendanceStatus = "Late";
                                        }
                                        else if (time <= S_EndTime)
                                        {
                                            Att_record.AttendanceStatus = "Late Abs";
                                        }

                                        db.attendance_Records.Add(Att_record);
                                        db.Entry(Att_record).State = EntityState.Added;
                                    }
                                }
                                else
                                {
                                    if (Att_record.AttendanceStatus == "Abs")
                                    {
                                        Att_record.AttendanceStatus = "Late Abs";
                                        Att_record.EntryTime = time.ToString();
                                        Att_record.Is_Sent = false;
                                    }
                                    else if (Att_record.AttendanceStatus == "Leave")
                                    {
                                        // no insert
                                    }
                                    else
                                    {
                                        if (time > S_LateTime && time < S_EndTime && !Att_record.Is_OUT && TimeSpan.Parse(Att_record.EntryTime).TotalMinutes + 10 < time.TotalMinutes)
                                        {
                                            Att_record.ExitStatus = "Early Leave";
                                            Att_record.Is_OUT = true;
                                            Att_record.ExitTime = time.ToString();
                                        }
                                        else if (time > S_LateTime && time < S_EndTime && Att_record.Is_OUT && TimeSpan.Parse(Att_record.ExitTime).TotalMinutes + 10 < time.TotalMinutes)
                                        {
                                            Att_record.ExitStatus = "Early Leave";
                                            Att_record.Is_OUT = true;
                                            Att_record.ExitTime = time.ToString();
                                            Att_record.Is_Updated = false;
                                        }
                                        else if (time > S_EndTime)
                                        {
                                            Att_record.ExitStatus = "Out";
                                            Att_record.Is_OUT = true;
                                            Att_record.ExitTime = time.ToString();
                                            Att_record.Is_Updated = false;
                                        }
                                    }

                                    db.Entry(Att_record).State = EntityState.Modified;
                                }

                            }

                            await db.SaveChangesAsync();
                        }
                    }
                }

                //Device last update time record
                prev_log.AddRange(today_log);
                var MaxDateTime = DateTime.Now;

                if (prev_log.Count > 0)
                {
                    MaxDateTime = prev_log.Max(l => l.Entry_DateTime);
                }

                device.Last_Down_Log_Time = MaxDateTime.ToString("yyyy-MM-dd HH:mm:ss");

                db.Devices.Add(device);
                db.Entry(device).State = EntityState.Modified;

                await db.SaveChangesAsync();
            }
        }

        public static List<Attendance_view> GetAttendance(Att_Type att_type)
        {
            List<Attendance_view> AV = new List<Attendance_view>();
            using (var db = new ModelContext())
            {
                var q = from a in db.attendance_Records
                        join u in db.Users
                        on a.DeviceID equals u.DeviceID
                        select new Attendance_view
                        {
                            DeviceID = u.DeviceID,
                            ID = u.ID,
                            Name = u.Name,
                            Designation = u.Designation,
                            ImgLink = LocalData.Instance.institution.Image_Link + "\\" + u.ID + ".jpg",
                            AttendanceStatus = a.AttendanceStatus,
                            AttendanceDate = a.AttendanceDate,
                            Is_OUT = a.Is_OUT,
                            Is_Student = u.Is_Student,
                            EntryTime = a.EntryTime,
                            ExitTime = a.ExitTime
                        };

                var current_Date = DateTime.Today.ToShortDateString();

                if (att_type == Att_Type.All)
                    AV = q.Where(a => a.AttendanceDate == current_Date).OrderByDescending(a => a.EntryTime).ToList();
                else if (att_type == Att_Type.All_Sutdent)
                    AV = q.Where(a => a.AttendanceDate == current_Date && a.Is_Student).OrderByDescending(a => a.EntryTime).ToList();
                else if (att_type == Att_Type.Studnet_IN)
                    AV = q.Where(a => a.AttendanceDate == current_Date && a.Is_Student && !a.Is_OUT).OrderByDescending(a => a.EntryTime).ToList();
                else if (att_type == Att_Type.Studnet_OUT)
                    AV = q.Where(a => a.AttendanceDate == current_Date && a.Is_Student && !a.Is_OUT).OrderByDescending(a => a.EntryTime).ToList();
                else if (att_type == Att_Type.All_Employee)
                    AV = q.Where(a => a.AttendanceDate == current_Date && !a.Is_Student).OrderByDescending(a => a.EntryTime).ToList();
                else if (att_type == Att_Type.Employee_IN)
                    AV = q.Where(a => a.AttendanceDate == current_Date && !a.Is_Student && !a.Is_OUT).OrderByDescending(a => a.EntryTime).ToList();
                else if (att_type == Att_Type.Employee_OUT)
                    AV = q.Where(a => a.AttendanceDate == current_Date && !a.Is_Student && a.Is_OUT).OrderByDescending(a => a.EntryTime).ToList();
            }
            return AV;
        }
    }
}
