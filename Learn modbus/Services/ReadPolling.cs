using FluentModbus;

namespace Learn_modbus.Services
{
    public class ReadPolling : BackgroundService
    {
        private readonly ILogger<ReadPolling> _logger;
        private readonly string ip = "127.0.0.1";
        private readonly byte _slaveId;
        private readonly int _startAddress;
        private readonly int _count;

        public ReadPolling(ILogger<ReadPolling> logger, byte slaveId, int startAddress, int count)
        {
            _logger = logger;
            _slaveId = slaveId;
            _startAddress = startAddress;
            _count = count;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await ReadFromSlave(_slaveId, _startAddress, _count);
                } catch (Exception ex)
                {
                    _logger.LogError(ex.Message);
                }

                // Poll every 5 seconds
                await Task.Delay(5000, stoppingToken);
            }
        }

        private async Task ReadFromSlave(byte slaveId, int startAddress, int count)
        {
            using (var client = new ModbusTcpClient())
            {
                try
                {
                    // Connect to the modbus server
                    client.Connect(ip, ModbusEndianness.BigEndian);

                    var data = await client.ReadHoldingRegistersAsync<short>(slaveId, startAddress, count);

                    // Log the read data
                    var value = data.ToArray();
                    string values = string.Join(", ", value);

                    _logger.LogInformation($"Read from slave {slaveId}: {values}");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex.Message);
                } finally
                {
                    client.Disconnect();
                }
            }
        }
    }
}
