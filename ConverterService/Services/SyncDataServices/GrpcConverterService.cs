using CurrencyRateService;
using Grpc.Core;

namespace ConverterService.Services.SyncDataServices;

public class GrpcConverterService : GrpcConverter.GrpcConverterBase
{
    private readonly ICurrencyRateClient _currencyRateClient;

    public GrpcConverterService(ICurrencyRateClient currencyRateClient)
    {
        _currencyRateClient = currencyRateClient;
    }

    public override async Task<ConversionResultResponse> GetConversionResult(ConversionResultRequest request, ServerCallContext context)
    {
        ValidateConversionResultRequest(request);

        // Retrieve currency rates
        (decimal fromRate, decimal toRate) = await GetRates(request.FromCurrencyCode, request.ToCurrencyCode);

        // Convert amount
        decimal conversionRate = toRate / fromRate;
        decimal reversedConversionRate = fromRate / toRate;
        decimal amount = decimal.Parse(request.Amount);

        var response = new ConversionResultResponse()
        {
            ConvertedAmount = (amount * conversionRate).ToString("F2"),
            ReservedConvertedAmount = (amount * reversedConversionRate).ToString("F2")
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