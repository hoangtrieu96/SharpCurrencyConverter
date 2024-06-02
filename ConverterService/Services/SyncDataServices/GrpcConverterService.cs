using Grpc.Core;

namespace ConverterService.Services.SyncDataServices;

public class GrpcConverterService : GrpcConverter.GrpcConverterBase
{
    public override async Task<ConversionResultResponse> GetConversionResult(ConversionResultRequest request, ServerCallContext context)
    {
        ValidateConversionResultRequest(request);

        var response = new ConversionResultResponse()
        {
            ConvertedAmount = "0",
            ReservedConvertedAmount = "0",
        };

        return await Task.FromResult(response);
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