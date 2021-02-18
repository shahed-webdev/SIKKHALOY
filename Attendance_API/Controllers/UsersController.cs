using Attendance_API.DB_Model;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Http;

namespace Attendance_API.Controllers
{
    //[Authorize]
    public class UsersController : ApiController
    {
        [Route("api/Users/{id}")]
        [HttpGet]
        public async Task<IEnumerable<VW_Attendance_Users>> Get(int id)
        {
            using (EduContext db = new EduContext())
            {
                return await db.VW_Attendance_Users.Where(a => a.SchoolID == id && a.ScheduleID != null).ToListAsync();
            }
        }

        [Route("api/Users/{id}/schedule")]
        [HttpGet]
        public async Task<IEnumerable<Attendance_Schedule_Day>> GetSchedule(int id)
        {
            using (EduContext db = new EduContext())
            {
                var Day = DateTime.Now.ToString("dddd");
                return await db.Attendance_Schedule_Days.Where(a => a.SchoolID == id && a.Day == Day).ToListAsync();
            }
        }

        [Route("api/Users/{id}/leave")]
        [HttpGet]
        public async Task<IEnumerable<LeaveVM>> AttendanceLeave(int id)
        {
            var today = DateTime.Today.ToShortDateString();
            using (EduContext db = new EduContext())
            {
                return await db.Attendance_User_Leaves.Where(a => a.SchoolID == id && a.StartDate <= DateTime.Today && a.EndDate >= DateTime.Today)
                    .Select(a => new LeaveVM() { DeviceID = a.DeviceID, LeaveDate = today }).Distinct().ToListAsync();
            }
        }


        [Route("api/Users/{id}/updateInfo")]
        [HttpGet]
        public async Task<IEnumerable<DataUpdateList_VM>> UpdateInfo(int id)
        {
            using (EduContext db = new EduContext())
            {
                var dataUpdateLists = await db.Attendance_Device_DataUpdateLists.Where(a => a.SchoolID == id).ToListAsync();

                db.Attendance_Device_DataUpdateLists.RemoveRange(dataUpdateLists);
                db.SaveChanges();

                return dataUpdateLists.Select(d => new DataUpdateList_VM()
                {
                    UpdateType = d.UpdateType,
                    UpdateDescription = d.UpdateDescription,
                    UpdateDate = d.UpdateDate.ToShortDateString()
                });
            }
        }

        [Route("api/Users/{id}/FingerPrint")]
        [HttpGet]
        public async Task<IEnumerable<User_FingerPrintVM>> GetFP(int id)
        {
            using (EduContext db = new EduContext())
            {
                return await db.Device_Finger_Print_Records
                .Where(a => a.SchoolID == id)
                .Select(a => new User_FingerPrintVM
                {
                    DeviceID = a.DeviceID,
                    Finger_Index = a.Finger_Index,
                    Flag = a.Flag,
                    Temp_Data = a.Temp_Data
                }).ToListAsync();
            }
        }
    }
}
