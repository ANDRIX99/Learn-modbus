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

                    // Disconnect from the server
                    client.Disconnect();

                    return Ok(data.ToArray());
                } catch (Exception ex)
                {
                    return BadRequest(ex.Message);
                }
            }
        }

        [HttpGet("read-specific-slave")]
        public IActionResult ReadFromSpecificSlave(string ip, byte slaveId, int startAddress, int count)
        {
            using (var client = new ModbusTcpClient())
            {
                try
                {
                    // Connect to the modbus server
                    client.Connect(ip, ModbusEndianness.BigEndian);

                    // Read holding registers from the specified slave
                    var data = client.ReadHoldingRegisters<short>(slaveId, startAddress, count);

                    // Disconnect from the server
                    client.Disconnect();

                    return Ok(data.ToArray());
                } catch (Exception ex)
                {
                    return BadRequest(ex.Message);
                }
            }
        }

        [HttpGet("read-coils")]
        public IActionResult ReadCoils(string ip, byte slaveId, int startAddress, int count)
        {
            using (var client = new ModbusTcpClient())
            {
                try
                {
                    // Connect to the modbus server
                    client.Connect(ip);

                    // Read coils from the specified slave
                    Span<byte> data = client.ReadCoils(slaveId, startAddress, count);

                    // Parse byte to bool
                    bool[] result = new bool[count];

                    for (int i = 0; i < count; i++)
                    {
                        int byteIndex = i / 8;
                        int bitIndex = i % 8;
                        result[i] = (data[byteIndex] & (1 << bitIndex)) != 0;
                    }

                    // Disconnect from the server
                    client.Disconnect();

                    return Ok(result);
                } catch (Exception ex)
                {
                    return BadRequest(ex.Message);
                }
            }
        }

        // Read coil with known bit position
        [HttpGet("read-coil-bitPosition")]
        public IActionResult ReadCoil(string ip, byte slaveId, int address, int bitPosition)
        {
            using (var client = new ModbusTcpClient())
            {
                try
                {
                    // Connect to the modbus server
                    client.Connect(ip);

                    // Read coil from the specified slave
                    Span<byte> data = client.ReadCoils(slaveId, address, 1);

                    bool result = (data[bitPosition] != 0);

                    // Disconnect from the server
                    client.Disconnect();

                    return Ok(result);
                } catch (Exception ex)
                {
                    return BadRequest(ex.Message);
                }
            }
        }

        [HttpPost("write-holding")]
        // Write a single holding register value to a specific slave device
        public IActionResult WriteRegister(string ip, byte slaveId, int address, short value)
        {
            using (var client = new ModbusTcpClient())
            {
                try
                {
                    // Connect to the Modbus server
                    client.Connect(ip, ModbusEndianness.BigEndian);

                    // Write the value
                    client.WriteSingleRegister(slaveId, address, value);

                    // Disconnect from the server
                    client.Disconnect();

                    return Ok("Value written successfully");
                } catch (Exception ex)
                {
                    return BadRequest(ex.Message);
                }
            }
        }

        [HttpPost("write-coil")]
        // Write a single coil value to a specific slave device
        public IActionResult WriteCoil(string ip, byte slaveId, int address, bool value)
        {
            using (var client = new ModbusTcpClient())
            {
                try
                {
                    // Connect to the Modbus server
                    client.Connect(ip, ModbusEndianness.BigEndian);

                    // Write the coil value
                    client.WriteSingleCoil(slaveId, address, value);

                    // Disconnect from the server
                    client.Disconnect();

                    return Ok("Value written successfully");
                } catch (Exception ex)
                {
                    return BadRequest(ex.Message);
                }
            }
        }
    }
}
