using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BioMetrixCore.DTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SAPO.Services;

namespace SAPO.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DeviceManipulatorController : ControllerBase
    {
        private DeviceManipulatorDTO deviceManipulatorDTO;
        private readonly ILogger<DeviceManipulatorController> _logger;
        private readonly IConfiguration _config;
        
        public DeviceManipulatorController(ILogger<DeviceManipulatorController> logger, IConfiguration config)
        {
            _logger = logger;
            _config = config;
            deviceManipulatorDTO = new DeviceManipulatorService(_logger, _config);
        }
        [HttpGet]
        public ActionResult<IEnumerable<string>> Get()
        {
            return new string[] { "value1", "value2" };
        }
        
        [HttpGet("{id}")]
        public ActionResult<string> Get(int id)
        {
            return "value";
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
                _logger.LogInformation("Pinging " + value.Address);
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
            try
            {
                //_logger.LogInformation("Connecting to " + value.Address);
                deviceManipulatorDTO.ConnectDevice(value.Address);
                return Ok("Connected to " + value.Address);
            }
            catch (Exception e)
            {
                _logger.LogError(e.Message);
                return BadRequest("Could not connect to " + value.Address);
            }
            
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
