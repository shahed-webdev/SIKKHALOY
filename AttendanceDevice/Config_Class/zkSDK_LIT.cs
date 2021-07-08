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

        public static async Task SaveLogsOrAttendanceInPc(List<LogView> prevLog, List<LogView> todayLog, Institution institution, Device device)
        {
            var DuplicatePunchCountableMin = 10;

            using (var db = new ModelContext())
            {
                var previousLogs = prevLog.Select(a => new AttendanceLog_Backup
                {
                    DeviceID = a.DeviceId,
                    Entry_Date = a.EntryDate,
                    Entry_Time = a.EntryTime.ToString(),
                    Entry_Day = a.EntryDay,
                    Backup_Reason = "Not Current Data"
                });

                db.attendanceLog_Backups.AddRange(previousLogs);

                if (!institution.Is_Device_Attendance_Enable)
                {
                    var logs = todayLog.Select(a => new AttendanceLog_Backup
                    {
                        DeviceID = a.DeviceId,
                        Entry_Date = a.EntryDate,
                        Entry_Time = a.EntryTime.ToString(),
                        Entry_Day = a.EntryDay,
                        Backup_Reason = "Device Attendance Disable"
                    });

                    db.attendanceLog_Backups.AddRange(logs);
                }
                else
                {
                    foreach (var log in todayLog)
                    {
                        var dt = log.EntryDateTime;
                        var time = log.EntryTime;

                        if (log.EntryDate != DateTime.Today.ToShortDateString()) continue;

                        var user = await db.Users.FirstOrDefaultAsync(u => u.DeviceID == log.DeviceId);
                        var schedule = await db.attendance_Schedule_Days.FirstOrDefaultAsync(u => u.ScheduleID == user.ScheduleID);

                        var isStuDisable = user != null && user.Is_Student && !institution.Is_Student_Attendance_Enable;
                        var isEmpDisable = user != null && !user.Is_Student && !institution.Is_Employee_Attendance_Enable;

                        // Student Attendance Disable
                        if (isStuDisable)
                        {
                            var logBackup = new AttendanceLog_Backup()
                            {
                                DeviceID = log.DeviceId,
                                Entry_Date = log.EntryDate,
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
                                DeviceID = log.DeviceId,
                                Entry_Date = log.EntryDate,
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
                                DeviceID = log.DeviceId,
                                Entry_Date = log.EntryDate,
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
                                DeviceID = log.DeviceId,
                                Entry_Date = log.EntryDate,
                                Entry_Time = dt.ToShortTimeString(),
                                Entry_Day = dt.ToString("dddd"),
                                Backup_Reason = "Schedule Off Day"
                            };

                            db.attendanceLog_Backups.Add(logBackup);
                        }
                        // Insert or Update Attendance Records
                        else
                        {
                            var attRecord = await db.attendance_Records.Where(a => a.DeviceID == log.DeviceId && a.AttendanceDate == log.EntryDate).FirstOrDefaultAsync();
                            var sStartTime = TimeSpan.Parse(schedule.StartTime);
                            var sLateTime = TimeSpan.Parse(schedule.LateEntryTime);
                            var sEndTime = TimeSpan.Parse(schedule.EndTime);

                            if (attRecord == null)
                            {
                                attRecord = new Attendance_Record();
                                attRecord.AttendanceDate = log.EntryDate;
                                attRecord.DeviceID = log.DeviceId;
                                attRecord.EntryTime = time.ToString();

                                if (time > sEndTime)
                                {
                                    //Enroll after end time (as first enroll)
                                    attRecord.AttendanceStatus = "Late Abs";
                                }
                                else
                                {
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

                                }
                                attRecord.Is_Sent = false;
                                attRecord.Is_Updated = false;
                                db.attendance_Records.Add(attRecord);
                                db.Entry(attRecord).State = EntityState.Added;
                            }
                            else
                            {
                                var isDuplicatePunch = false;

                                if (attRecord.Is_OUT)
                                {
                                    if (TimeSpan.TryParse(attRecord.ExitStatus, out var previousTime))
                                    {
                                        isDuplicatePunch = previousTime.TotalMinutes + DuplicatePunchCountableMin > time.TotalMinutes;
                                    }
                                }
                                else
                                {

                                    if (TimeSpan.TryParse(attRecord.EntryTime, out var previousTime))
                                    {
                                        isDuplicatePunch = previousTime.TotalMinutes + DuplicatePunchCountableMin > time.TotalMinutes;
                                    }
                                }

                                if (!isDuplicatePunch)
                                {
                                    if (attRecord.AttendanceStatus == "Abs")
                                    {
                                        attRecord.AttendanceStatus = "Late Abs";
                                        attRecord.EntryTime = time.ToString();
                                        attRecord.Is_Updated = false;


                                    }
                                    else if (attRecord.AttendanceStatus == "Leave")
                                    {
                                        // no insert
                                    }
                                    else
                                    {
                                        if (time > sLateTime && time < sEndTime)
                                        {
                                            attRecord.ExitStatus = "Early Leave";
                                        }
                                        else if (time > sEndTime)
                                        {
                                            attRecord.ExitStatus = "Out";
                                        }

                                        attRecord.Is_Updated = false;
                                        attRecord.ExitTime = time.ToString();
                                        attRecord.Is_OUT = true;


                                    }


                                    db.Entry(attRecord).State = EntityState.Modified;
                                }
                            }

                        }

                        await db.SaveChangesAsync();
                    }
                }

                //Device last update time record
                prevLog.AddRange(todayLog);
                var maxDateTime = DateTime.Now;


                if (prevLog.Count > 0)
                {
                    maxDateTime = prevLog.Max(l => l.EntryDateTime).AddSeconds(1);

                }

                device.Last_Down_Log_Time = maxDateTime.ToString("yyyy-MM-dd HH:mm:ss");

                db.Devices.Add(device);
                db.Entry(device).State = EntityState.Modified;

                await db.SaveChangesAsync();
            }
        }

        public static List<Attendance_view> GetDailyAttendanceRecords(AttType attType)
        {
            var attendanceRecords = new List<Attendance_view>();
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

                if (attType == AttType.All)
                    attendanceRecords = q.Where(a => a.AttendanceDate == currentDate).OrderByDescending(a => a.EntryTime).ToList();
                else if (attType == AttType.AllStudent)
                    attendanceRecords = q.Where(a => a.AttendanceDate == currentDate && a.Is_Student).OrderByDescending(a => a.EntryTime).ToList();
                else if (attType == AttType.StudentIn)
                    attendanceRecords = q.Where(a => a.AttendanceDate == currentDate && a.Is_Student && !a.Is_OUT).OrderByDescending(a => a.EntryTime).ToList();
                else if (attType == AttType.StudentOut)
                    attendanceRecords = q.Where(a => a.AttendanceDate == currentDate && a.Is_Student && !a.Is_OUT).OrderByDescending(a => a.EntryTime).ToList();
                else if (attType == AttType.AllEmployee)
                    attendanceRecords = q.Where(a => a.AttendanceDate == currentDate && !a.Is_Student).OrderByDescending(a => a.EntryTime).ToList();
                else if (attType == AttType.EmployeeIn)
                    attendanceRecords = q.Where(a => a.AttendanceDate == currentDate && !a.Is_Student && !a.Is_OUT).OrderByDescending(a => a.EntryTime).ToList();
                else if (attType == AttType.EmployeeOut)
                    attendanceRecords = q.Where(a => a.AttendanceDate == currentDate && !a.Is_Student && a.Is_OUT).OrderByDescending(a => a.EntryTime).ToList();
            }
            return attendanceRecords;
        }
    }
}
