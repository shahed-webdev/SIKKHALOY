﻿using System.Data.Entity;

namespace SmsSenderApp
{
    public class ModelContext : DbContext
    {
        public ModelContext()
            : base(@"Data Source=.\;Initial Catalog=Edu;Integrated Security=SSPI")
        {
        }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            //throw new UnintentionalCodeFirstException();
        }

        //public virtual DbSet<Attendance_Device_Setting> Attendance_Device_Setting { get; set; }
        public virtual DbSet<Attendance_Record> Attendance_Record { get; set; }
        //public virtual DbSet<Attendance_Schedule> Attendance_Schedule { get; set; }
        //public virtual DbSet<Attendance_Schedule_AssignStudent> Attendance_Schedule_AssignStudent { get; set; }
        //public virtual DbSet<Attendance_Schedule_Day> Attendance_Schedule_Day { get; set; }
        public virtual DbSet<Attendance_SMS> Attendance_SMS { get; set; }
        public virtual DbSet<Attendance_SMS_Failed> Attendance_SMS_Failed { get; set; }
        public virtual DbSet<Attendance_SMS_Sender> Attendance_SMS_Sender { get; set; }
        //public virtual DbSet<Attendance_Student> Attendance_Student { get; set; }
        //public virtual DbSet<Education_Year> Education_Year { get; set; }
        //public virtual DbSet<Employee_Attendance_Report> Employee_Attendance_Report { get; set; }
        //public virtual DbSet<Employee_Attendance_Schedule_Assign> Employee_Attendance_Schedule_Assign { get; set; }
        //public virtual DbSet<SchoolInfo> SchoolInfoes { get; set; }
        public virtual DbSet<SikkhaloySetting> SikkhaloySettings { get; set; }
        public virtual DbSet<SMS> SMS { get; set; }
        public virtual DbSet<SMS_OtherInfo> SMS_OtherInfo { get; set; }
        public virtual DbSet<SMS_Send_Record> SMS_Send_Record { get; set; }
        //public virtual DbSet<VW_Attendance_Stu> VW_Attendance_Stu { get; set; }
        //public virtual DbSet<VW_Attendance_Stu_Setting> VW_Attendance_Stu_Setting { get; set; }
        //public virtual DbSet<VW_Attendance_User_Leave> VW_Attendance_User_Leave { get; set; }
        //public virtual DbSet<VW_Attendance_Users> VW_Attendance_Users { get; set; }
        //public virtual DbSet<VW_Emp_Info> VW_Emp_Info { get; set; }
    }
}