using ConverterService.Services.SyncDataServices;

var builder = WebApplication.CreateBuilder(args);
{
    builder.Services.AddAutoMapper(typeof(Program));
    builder.Services.AddScoped<ICurrencyRateClient, CurrencyRateClient>();
}

var app = builder.Build();
{
    app.Run();
}
