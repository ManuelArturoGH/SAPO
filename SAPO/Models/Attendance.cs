using System;

namespace SAPO.Models
{
    public class Attendance
    {
        public readonly int AttendanceMachineID;
        public readonly int UserID;
        public readonly DateTime AttendanceTime;
        public readonly string AccessMode;
        public readonly string AttendanceStatus;

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