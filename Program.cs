using FinancialAdvisorTelegramBot.Bot;
using FinancialAdvisorTelegramBot.Data;
using FinancialAdvisorTelegramBot.Utils;
using Laraue.EfCoreTriggers.PostgreSql.Extensions;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// database init
builder.Services.AddDbContext<AppDbContext>(
    options => options
        .UseNpgsql(builder.Configuration.GetConnectionString("AppDatabaseConnection"))
        .UsePostgreSqlTriggers());
builder.Services.AddScoped<DbContext>(provider => provider.GetRequiredService<AppDbContext>());
AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

// custom
builder.Services.AutomaticAddCustomRepositoriesFromAssembly();
builder.Services.AutomaticAddCustomServicesFromAssembly();

// telegram
builder.Services.Configure<BotSettings>(builder.Configuration.GetSection("BotSettings"));
builder.Services.AutomaticAddUpdateListenersFromAssembly();
builder.Services.AutomaticAddCommandsFromAssembly();

// main services
builder.Services.AddControllers().AddNewtonsoftJson();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

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
