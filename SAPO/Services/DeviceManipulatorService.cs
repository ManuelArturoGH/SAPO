using System;
using BioMetrixCore.DTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SAPO.Controllers;
using SAPO.utilities;
using SAPO.EnvironmentVar;

namespace SAPO.Services
{
    public class DeviceManipulatorService : DeviceManipulatorDTO
    {
        private ZkemClient zkemClient;
        private readonly IConfiguration _config;
        private readonly EnvironmentVars _envVars;
        private ILogger<DeviceManipulatorController> _logger;
        
        public DeviceManipulatorService(ILogger<DeviceManipulatorController> logger, IConfiguration config)
        {
            _config = config;
            _logger = logger;
            _envVars = new EnvironmentVars(_config);
        }
        
        public bool PingDevice(string ipAdd)
        {
            return  UniversalStatic.PingTheDevice(ipAdd);
        }
        
        public bool ConnectDevice(string ipAdd)
        {
            
            zkemClient = new ZkemClient(RaiseDeviceEvent, _logger);
            return zkemClient.Connect_Net(ipAdd.ToString(), GetEnvVar());
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

        private int GetEnvVar()
        {
            _logger.LogDebug("GetEnvVar" + _envVars.Port);
            return int.Parse(_envVars.Port ?? "4370");
        }
    }
}
