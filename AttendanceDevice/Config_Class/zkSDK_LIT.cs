using AttendanceDevice.Model;
using AttendanceDevice.ViewModel;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;

namespace AttendanceDevice.Config_Class
{
    public static class Machine
    {
        public static readonly int Number = 1;

        public static async Task Save_logData(List<LogView> prev_log, List<LogView> today_log, Institution institution, Device device)
        {
            using (var db = new ModelContext())
            {
                var pLog = prev_log.Select(a => new AttendanceLog_Backup
                {
                    DeviceID = a.DeviceID,
                    Entry_Date = a.Entry_Date,
                    Entry_Time = a.Entry_Time.ToString(),
                    Entry_Day = a.Entry_Day,
                    Backup_Reason = "Not Current Data"
                });

                db.attendanceLog_Backups.AddRange(pLog);

                if (!institution.Is_Device_Attendance_Enable)
                {
                    var log = today_log.Select(a => new AttendanceLog_Backup
                    {
                        DeviceID = a.DeviceID,
                        Entry_Date = a.Entry_Date,
                        Entry_Time = a.Entry_Time.ToString(),
                        Entry_Day = a.Entry_Day,
                        Backup_Reason = "Device Attendance Disable"
                    });

                    db.attendanceLog_Backups.AddRange(log);
                }
                else
                {
                    foreach (var log in today_log)
                    {
                        var dt = log.Entry_DateTime;
                        var time = log.Entry_Time;

                        if (log.Entry_Date != DateTime.Today.ToShortDateString()) continue;

                        var user = await db.Users.FirstOrDefaultAsync(u => u.DeviceID == log.DeviceID);
                        var schedule = await db.attendance_Schedule_Days.FirstOrDefaultAsync(u => u.ScheduleID == user.ScheduleID);

                        var isStuDisable = user != null && user.Is_Student && !institution.Is_Student_Attendance_Enable;
                        var isEmpDisable = user != null && !user.Is_Student && !institution.Is_Employee_Attendance_Enable;

                        // Student Attendance Disable
                        if (isStuDisable)
                        {
                            var logBackup = new AttendanceLog_Backup()
                            {
                                DeviceID = log.DeviceID,
                                Entry_Date = log.Entry_Date,
                                Entry_Time = dt.ToShortTimeString(),
                                Entry_Day = dt.ToString("dddd"),
                                Backup_Reason = "Student Attendance Disable"
                            };

                            db.attendanceLog_Backups.Add(logBackup);
                        }
                        // Employee Attendance Disable
                        else if (isEmpDisable)
                        {
                            var logBackup = new AttendanceLog_Backup()
                            {
                                DeviceID = log.DeviceID,
                                Entry_Date = log.Entry_Date,
                                Entry_Time = dt.ToShortTimeString(),
                                Entry_Day = dt.ToString("dddd"),
                                Backup_Reason = "Employee Attendance Disable"
                            };

                            db.attendanceLog_Backups.Add(logBackup);
                        }

                        //Holiday attendance disable
                        else if (institution.Is_Today_Holiday && !institution.Holiday_NotActive)
                        {
                            var logBackup = new AttendanceLog_Backup()
                            {
                                DeviceID = log.DeviceID,
                                Entry_Date = log.Entry_Date,
                                Entry_Time = dt.ToShortTimeString(),
                                Entry_Day = dt.ToString("dddd"),
                                Backup_Reason = "Holiday attendance disable"
                            };

                            db.attendanceLog_Backups.Add(logBackup);

                        }
                        //Schedule Off day
                        else if (schedule != null && !schedule.Is_OnDay)
                        {
                            var logBackup = new AttendanceLog_Backup()
                            {
                                DeviceID = log.DeviceID,
                                Entry_Date = log.Entry_Date,
                                Entry_Time = dt.ToShortTimeString(),
                                Entry_Day = dt.ToString("dddd"),
                                Backup_Reason = "Schedule Off Day"
                            };

                            db.attendanceLog_Backups.Add(logBackup);
                        }
                        // Insert or Update Attendance Records
                        else
                        {
                            var attRecord = await db.attendance_Records.Where(a => a.DeviceID == log.DeviceID && a.AttendanceDate == log.Entry_Date).FirstOrDefaultAsync();
                            var S_startTime = TimeSpan.Parse(schedule.StartTime);
                            var S_LateTime = TimeSpan.Parse(schedule.LateEntryTime);
                            var S_EndTime = TimeSpan.Parse(schedule.EndTime);

                            if (attRecord == null)
                            {
                                attRecord = new Attendance_Record();

                                if (time > S_EndTime)
                                {
                                    //Enroll after end time (as first enroll)
                                }
                                else
                                {
                                    attRecord.AttendanceDate = log.Entry_Date;
                                    attRecord.DeviceID = log.DeviceID;
                                    attRecord.EntryTime = time.ToString();

                                    if (time <= S_startTime)
                                    {
                                        attRecord.AttendanceStatus = "Pre";
                                    }
                                    else if (time <= S_LateTime)
                                    {
                                        attRecord.AttendanceStatus = "Late";
                                    }
                                    else if (time <= S_EndTime)
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
                                    attRecord.AttendanceStatus = "Late Abs";
                                    attRecord.EntryTime = time.ToString();
                                    attRecord.Is_Sent = false;
                                }
                                else if (attRecord.AttendanceStatus == "Leave")
                                {
                                    // no insert
                                }
                                else
                                {
                                    if (time > S_LateTime && time < S_EndTime && !attRecord.Is_OUT && TimeSpan.Parse(attRecord.EntryTime).TotalMinutes + 10 < time.TotalMinutes)
                                    {
                                        attRecord.ExitStatus = "Early Leave";
                                        attRecord.Is_OUT = true;
                                        attRecord.ExitTime = time.ToString();
                                    }
                                    else if (time > S_LateTime && time < S_EndTime && attRecord.Is_OUT && TimeSpan.Parse(attRecord.ExitTime).TotalMinutes + 10 < time.TotalMinutes)
                                    {
                                        attRecord.ExitStatus = "Early Leave";
                                        attRecord.Is_OUT = true;
                                        attRecord.ExitTime = time.ToString();
                                        attRecord.Is_Updated = false;
                                    }
                                    else if (time > S_EndTime)
                                    {
                                        attRecord.ExitStatus = "Out";
                                        attRecord.Is_OUT = true;
                                        attRecord.ExitTime = time.ToString();
                                        attRecord.Is_Updated = false;
                                    }
                                }

                                db.Entry(attRecord).State = EntityState.Modified;
                            }

                        }

                        await db.SaveChangesAsync();
                    }
                }

                //Device last update time record
                prev_log.AddRange(today_log);
                var maxDateTime = DateTime.Now;

                if (prev_log.Count > 0)
                {
                    maxDateTime = prev_log.Max(l => l.Entry_DateTime);
                }

                device.Last_Down_Log_Time = maxDateTime.ToString("yyyy-MM-dd HH:mm:ss");

                db.Devices.Add(device);
                db.Entry(device).State = EntityState.Modified;

                await db.SaveChangesAsync();
            }
        }

        public static List<Attendance_view> GetAttendance(AttType att_type)
        {
            var av = new List<Attendance_view>();
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

                var currentDate = DateTime.Today.ToShortDateString();

                if (att_type == AttType.All)
                    av = q.Where(a => a.AttendanceDate == currentDate).OrderByDescending(a => a.EntryTime).ToList();
                else if (att_type == AttType.AllStudent)
                    av = q.Where(a => a.AttendanceDate == currentDate && a.Is_Student).OrderByDescending(a => a.EntryTime).ToList();
                else if (att_type == AttType.StudentIn)
                    av = q.Where(a => a.AttendanceDate == currentDate && a.Is_Student && !a.Is_OUT).OrderByDescending(a => a.EntryTime).ToList();
                else if (att_type == AttType.StudentOut)
                    av = q.Where(a => a.AttendanceDate == currentDate && a.Is_Student && !a.Is_OUT).OrderByDescending(a => a.EntryTime).ToList();
                else if (att_type == AttType.AllEmployee)
                    av = q.Where(a => a.AttendanceDate == currentDate && !a.Is_Student).OrderByDescending(a => a.EntryTime).ToList();
                else if (att_type == AttType.EmployeeIn)
                    av = q.Where(a => a.AttendanceDate == currentDate && !a.Is_Student && !a.Is_OUT).OrderByDescending(a => a.EntryTime).ToList();
                else if (att_type == AttType.EmployeeOut)
                    av = q.Where(a => a.AttendanceDate == currentDate && !a.Is_Student && a.Is_OUT).OrderByDescending(a => a.EntryTime).ToList();
            }
            return av;
        }
    }
}
