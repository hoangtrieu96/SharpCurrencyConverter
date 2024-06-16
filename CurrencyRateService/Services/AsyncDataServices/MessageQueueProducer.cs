using System.Text;
using System.Text.Json;
using CurrencyRateService.DTOs;
using RabbitMQ.Client;

namespace CurrencyRateService.Services.AsyncDataServices;

public class MessageQueueProducer : IMessageQueueProducer, IDisposable
{
    private readonly IConfiguration _configuration;
    private readonly IConnection? _connection;
    private readonly IModel? _channel;
    private readonly string? _exchangeName;
    private readonly string? _appName;
    private readonly string _rateUpdateEventName = "RateUpdateEvent";

    public MessageQueueProducer(IConfiguration configuration)
    {
        _configuration = configuration;
        _appName = _configuration["AppName"];

        var factory = new ConnectionFactory()
        {
            HostName = _configuration["RabbitMQ:Host"],
            Port = int.Parse(_configuration["RabbitMQ:Port"]!),
            UserName = _configuration["RabbitMQ:User"],
            Password = _configuration["RabbitMQ:Password"]
        };

        try
        {
            _connection = factory.CreateConnection();
            _channel = _connection.CreateModel();

            var _exchangeName = _configuration["RabbitMQ:RateUpdateMQ:ExchangeName"];
            _channel.ExchangeDeclare(exchange: _exchangeName, type: ExchangeType.Direct);
            _connection.ConnectionShutdown += RabbitMQ_ConnectionShutdown;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error connecting to RabbitMQ: {ex.Message}");
        }
    }

    public void Dispose()
    {
        _channel?.Close();
        _channel?.Dispose();
        _connection?.Close();
        _connection?.Dispose();

    }

    public void PublishRateUpdateEvent()
    {
        if (_channel != null)
        {
            var rateUpdateEvent = GenerateRateUpdateEvent();
            var messageBody = JsonSerializer.Serialize(rateUpdateEvent);
            var body = Encoding.UTF8.GetBytes(messageBody);

            var properties = _channel.CreateBasicProperties();
            properties.MessageId = rateUpdateEvent.MessageId;
            properties.Timestamp = new AmqpTimestamp(rateUpdateEvent.Timestamp);
            properties.AppId = rateUpdateEvent.Source;

            var routingKey = _configuration["RabbitMQ:RateUpdateMQ:RoutingKey"];

            _channel.BasicPublish(exchange: _exchangeName,
                                    routingKey: routingKey,
                                    basicProperties: properties,
                                    body: body);

            Console.WriteLine($"Rate Update Event Sent: {messageBody}");
        }
    }

    private RateUpdateEventDTO GenerateRateUpdateEvent()
    {
        var rateUpdateEvent = new RateUpdateEventDTO
        {
            MessageId = Guid.NewGuid().ToString(),
            Event = _rateUpdateEventName,
            Timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
            Source = _appName!,
        };

        return rateUpdateEvent;
    }

    private void RabbitMQ_ConnectionShutdown(object? sender, ShutdownEventArgs e)
    {
        Console.WriteLine("RabbitMQ connection shutdown");
    }
}