using ConverterService.Data;
using CurrencyRateService;
using CurrencyRateService.Models;
using Grpc.Core;

namespace ConverterService.Services.SyncDataServices;

public class GrpcConverterService : GrpcConverter.GrpcConverterBase
{
    private readonly ICurrencyRateClient _currencyRateClient;
    private readonly ICacheService<decimal?> _cacheService;

    public GrpcConverterService(ICurrencyRateClient currencyRateClient, ICacheService<decimal?> cacheService)
    {
        _currencyRateClient = currencyRateClient;
        _cacheService = cacheService;
    }

    public override async Task<ConversionResultResponse> GetConversionResult(ConversionResultRequest request, ServerCallContext context)
    {
        ValidateConversionResultRequest(request);

        // Retrieve currency rates from cache first, if not found, fetch from CurrencyRateService
        decimal? fromRate = await _cacheService.GetAsync(request.FromCurrencyCode);
        decimal? toRate = await _cacheService.GetAsync(request.ToCurrencyCode);
        if (!fromRate.HasValue || !toRate.HasValue)
        {
            (fromRate, toRate) = await GetRates(request.FromCurrencyCode, request.ToCurrencyCode);
            
            // Update cache with latest rates
            await _cacheService.SetAsync(request.FromCurrencyCode, fromRate, TimeSpan.FromMinutes(10));
            await _cacheService.SetAsync(request.ToCurrencyCode, toRate, TimeSpan.FromMinutes(10));
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