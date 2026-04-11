using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Exceptions;

namespace EduScoring.Common.Messaging;

public class RabbitMQService : IRabbitMQService
{
    private readonly string _hostName;
    private readonly ILogger<RabbitMQService> _logger;

    public RabbitMQService(IConfiguration config, ILogger<RabbitMQService> logger)
    {
        _hostName = config["RabbitMQ:Host"] ?? "localhost";
        _logger = logger;

        if (_hostName == "localhost")
            _logger.LogWarning("[RabbitMQ] Đang dùng host mặc định 'localhost'. Kiểm tra RabbitMQ:Host trong config.");
    }

    public async Task PublishAsync<T>(string queueName, T message)
    {
        // ── Validate input
        if (string.IsNullOrWhiteSpace(queueName))
        {
            _logger.LogError("[RabbitMQ] queueName không được để trống.");
            throw new ArgumentException("queueName không được để trống.", nameof(queueName));
        }

        if (message is null)
        {
            _logger.LogError("[RabbitMQ] Message gửi tới queue '{Queue}' là null.", queueName);
            throw new ArgumentNullException(nameof(message));
        }

        _logger.LogDebug("[RabbitMQ] Bắt đầu publish tới queue '{Queue}' | Host={Host}", queueName, _hostName);

        try
        {
            // ── 1. Kết nối
            var factory = new ConnectionFactory { HostName = _hostName };
            await using var connection = await factory.CreateConnectionAsync();
            await using var channel = await connection.CreateChannelAsync();

            _logger.LogDebug("[RabbitMQ] Kết nối thành công tới '{Host}'.", _hostName);

            // ── 2. Khai báo queue ─────────────────────────────────────────────
            await channel.QueueDeclareAsync(
                queue: queueName,
                durable: true,
                exclusive: false,
                autoDelete: false,
                arguments: null);

            _logger.LogDebug("[RabbitMQ] Queue '{Queue}' đã sẵn sàng.", queueName);

            // ── 3. Serialize
            var jsonMessage = JsonSerializer.Serialize(message);
            var body = Encoding.UTF8.GetBytes(jsonMessage);

            _logger.LogDebug("[RabbitMQ] Payload size={Size} bytes | Queue='{Queue}'", body.Length, queueName);

            // ── 4. Publish
            await channel.BasicPublishAsync(
                exchange: string.Empty,
                routingKey: queueName,
                mandatory: false,
                basicProperties: new BasicProperties { Persistent = true },
                body: body);

            _logger.LogInformation("[RabbitMQ] Publish thành công | Queue='{Queue}' | Type={Type}",
                queueName, typeof(T).Name);
        }
        catch (BrokerUnreachableException ex)
        {
            _logger.LogError(ex, "[RabbitMQ] Không thể kết nối tới broker '{Host}'. Kiểm tra RabbitMQ có đang chạy không.", _hostName);
            throw new InvalidOperationException($"Không thể kết nối RabbitMQ tại '{_hostName}'.", ex);
        }
        catch (OperationInterruptedException ex)
        {
            _logger.LogError(ex, "[RabbitMQ] Thao tác bị gián đoạn khi publish tới queue '{Queue}'.", queueName);
            throw new InvalidOperationException($"Thao tác RabbitMQ bị gián đoạn tại queue '{queueName}'.", ex);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[RabbitMQ] Lỗi không xác định khi publish tới queue '{Queue}'.", queueName);
            throw new InvalidOperationException($"Lỗi khi publish message tới queue '{queueName}'.", ex);
        }
    }
}