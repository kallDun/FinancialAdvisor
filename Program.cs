using FinancialAdvisorTelegramBot.Data;
using FinancialAdvisorTelegramBot.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// database init
builder.Services.AddDbContext<AppDbContext>(
    options => options.UseNpgsql(builder.Configuration.GetConnectionString("AppDatabaseConnection")));
builder.Services.AddScoped<DbContext>(provider => provider.GetRequiredService<AppDbContext>());

// custom services
builder.Services.AddScoped<IUserService, UserService>();

// main services
builder.Services.AddControllers();
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
