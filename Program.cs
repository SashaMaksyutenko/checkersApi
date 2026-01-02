using CheckersApi.Engine;
using CheckersApi.Validation;
using Microsoft.Extensions.Caching.Memory;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// кеш з лімітом на 20к позицій (за тз)
builder.Services.AddMemoryCache(options =>
{
    options.SizeLimit = 20000;
});

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod());
});

// engine pool (якщо chinook)
builder.Services.AddSingleton<ChinookWorkerPool>(sp =>
{
    var cfg = sp.GetRequiredService<IConfiguration>();
    var engineType = cfg["Engine:Type"] ?? "kingsrow";

    if (engineType.Equals("chinook", StringComparison.OrdinalIgnoreCase))
    {
        var exePath = cfg["Engine:Path"] ?? throw new InvalidOperationException("Engine:Path not configured");
        var workers = int.TryParse(cfg["Engine:Workers"], out var w) ? w : 2;
        return new ChinookWorkerPool(exePath, workers);
    }

    return null!;
});

// engine adapter
builder.Services.AddSingleton<IEngineAdapter>(sp =>
{
    var cfg = sp.GetRequiredService<IConfiguration>();
    var engineType = cfg["Engine:Type"] ?? "kingsrow";

    IEngineAdapter adapter;

    if (engineType.Equals("chinook", StringComparison.OrdinalIgnoreCase))
    {
        var pool = sp.GetRequiredService<ChinookWorkerPool>();
        adapter = new ChinookAdapter(pool);
    }
    else
    {
        var dbPath = cfg["Engine:Databases"] ?? "";
        var useInit = bool.TryParse(cfg["Engine:UseInit"], out var flag) && flag;
        adapter = new KingsRowAdapter(dbPath, useInit);
    }

    // обгортаємо в кеш (15 хвилин ttl за тз)
    var cache = sp.GetRequiredService<IMemoryCache>();
    return new CachedEngineAdapter(adapter, cache);
});

var app = builder.Build();

app.UseRouting();
app.UseCors();
app.UseAuthorization();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Checkers API v1");
        c.RoutePrefix = "swagger";
    });
}

app.MapControllers();
app.Run();