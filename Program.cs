using FinancialAdvisorTelegramBot.Bot;
using FinancialAdvisorTelegramBot.Bot.Updates;
using FinancialAdvisorTelegramBot.Data;
using FinancialAdvisorTelegramBot.Services;
using FinancialAdvisorTelegramBot.Utils;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// database init
builder.Services.AddDbContext<AppDbContext>(
    options => options.UseNpgsql(builder.Configuration.GetConnectionString("AppDatabaseConnection")));
builder.Services.AddScoped<DbContext>(provider => provider.GetRequiredService<AppDbContext>());


// telegram
builder.Services.AddSingleton<IBot, Bot>();
builder.Services.Configure<BotSettings>(builder.Configuration.GetSection("BotSettings"));
builder.Services.AddSingleton<ITelegramUpdateListenerContainer, TelegramUpdateListenerContainer>();
builder.Services.AddScoped<ITelegramUpdateDistributor, TelegramUpdateDistributor>();
builder.Services.AutomaticAddUpdateListenersFromAssembly();
builder.Services.AutomaticAddCommandsFromAssembly();


// custom
builder.Services.AddScoped<IUserService, UserService>();


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
