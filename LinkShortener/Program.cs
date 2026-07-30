using LinkShortener;
using LinkShortener.Repositories;
using LinkShortener.Services;
using Microsoft.EntityFrameworkCore;
using LRU_Cache;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<LinkDbContext>(
    options =>
    {
        options.UseSqlite(builder.Configuration.GetConnectionString(nameof(LinkDbContext)));
    });

builder.Services.AddScoped<LinksRepository>();

builder.Services.AddScoped<LinksService>();
builder.Services.AddSingleton<ICodeGenerator, CodeGenerator>();
builder.Services.AddSingleton<IClock, Clock>();

var cache = new LruCache<string, string>(capacity: 100, TimeSpan.FromMinutes(10));
builder.Services.AddSingleton(cache);

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
