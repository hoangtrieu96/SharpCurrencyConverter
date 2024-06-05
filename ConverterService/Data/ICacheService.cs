namespace ConverterService.Data;

public interface ICacheService<T>
{
    Task<T?> GetAsync(string key);
    Task SetAsync(string key, T value, TimeSpan? expiry = null);
    Task RemoveAsync(string key);
}