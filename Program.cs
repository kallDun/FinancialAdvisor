using FinancialAdvisorTelegramBot.Bot;
using FinancialAdvisorTelegramBot.Data;
using FinancialAdvisorTelegramBot.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// database init
builder.Services.AddDbContext<AppDbContext>(
    options => options.UseNpgsql(builder.Configuration.GetConnectionString("AppDatabaseConnection")));
builder.Services.AddScoped<DbContext>(provider => provider.GetRequiredService<AppDbContext>());

// custom
builder.Services.AddSingleton<IBot, Bot>();
builder.Services.AddScoped<IUserService, UserService>();

// main services
builder.Services.AddControllers().AddNewtonsoftJson();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// configures
builder.Services.Configure<BotSettings>(builder.Configuration.GetSection("BotSettings"));

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
