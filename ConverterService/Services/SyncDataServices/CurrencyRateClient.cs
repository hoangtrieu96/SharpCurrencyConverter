using AutoMapper;
using CurrencyRateService;
using CurrencyRateService.Models;
using Grpc.Core;
using Grpc.Net.Client;

namespace ConverterService.Services.SyncDataServices;

public class CurrencyRateClient : ICurrencyRateClient
{
    private readonly IConfiguration _configuration;
    private readonly IMapper _mapper;

    public CurrencyRateClient(IConfiguration configuration, IMapper mapper)
    {
        _configuration = configuration;
        _mapper = mapper;
    }

    public async Task<IEnumerable<CurrencyRate>> GetRateFromTo(RateFromToRequest request)
    {
        string gRPCAddress = _configuration["GrpcCurrencyRateService"]!;
        var channel = GrpcChannel.ForAddress(gRPCAddress);
        var client = new GrpcCurrencyRate.GrpcCurrencyRateClient(channel);
        IEnumerable<CurrencyRate> result = [];

        try
        {
            var response = await client.GetRateFromToAsync(request);
            result = _mapper.Map<IEnumerable<CurrencyRate>>(response.Rates);
        }
        catch (RpcException rpcException)
        {
            Console.WriteLine($"Failed to get {nameof(GetRateFromTo)}: {rpcException}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to get {nameof(GetRateFromTo)}: {ex}");
        }
        
        return result;
    }
}