using CurrencyRateService.Models;
using Microsoft.EntityFrameworkCore;

namespace CurrencyRateService.Data;

public class CurrencyRateRepo : ICurrencyRateRepo
{
    private readonly AppDBContext _context;

    public CurrencyRateRepo(AppDBContext context)
    {
        _context = context;
    }

    public async Task CreateRate(CurrencyRate rate)
    {
        await _context.CurrencyRates.AddAsync(rate);
    }

    public async Task<IEnumerable<CurrencyRate>> GetAllRates()
    {
        return await _context.CurrencyRates.ToListAsync();
    }

    public async Task<CurrencyRate?> GetRateByCurrencyCode(string CurrencyCode)
    {
        return await _context.CurrencyRates
            .Where(x => x.CurrencyCode.Equals(CurrencyCode, StringComparison.CurrentCultureIgnoreCase))
            .FirstOrDefaultAsync();
    }

    public async Task<bool> SaveChanges()
    {
        return await _context.SaveChangesAsync() > 0;
    }

    public void UpdateRate(CurrencyRate rate)
    {
        _context.CurrencyRates.Update(rate);
    }

    public async Task UpsertRate(CurrencyRate rate)
    {
        var existingRate = await GetRateByCurrencyCode(rate.CurrencyCode);
        if (existingRate == null)
        {
            await CreateRate(rate);
        }
        else
        {
            if (rate.RateToUSD != existingRate.RateToUSD)
            {
                existingRate.CurrencyName = rate.CurrencyName;
                existingRate.RateToUSD = rate.RateToUSD;
                existingRate.UpdatedAt = rate.UpdatedAt;
                existingRate.NextUpdateAt = rate.NextUpdateAt;
                UpdateRate(existingRate);
            }
        }
    }
}