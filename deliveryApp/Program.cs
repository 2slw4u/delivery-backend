using deliveryApp.Models;
using deliveryApp.Services;
using deliveryApp.Services.ExceptionProcessor;
using deliveryApp.Services.Interfaces;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddDbContext<AppDbContext>(options => options.UseNpgsql(builder.Configuration.GetConnectionString("MainDbConnectionString")));

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = StandardJwtConfiguration.Issuer,
            ValidateAudience = true,
            ValidAudience = StandardJwtConfiguration.Audience,
            ValidateLifetime = true,
            IssuerSigningKey = StandardJwtConfiguration.GenerateSecurityKey(),
            ValidateIssuerSigningKey = true,
        };
    });

builder.Services.AddScoped<IDishService, DishService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseExceptionMiddleware();

app.UseAuthorization();

app.MapControllers();

app.Run();
