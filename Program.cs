using FinancialAdvisorTelegramBot.Bot;
using FinancialAdvisorTelegramBot.Data;
using FinancialAdvisorTelegramBot.Services.Advisor;
using FinancialAdvisorTelegramBot.Services.Background;
using FinancialAdvisorTelegramBot.Utils;
using Laraue.EfCoreTriggers.PostgreSql.Extensions;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// database
builder.Services.AddDbContext<AppDbContext>(
    options => options
        .UseNpgsql(builder.Configuration.GetConnectionString("AppDatabaseConnection"))
        .UsePostgreSqlTriggers());
builder.Services.AddScoped<DbContext>(provider => provider.GetRequiredService<AppDbContext>());
AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

// settings
builder.Services.Configure<BotSettings>(builder.Configuration.GetSection("BotSettings"));
builder.Services.Configure<OpenAiSettings>(builder.Configuration.GetSection("OpenAiSettings"));

// telegram
builder.Services.AutomaticAddUpdateListenersFromAssembly();
builder.Services.AutomaticAddCommandsFromAssembly();

// custom
builder.Services.AutomaticAddCustomRepositoriesFromAssembly();
builder.Services.AutomaticAddCustomServicesFromAssembly();
builder.Services.AddHostedService<BackgroundServicesPool>();

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
