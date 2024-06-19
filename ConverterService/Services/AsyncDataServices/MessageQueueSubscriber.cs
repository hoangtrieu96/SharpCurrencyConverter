
using System.Text;
using System.Text.Json;
using ConverterService.Data;
using ConverterService.DTOs;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace ConverterService.Services.AsyncDataServices;

public class MessageQueueSubscriber : BackgroundService
{
    private readonly IConfiguration _configuration;
    private readonly IServiceProvider _serviceProvider;
    private readonly string _cacheCurrencyRatesKey = "CurrencyRates";
    private IConnection? _connection;
    private IModel? _channel;
    private string? _queueName;

    public MessageQueueSubscriber(IConfiguration configuration, IServiceProvider serviceProvider)
    {
        _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));

        InitializeRabbitMQ();
    }

    private void InitializeRabbitMQ()
    {
        string exchangeName = _configuration["RabbitMQ:RateUpdateMQ:ExchangeName"]!;
        string routingKey = _configuration["RabbitMQ:RateUpdateMQ:RoutingKey"]!;

        Console.WriteLine("Host", _configuration["RabbitMQ:Host"]);
        Console.WriteLine("Port", _configuration["RabbitMQ:Port"]);

        var factory = new ConnectionFactory
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
            _channel.ExchangeDeclare(exchange: exchangeName, ExchangeType.Direct);
            _queueName = _channel.QueueDeclare().QueueName;
            _channel.QueueBind(queue: _queueName, exchange: exchangeName, routingKey: routingKey);

            Console.WriteLine("Listening to RabbitMQ for currency rate updates...");

            _connection.ConnectionShutdown += RabbitMQ_ConnectionShutdown;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to init RabbitMQ: {ex.Message}");
        }
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        stoppingToken.ThrowIfCancellationRequested();

        var consumer = new EventingBasicConsumer(_channel);

        consumer.Received += async (ModuleHandle, ea) =>
        {
            var body = ea.Body.ToArray();
            var message = Encoding.UTF8.GetString(body);

            Console.WriteLine($"Received message: {message}");

            RateUpdateEventDTO? rateUpdateEvent = JsonSerializer.Deserialize<RateUpdateEventDTO>(message);
            if (rateUpdateEvent == null)
            {
                Console.WriteLine("Failed to deserialize message.");
                return;
            }

            using var scope = _serviceProvider.CreateScope();
            ICacheService<Dictionary<string, decimal>?> cacheService = scope.ServiceProvider.GetRequiredService<ICacheService<Dictionary<string, decimal>?>>();
        
            await cacheService.RemoveAsync(_cacheCurrencyRatesKey);
            Console.WriteLine("Deleted currency rates in cache.");
        };

        _channel.BasicConsume(queue: _queueName, autoAck: true, consumer: consumer);

        return Task.CompletedTask;
    }

    private void RabbitMQ_ConnectionShutdown(object? sender, ShutdownEventArgs e)
    {
        Console.WriteLine("RabbitMQ Connection Shutdown");
    }

    public override void Dispose()
    {
        _channel?.Close();
        _channel?.Dispose();
        _connection?.Close();
        _connection?.Dispose();
        base.Dispose();
    }
}