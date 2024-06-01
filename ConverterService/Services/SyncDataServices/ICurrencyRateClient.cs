using CurrencyRateService;
using CurrencyRateService.Models;

namespace ConverterService.Services.SyncDataServices;

public interface ICurrencyRateClient
{
    Task<IEnumerable<CurrencyRate>> GetRateFromTo(RateFromToRequest request);
}