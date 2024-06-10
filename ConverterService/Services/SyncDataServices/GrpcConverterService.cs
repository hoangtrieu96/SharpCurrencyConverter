using ConverterService.Data;
using CurrencyRateService;
using CurrencyRateService.Models;
using Grpc.Core;

namespace ConverterService.Services.SyncDataServices;

public class GrpcConverterService : GrpcConverter.GrpcConverterBase
{
    private readonly ICurrencyRateClient _currencyRateClient;
    private readonly ICacheService<Dictionary<string, decimal>?> _cacheService;
    private readonly string _cacheCurrencyRatesKey = "CurrencyRates";

    public GrpcConverterService(ICurrencyRateClient currencyRateClient, ICacheService<Dictionary<string, decimal>?> cacheService)
    {
        _currencyRateClient = currencyRateClient;
        _cacheService = cacheService;
    }

    public override async Task<ConversionResultResponse> GetConversionResult(ConversionResultRequest request, ServerCallContext context)
    {
        ValidateConversionResultRequest(request);

        // Retrieve currency rates from cache first, if not found, fetch from CurrencyRateService
        decimal? fromRate = null;
        decimal? toRate = null;
        string fromCurrencyCode = request.FromCurrencyCode.ToUpper();
        string toCurrencyCode = request.ToCurrencyCode.ToUpper();
        
        Dictionary<string, decimal>? currencyRates = await _cacheService.GetAsync(_cacheCurrencyRatesKey); 
        if (currencyRates != null)
        {
            if (currencyRates.ContainsKey(fromCurrencyCode) && currencyRates.ContainsKey(toCurrencyCode))
            {
                fromRate = currencyRates[fromCurrencyCode];
                toRate = currencyRates[toCurrencyCode];
            }
        } 
        else
        {
            currencyRates = [];
        }

        // Get rates from CurrencyRateService if not found in cache
        if (!fromRate.HasValue || !toRate.HasValue)
        {
            (fromRate, toRate) = await GetRates(fromCurrencyCode, toCurrencyCode);
            
            // Update cache with latest rates
            currencyRates[fromCurrencyCode] = fromRate.Value;
            currencyRates[toCurrencyCode] = toRate.Value;
            await _cacheService.SetAsync(_cacheCurrencyRatesKey, currencyRates, TimeSpan.FromMinutes(10));
        }

        // Convert amount
        decimal conversionRate = toRate.Value / fromRate.Value;
        decimal reversedConversionRate = fromRate.Value / toRate.Value;
        decimal amount = decimal.Parse(request.Amount);

        var response = new ConversionResultResponse()
        {
            ConvertedAmount = (amount * conversionRate).ToString("F6"),
            ReservedConvertedAmount = (amount * reversedConversionRate).ToString("F6")
        };

        return response;
    }

    private async Task<(decimal fromRate, decimal toRate)> GetRates(string fromCurrencyCode, string toCurrencyCode)
    {
        var rateFromToRequest = new RateFromToRequest()
        {
            FromCurrencyCode = fromCurrencyCode,
            ToCurrencyCode = toCurrencyCode
        };

        var rateFromToResponse = await _currencyRateClient.GetRateFromTo(rateFromToRequest) 
                        ?? throw new RpcException(new Status(StatusCode.NotFound, $"Currency rate not found."));
        var rates = rateFromToResponse.ToArray();

        if (rates.Length != 2)
            throw new RpcException(new Status(StatusCode.DataLoss, $"Currency rates were missing."));

        decimal? fromRate = rates.Where(r => r.CurrencyCode == fromCurrencyCode).FirstOrDefault()?.RateToUSD;
        decimal? toRate = rates.Where(r => r.CurrencyCode == toCurrencyCode).FirstOrDefault()?.RateToUSD;

        if (!fromRate.HasValue || !toRate.HasValue)
            throw new RpcException(new Status(StatusCode.NotFound, $"Currency rate not found."));

        return (fromRate.Value, toRate.Value);
    }

    private void ValidateConversionResultRequest(ConversionResultRequest request)
    {
        if (string.IsNullOrEmpty(request.FromCurrencyCode))
            throw new RpcException(new Status(StatusCode.InvalidArgument, $"From currency code is required."));

        if (string.IsNullOrEmpty(request.ToCurrencyCode))
            throw new RpcException(new Status(StatusCode.InvalidArgument, $"To currency code is required."));

        if (string.IsNullOrEmpty(request.Amount))
            throw new RpcException(new Status(StatusCode.InvalidArgument, $"Conversion amount is required."));
    }
}