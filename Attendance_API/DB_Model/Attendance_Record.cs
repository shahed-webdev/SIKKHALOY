using System;
using System.ComponentModel.DataAnnotations;

namespace Attendance_API.DB_Model
{
    public class Attendance_Record
    {
        [Key]
        public int AttendanceRecordID { get; set; }
        public int SchoolID { get; set; }
        public int RegistrationID { get; set; } = 0;
        public int StudentID { get; set; }
        public int StudentClassID { get; set; }
        public int ClassID { get; set; }
        public int EducationYearID { get; set; }
        public string Attendance { get; set; }
        public DateTime AttendanceDate { get; set; }
        public TimeSpan? EntryTime { get; set; }
        public TimeSpan? ExitTime { get; set; }
        public string ExitStatus { get; set; }
        public bool Is_OUT { get; set; }
    }
}