using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SAPO.Interfaces.DTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SAPO.Interfaces;
using SAPO.Models;
using SAPO.Services;
using SAPO.utilities;


namespace SAPO.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DeviceManipulatorController : ControllerBase
    {
        private DeviceManipulatorDTO deviceManipulatorDTO;
        private readonly ILogger<DeviceManipulatorController> _logger;

        public DeviceManipulatorController(ILogger<DeviceManipulatorController> logger)
        {
            _logger = logger;
            deviceManipulatorDTO = new DeviceManipulatorService(_logger);
        }

        [HttpGet]
        public ActionResult<IEnumerable<string>> Get()
        {
            return new string[] { "value1", "value2" };
        }

        [HttpGet("employees")]
        public ActionResult<IList<Employees>> Get([FromQuery] int machineNumber, 
            [FromQuery] string ipAddress, 
            [FromQuery] int port)
        {
            try
            {
                var users = deviceManipulatorDTO.GetUserInfo(machineNumber, ipAddress, port).ToList();
                var userList = new List<Employees>();
                if (users.Count == 0)
                {
                    return NotFound("No users found on machine number " + machineNumber);
                }

                foreach (var user in users)
                {
                    if (user.GetName() != null)
                    {
                        _logger.LogDebug(user.GetName());
                    }

                    if (userList.Exists(u => u.GetId() == user.GetId()))
                    {
                        _logger.LogDebug("User already in list");
                    }
                    else
                    {
                        userList.Add(user);
                    }
                }

                return Ok(userList);
            }
            catch (Exception e)
            {
                _logger.LogError(e.Message);
                return BadRequest("Could not get users from machine number " + machineNumber);
            }
        }
        
        [HttpGet("attendance")]
        public ActionResult<IList<Attendance>> GetAttendance([FromQuery] int machineNumber, 
            [FromQuery] string ipAddress, 
            [FromQuery] int port)
        {
            try
            {
                var logs = deviceManipulatorDTO.GetAttendance(machineNumber, ipAddress, port).ToList();
                var logList = new List<Attendance>();
                if (logs.Count == 0)
                {
                    return NotFound("No attendance logs found on machine number " + machineNumber);
                }

                foreach (var log in logs)
                {
                    if (logList.Exists(l => l.UserID == log.UserID && l.AttendanceTime == log.AttendanceTime && l.AccessMode == log.AccessMode))
                    {
                        _logger.LogDebug("Log already in list");
                    }
                    else
                    {
                        logList.Add(log);
                    }
                }
                return Ok(logList);
            }
            catch (Exception e)
            {
                _logger.LogError(e.Message);
                return BadRequest("Could not get attendance logs from machine number " + machineNumber);
            }
        }

            [HttpPost]
        public void Post([FromBody] string value)
        {
        }

        [HttpPost("ping")]
        public IActionResult Ping([FromBody] AdressDTO value)
        {
            try
            {
                _logger.LogInformation("Pinging ");
                deviceManipulatorDTO.PingDevice(value.Address);
                return Ok("Pinged" + value.Address);
            }
            catch (Exception e)
            {
                _logger.LogError(e.Message);
                return BadRequest("Could not ping " + value.Address);
            }

        }

        [HttpPost("connect")]
        public IActionResult Connect([FromBody] AdressDTO value)
        {
            _logger.LogDebug("Connecting to " + value.Address);
            try
            {
                //_logger.LogDebug("Connecting to " + value.Address);
                deviceManipulatorDTO.ConnectDevice(value.Address, value.Port);
                return Ok("Connected to " + value.Address);
            }
            catch (Exception e)
            {
                _logger.LogError(e.Message);
                return BadRequest("Could not connect to " + value.Address);
            }

        }

        [HttpGet("ok")]
        public IActionResult OkTest()
        {
            return Ok("The DeviceManipulatorController is up and running.");
        }

    [HttpPut("{id}")]
        public void Put(int id, [FromBody] string value)
        {
        }
        
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }
    }
}
