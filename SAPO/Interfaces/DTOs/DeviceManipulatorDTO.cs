using System.Collections.Generic;
using SAPO.Models;
using SAPO.utilities;

namespace SAPO.Interfaces.DTOs
{
    public interface DeviceManipulatorDTO
    {
        bool PingDevice(string ipAddress);
        bool ConnectDevice(string ipAddress , int port);
        
        ICollection<Employees> GetUserInfo(int machineNumber, string ipAddress, int port);
        
        ICollection<Attendance> GetAttendance(int machineNumber, string ipAddress, int port);
        
        
    }
}