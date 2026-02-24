using FluentModbus;
using System.Buffers.Binary;
using System.Text.Json;

namespace Learn_modbus.Services
{
    public class Polling : BackgroundService
    {
        private readonly ILogger<Polling> _logger;
        private readonly string _configPath = "config.json";

        public record RegisterConfig(string Tag, string Address, string Type, string DataType, double Scale = 1.0, int BitPosition = 0);
        public record PollingConfig(string IpAddress, List<RegisterConfig> Registers);

        public Polling (ILogger<Polling> logger)
        {
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            // Load configuration from JSON file
            var json = await File.ReadAllTextAsync(_configPath, stoppingToken);
            var config = JsonSerializer.Deserialize<PollingConfig>(json);

            using (var client = new ModbusTcpClient())
            {
                try
                {
                    while (!stoppingToken.IsCancellationRequested)
                    {
                        try
                        {
                            if (!client.IsConnected)
                                client.Connect(config.IpAddress);

                            foreach (var register in config.Registers)
                            {
                                if (register.Type == "HoldingRegister")
                                {
                                    int bytesToRead = (register.DataType == "Float32") ? 4 : 2; // Float32 requires 2 registers (4 bytes), while Int16 requires 1 register (2 bytes)
                                    var data = client.ReadHoldingRegisters<byte>(1, int.Parse(register.Address), bytesToRead);
                                    
                                    if (register.DataType == "Int16")
                                    {
                                        short value = BinaryPrimitives.ReadInt16BigEndian(data);
                                        _logger.LogInformation($"{register.Tag}: {value * register.Scale}");
                                    }

                                    if (register.DataType == "Float32")
                                    {
                                        float value = BinaryPrimitives.ReadSingleBigEndian(data);
                                        _logger.LogInformation($"{register.Tag}: {value * register.Scale}");
                                    }
                                }

                                if (register.Type == "Coil")
                                {
                                    var coil = client.ReadCoils(1, int.Parse(register.Address), 1);

                                    // Calculate bit and byte indexs
                                    int bytePosition = register.BitPosition / 8;
                                    int bitIndex = register.BitPosition % 8;

                                    bool value = (coil[bytePosition] & (1 << bitIndex)) != 0;

                                    _logger.LogInformation($"{register.Tag}: {value}");
                                }
                            }

                            await Task.Delay(5000, stoppingToken); // Poll every 5 seconds
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex.Message);
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex.Message);
                }
            }
        }

        private object ProcessData(Span<short> data, string dataType, double scale)
        {
            return dataType switch
            {
                "Float32" => BitConverter.ToSingle(BitConverter.GetBytes(data[0]).Concat(BitConverter.GetBytes(data[1])).ToArray(), 0) * scale,
                "Int16" => data[0] * scale,
                _ => data[0]
            };
        }
    }
}
