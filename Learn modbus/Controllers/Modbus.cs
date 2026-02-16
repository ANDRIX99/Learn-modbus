using Microsoft.AspNetCore.Mvc;
using FluentModbus;

namespace Learn_modbus.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class Modbus : Controller
    {
        [HttpGet("read-holding")]
        public IActionResult ReadRegister(string ip, int startAddress, int count)
        {
            using (var client = new ModbusTcpClient())
            {
                try
                {
                    // Connect to the Modbus server
                    client.Connect(ip, ModbusEndianness.BigEndian);

                    // Read holding registers
                    var data = client.ReadHoldingRegisters<short>(1, startAddress, count);

                    return Ok(data.ToArray());
                } catch (Exception ex)
                {
                    return BadRequest(ex.Message);
                }
            }
        }

        [HttpPost("write-holding")]
        public IActionResult WriteRegister(string ip, int address, short value)
        {
            using (var client = new ModbusTcpClient())
            {
                try
                {
                    // Connect to the Modbus server
                    client.Connect(ip, ModbusEndianness.BigEndian);

                    // Write the value
                    client.WriteSingleRegister(1, address, value);

                    return Ok("Value written successfully");
                } catch (Exception ex)
                {
                    return BadRequest(ex.Message);
                }
            }
        }
    }
}
