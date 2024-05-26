using CurrencyRateService.Data;
using Grpc.Core;

namespace CurrencyRateService.Services;

public class GrpcCurrencyRateService : GrpcCurrencyRate.GrpcCurrencyRateBase
{
    private readonly ICurrencyRateRepo _repository;

    public GrpcCurrencyRateService(ICurrencyRateRepo repository)
    {
        _repository = repository;
    }

    public override async Task<RateFromToResponse> GetRateFromTo(RateFromToRequest request, ServerCallContext context)
    {
        ValidateRateFromToRequest(request);

        var fromCurrencyRate = await _repository.GetRateByCurrencyCode(request.FromCurrencyCode)
                                    ?? throw new RpcException(new Status(StatusCode.NotFound, $"Exchange rate not found for {request.FromCurrencyCode}."));
        var toCurrencyCode = await _repository.GetRateByCurrencyCode(request.ToCurrencyCode)
                                    ?? throw new RpcException(new Status(StatusCode.NotFound, $"Exchange rate not found for {request.ToCurrencyCode}."));
        var response = new RateFromToResponse 
        {
            FromCurrencyCode = fromCurrencyRate.CurrencyCode,
            FromCurrencyName = fromCurrencyRate.CurrencyName,
            FromCurrencyRate = fromCurrencyRate.RateToUSD.ToString(),
            ToCurrencyCode = toCurrencyCode.CurrencyCode,
            ToCurrencyName = toCurrencyCode.CurrencyName,
            ToCurrencyRate = toCurrencyCode.RateToUSD.ToString(),
            UpdatedAtTimestamp = toCurrencyCode.NextUpdateAt
        };
        
        return await Task.FromResult(response);
    }

    private void ValidateRateFromToRequest(RateFromToRequest request)
    {
        if (string.IsNullOrEmpty(request.FromCurrencyCode))
            throw new RpcException(new Status(StatusCode.InvalidArgument, $"From currency code is required."));

        if (string.IsNullOrEmpty(request.ToCurrencyCode))
            throw new RpcException(new Status(StatusCode.InvalidArgument, $"To currency code is required."));
    }
}