using System;

namespace SAPO.Models
{
    public class Attendance
    {
        public int AttendanceMachineID { get; set; }
        public int UserID { get; set; }
        public DateTime AttendanceTime { get; set; }
        public string AccessMode { get; set; } = "";
        public string AttendanceStatus { get; set; } = "";

        public Attendance() { } // Necesario para la deserialización si se usa más adelante

        public Attendance(int attendanceMachineId, int userID, DateTime attendanceTime, string accessMode, string attendanceStatus)
        {
            AttendanceMachineID = attendanceMachineId;
            UserID = userID;
            AttendanceTime = attendanceTime;
            AccessMode = accessMode;
            AttendanceStatus = attendanceStatus;
        }
    }
}