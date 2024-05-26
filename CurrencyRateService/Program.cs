using CurrencyRateService.Data;
using CurrencyRateService.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);
{
    builder.Services.AddDbContext<AppDBContext>(opt =>
        opt.UseSqlServer(builder.Configuration["ConnectionStrings:CurrencyRateConnection"])
    );
    builder.Services.AddScoped<ICurrencyRateRepo, CurrencyRateRepo>();
    builder.Services.AddHostedService<CurrencyRateFetcher>();
    builder.Services.AddAutoMapper(typeof(Program));
    builder.Services.AddHttpClient();
    builder.Services.AddGrpc();
    builder.Services.AddGrpcReflection();
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen();
}

var app = builder.Build();
{
    app.MapGrpcService<GrpcCurrencyRateService>(); // Default: http(s)://<host>:<port>/<service name>/<method name>
    app.MapGrpcReflectionService();
    app.MapGet("/", () => "Communication with gRPC endpoints must be made through a gRPC client.");

    app.UseSwagger();
    app.UseSwaggerUI();

    MigrateDB.ApplyMigration(app);

    app.Run();
}


