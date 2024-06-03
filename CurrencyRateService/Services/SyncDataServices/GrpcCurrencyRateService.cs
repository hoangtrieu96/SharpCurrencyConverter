using AutoMapper;
using CurrencyRateService.Data;
using Grpc.Core;

namespace CurrencyRateService.Services.SyncDataServices;

public class GrpcCurrencyRateService : GrpcCurrencyRate.GrpcCurrencyRateBase
{
    private readonly ICurrencyRateRepo _repository;
    private readonly IMapper _mapper;

    public GrpcCurrencyRateService(ICurrencyRateRepo repository, IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    public override async Task<RateFromToResponse> GetRateFromTo(RateFromToRequest request, ServerCallContext context)
    {
        ValidateRateFromToRequest(request);

        var fromCurrencyRate = await _repository.GetRateByCurrencyCode(request.FromCurrencyCode)
                                    ?? throw new RpcException(new Status(StatusCode.NotFound, $"Exchange rate not found for {request.FromCurrencyCode}."));
        var toCurrencyRate = await _repository.GetRateByCurrencyCode(request.ToCurrencyCode)
                                    ?? throw new RpcException(new Status(StatusCode.NotFound, $"Exchange rate not found for {request.ToCurrencyCode}."));
        
        
        var response = new RateFromToResponse();
        response.Rates.Add(_mapper.Map<GrpcCurrencyRateModel>(fromCurrencyRate));
        response.Rates.Add(_mapper.Map<GrpcCurrencyRateModel>(toCurrencyRate));

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