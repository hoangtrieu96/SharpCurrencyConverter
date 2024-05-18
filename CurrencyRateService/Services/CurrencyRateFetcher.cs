
namespace CurrencyRateService.Services;

public class CurrencyRateFetcher : BackgroundService
{
    private readonly ILogger<CurrencyRateFetcher> _logger;
    private readonly IConfiguration _configuration;
    private readonly string _currentcyRateUrl;

    public CurrencyRateFetcher(ILogger<CurrencyRateFetcher> logger, IConfiguration configuration)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        _currentcyRateUrl = configuration["CurrencyRateEndpoint"] ?? throw new ArgumentNullException("CurrencyRateEndpoint");
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation($"{nameof(CurrencyRateFetcher)} started.");

        while (!stoppingToken.IsCancellationRequested)
        {   
            await FetchCurrencyRate();
            await Task.Delay(2000, stoppingToken);
        }

        _logger.LogInformation($"{nameof(CurrencyRateFetcher)} stopped");
    }

    private async Task FetchCurrencyRate() {
        _logger.LogInformation($"Fetching rate data from {_currentcyRateUrl} at {DateTime.Now}...");
        await Task.Delay(0);
    }
}