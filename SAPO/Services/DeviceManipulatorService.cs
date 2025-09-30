using System;
using System.Collections.Generic;
using SAPO.Interfaces.DTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SAPO.Controllers;
using SAPO.Interfaces;
using SAPO.utilities;
using System.Linq;
using SAPO.Models;

namespace SAPO.Services
{
    public class DeviceManipulatorService : DeviceManipulatorDTO
    {
        
        private DeviceManipulator _deviceManipulator;
        private ILogger<DeviceManipulatorController> _logger;
        private ZkemClient zkemClient;
        
        public DeviceManipulatorService(ILogger<DeviceManipulatorController> logger)
        {
            _logger = logger;
            zkemClient = new ZkemClient(_logger, RaiseDeviceEvent);
            
        }
        
        public bool PingDevice(string ipAdd)
        {
            _logger.LogDebug("Pinging device at IP: " + ipAdd);
            return  UniversalStatic.PingTheDevice(ipAdd);
        }
        
        public bool ConnectDevice(string ipAdd, int port)
        {
            _logger.LogDebug("Connecting to device at IP: " + ipAdd + " on port: " + port);
            zkemClient = new ZkemClient(_logger, RaiseDeviceEvent);
            return zkemClient.Connect_Net(ipAdd, port); 
        }
        public ICollection<Employees> GetUserInfo(int machineNumber, string ipAddress, int port)
        {
            
            if (zkemClient != null)
            {
                
                zkemClient.Connect_Net(ipAddress, port);
                
                _deviceManipulator = new DeviceManipulator();

                var result = new List<Employees>();
                var users = _deviceManipulator.GetAllUserInfo(zkemClient, machineNumber, _logger);

                foreach (var user in users)
                {
                    Employees emp = new Employees(user.EnrollNumber.ToString(), user.Name,
                        user.FingerIndex, user.Privelage, user.Enabled, DateTime.Now);
                    result.Add(emp);
                }
                zkemClient.Disconnect();
                return result;
            }
            else
            {
                _logger.LogError("ZkemClient is not initialized.");
                return new List<Employees>();
            }
        }
        
        public ICollection<Attendance> GetAttendance(int machineNumber, string ipAddress, int port)
        {
            if (zkemClient != null)
            {
                
                zkemClient.Connect_Net(ipAddress, port);
                
                _deviceManipulator = new DeviceManipulator();

                var result = new List<Attendance>();
                var logs = _deviceManipulator.GetLogData(zkemClient, machineNumber, _logger).ToList();

                foreach (var log in logs)
                {
                    Attendance attendanceLog = new Attendance(log.MachineNumber, log.IndRegID,
                        DateTime.Parse(log.DateTimeRecord), (log.VerifyMode == 0)? "Huella" : "Clave" , (log.InOutMode == 0)? "Entrada" : "Salida");
                    result.Add(attendanceLog);
                }
                zkemClient.Disconnect();
                return result;
            }
            else
            {
                _logger.LogError("ZkemClient is not initialized.");
                return new List<Attendance>();
            }
        }
        
        private void RaiseDeviceEvent(object sender, string actionType)
        {
            switch (actionType)
            {
                case UniversalStatic.acx_Disconnect:
                {
                    break;
                    
                }
                case "FingerDetected":
                {
                    break;
                }
                default:
                    break;
            }

        }
    }
}
