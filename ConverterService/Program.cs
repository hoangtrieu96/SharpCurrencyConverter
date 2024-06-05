using ConverterService.Data;
using ConverterService.Services.SyncDataServices;
using Microsoft.OpenApi.Models;
using StackExchange.Redis;

var builder = WebApplication.CreateBuilder(args);
{
    builder.Services.AddAutoMapper(typeof(Program));
    builder.Services.AddScoped<ICurrencyRateClient, CurrencyRateClient>();
    builder.Services.AddGrpc().AddJsonTranscoding();
    builder.Services.AddGrpcReflection();
    builder.Services.AddGrpcSwagger();
    builder.Services.AddSwaggerGen(conf => 
    {
        conf.SwaggerDoc("v1", new OpenApiInfo { Title = "Sharp Currency Converter API", Version = "v1" });
    });
    builder.Services.AddSingleton<IConnectionMultiplexer>(ConnectionMultiplexer.Connect(builder.Configuration["Redis:ConnectionString"]!));
    builder.Services.AddScoped(typeof(ICacheService<>), typeof(CacheServiceRedis<>));
}

var app = builder.Build();
{
    app.UseSwagger();
    app.UseSwaggerUI(conf => 
    {
        conf.SwaggerEndpoint("/swagger/v1/swagger.json", "Sharp Currency Converter API v1");
    });
    app.MapGrpcService<GrpcConverterService>();
    app.MapGrpcReflectionService();
    app.Run();
}
