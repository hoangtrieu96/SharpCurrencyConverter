using CurrencyRateService.Services;

var builder = WebApplication.CreateBuilder(args);
{
    builder.Services.AddControllers();
    builder.Services.AddHostedService<CurrencyRateFetcher>();
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen();
}

var app = builder.Build();
{
    app.MapControllers();
    app.UseSwagger();
    app.UseSwaggerUI();
    app.Run();
}


