using CheckersApi.Engine;
using CheckersApi.Validation;
using Microsoft.Extensions.Caching.Memory;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddMemoryCache(options => options.SizeLimit = 20000);

// DI: KingsRow adapter + cache
builder.Services.AddSingleton<IEngineAdapter>(sp =>
{
    var cfg = sp.GetRequiredService<IConfiguration>();
    var dbPath = cfg["Engine:Databases"] ?? "";
    var adapter = new KingsRowAdapter(dbPath);

    var cache = sp.GetRequiredService<IMemoryCache>();
    return new CachedEngineAdapter(adapter, cache);
});

var app = builder.Build();

app.UseRouting();
app.UseAuthorization();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapControllers();
app.Run();
