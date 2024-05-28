using CurrencyRateService.Data;
using CurrencyRateService.Models;
using CurrencyRateService.Services;
using FakeItEasy;
using Grpc.Core;

namespace CurrencyRateService.Tests.Services;

public class GrpcCurrencyRateServiceTests
{
    private readonly ICurrencyRateRepo _fakeCurrencyRateRepo;
    private readonly GrpcCurrencyRateService _grpcService;
    private readonly ServerCallContext _serverCallContext;

    public GrpcCurrencyRateServiceTests()
    {
        _fakeCurrencyRateRepo = A.Fake<ICurrencyRateRepo>();
        _grpcService = new GrpcCurrencyRateService(_fakeCurrencyRateRepo);
        _serverCallContext = A.Fake<ServerCallContext>();
    }

    [Fact]
    public async Task GetRateFromTo_ValidRequest_ReturnsCorrectResponse()
    {
        // Arrange
        var fromCurrencyCode = "USD";
        var fromCurrencyName = "United States Dollar";
        var fromRateToUSD = 1.000000m;
        var toCurrencyCode = "AUD";
        var toCurrencyName = "Australian Dollar";
        var toRateToUSD = 1.509700m;
        var updatedAt = 1716249602;
        var nextUpdateAt = 1716336002;

        A.CallTo(() => _fakeCurrencyRateRepo.GetRateByCurrencyCode("USD"))
            .Returns(Task.FromResult<CurrencyRate?>(new CurrencyRate
            {
                Id = 1,
                CurrencyCode = fromCurrencyCode,
                CurrencyName = fromCurrencyName,
                RateToUSD = fromRateToUSD,
                UpdatedAt = updatedAt,
                NextUpdateAt = nextUpdateAt
            })
        );

        A.CallTo(() => _fakeCurrencyRateRepo.GetRateByCurrencyCode("AUD"))
            .Returns(Task.FromResult<CurrencyRate?>(new CurrencyRate
            {
                Id = 2,
                CurrencyCode = toCurrencyCode,
                CurrencyName = toCurrencyName,
                RateToUSD = toRateToUSD,
                UpdatedAt = updatedAt,
                NextUpdateAt = nextUpdateAt
            })
        );

        var request = new RateFromToRequest
        {
            FromCurrencyCode = fromCurrencyCode,
            ToCurrencyCode = toCurrencyCode
        };

        // Act
        var response = await _grpcService.GetRateFromTo(request, _serverCallContext);

        // Assert
        Assert.IsType<RateFromToResponse>(response);
        Assert.Equal(fromCurrencyCode, response.FromCurrencyCode);
        Assert.Equal(fromCurrencyName, response.FromCurrencyName);
        Assert.Equal(fromRateToUSD.ToString(), response.FromCurrencyRate);
        Assert.Equal(toCurrencyCode, response.ToCurrencyCode);
        Assert.Equal(toCurrencyName, response.ToCurrencyName);
        Assert.Equal(toRateToUSD.ToString(), response.ToCurrencyRate);
        Assert.Equal(nextUpdateAt, response.UpdatedAtTimestamp);
    }

    [Fact]
    public async Task GetRateFromTo_MissingFromCurrencyCode_ThrowsRpcException()
    {
        // Arrange
        var request = new RateFromToRequest
        {
            FromCurrencyCode = "",
            ToCurrencyCode = "AUD"
        };

        // Act
        var exception = await Assert.ThrowsAsync<RpcException>(() => _grpcService.GetRateFromTo(request, _serverCallContext));
        
        // Assert
        Assert.Equal(StatusCode.InvalidArgument, exception.StatusCode);
    }

    [Fact]
    public async Task GetRateFromTo_CurrencyCodeNotFound_ThrowsRpcException()
    {
        // Arrange
        var fromCurrencyCode = "Not Found Currency Code";
        var toCurrencyCode = "AUD";

        A.CallTo(() => _fakeCurrencyRateRepo.GetRateByCurrencyCode(fromCurrencyCode))
            .Returns(Task.FromResult<CurrencyRate?>(null));
        
        var request = new RateFromToRequest
        {
            FromCurrencyCode = fromCurrencyCode,
            ToCurrencyCode = toCurrencyCode
        };

        // Act
        var exception = await Assert.ThrowsAsync<RpcException>(() => _grpcService.GetRateFromTo(request, _serverCallContext));

        // Assert
        Assert.Equal(StatusCode.Internal, exception.StatusCode);
    }
}