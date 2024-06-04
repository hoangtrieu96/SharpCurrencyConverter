
using System.Text.Json;
using AutoMapper;
using CurrencyRateService.Data;
using CurrencyRateService.DTOs;
using CurrencyRateService.Models;
using CurrencyRateService.Utility;

namespace CurrencyRateService.Services.BackgroundServices;

public class CurrencyRateFetcher : BackgroundService
{
    private readonly ILogger<CurrencyRateFetcher> _logger;
    private readonly IConfiguration _configuration;
    private readonly string _currentcyRateUrl;
    private readonly HttpClient _httpClient;
    private readonly IMapper _mapper;
    private readonly IServiceProvider _serviceProvider;

    public CurrencyRateFetcher(
        ILogger<CurrencyRateFetcher> logger,
        IConfiguration configuration,
        HttpClient httpClient,
        IMapper mapper,
        IServiceProvider serviceProvider
    )
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        _currentcyRateUrl = configuration["CurrencyRateEndpoint"] ?? throw new ArgumentNullException("CurrencyRateEndpoint");
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation($"{nameof(CurrencyRateFetcher)} started.");

        while (!stoppingToken.IsCancellationRequested)
        {
            await FetchCurrencyRates();
            await Task.Delay(600000, stoppingToken);
        }

        _logger.LogInformation($"{nameof(CurrencyRateFetcher)} stopped");
    }

    private async Task FetchCurrencyRates()
    {
        _logger.LogInformation($"Fetching rate data from {_currentcyRateUrl} at {DateTime.Now}...");
        try
        {
            var response = await _httpClient.GetAsync(_currentcyRateUrl);
            response.EnsureSuccessStatusCode();

            string responseContent = await response.Content.ReadAsStringAsync();
            ExchangeRateAPIReadDTO? exchangeRateAPIReadDTO = JsonSerializer.Deserialize<ExchangeRateAPIReadDTO>(responseContent)
                                                ?? throw new JsonException("Failed to parse the response.");

            await UpsertCurrencyRates(exchangeRateAPIReadDTO);
        }
        catch (Exception ex)
        {
            _logger.LogError($"Failed to retrieve currency rate data: {ex}");
        }
    }

    private async Task UpsertCurrencyRates(ExchangeRateAPIReadDTO exchangeRateAPIReadDTO)
    {
        using var scope = _serviceProvider.CreateScope();
        ICurrencyRateRepo _currencyRateRepo = scope.ServiceProvider.GetRequiredService<ICurrencyRateRepo>();
        
        foreach (var rate in exchangeRateAPIReadDTO.ConversionRates)
        {
            var currencyName = CurrencyNames.GetCurrencyName(rate.Key);
            if (currencyName != null && currencyName != string.Empty)
            {
                _logger.LogInformation($"{currencyName}: {rate.Value}");

                CurrencyRateWriteDTO rateWriteDTO = new CurrencyRateWriteDTO
                {
                    CurrencyCode = rate.Key.ToUpper(),
                    CurrencyName = currencyName,
                    RateToUSD = rate.Value,
                    UpdatedAt = exchangeRateAPIReadDTO.TimeLastUpdateUnix,
                    NextUpdateAt = exchangeRateAPIReadDTO.TimeNextUpdateUnix
                };

                await _currencyRateRepo.UpsertRate(_mapper.Map<CurrencyRate>(rateWriteDTO));
            }
        }

        await _currencyRateRepo.SaveChanges();
    }
}