using CurrencyRateService.Models;

namespace CurrencyRateService.Data;

public interface ICurrencyRateRepo
{
    Task<bool> SaveChanges();

    Task<IEnumerable<CurrencyRate>> GetAllRates();
    Task<CurrencyRate?> GetRateByCurrencyCode(string CurrencyCode);
    Task CreateRate(CurrencyRate rate);
    void UpdateRate(CurrencyRate rate);
    Task UpsertRate(CurrencyRate rate);
}