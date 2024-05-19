using Microsoft.EntityFrameworkCore;

namespace CurrencyRateService.Data;

public static class MigrateDB
{
    public static void ApplyMigration(IApplicationBuilder app)
    {
        using var serviceScope = app.ApplicationServices.CreateScope();
        var dbContext = serviceScope.ServiceProvider.GetRequiredService<AppDBContext>();
        dbContext.Database.Migrate();
    } 
}