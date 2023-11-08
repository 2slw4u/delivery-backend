using deliveryApp.Configurations;
using deliveryApp.Models;
using deliveryApp.Models.GAR;
using deliveryApp.Policies;
using deliveryApp.Services;
using deliveryApp.Services.ExceptionProcessor;
using deliveryApp.Services.Interfaces;
using deliveryApp.Services.QuartzJobs;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Quartz;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddDbContext<AppDbContext>(options => options.UseNpgsql(builder.Configuration.GetConnectionString("MainDbConnectionString")));
builder.Services.AddDbContext<GarDbContext>(options => options.UseNpgsql(builder.Configuration.GetConnectionString("GarDbConnectionString")));

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        In = ParameterLocation.Header,
        Description = "Enter token",
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        BearerFormat = "JWT",
        Scheme = "Bearer"
    });

    options.OperationFilter<SwaggerFilter>();
});

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AuthorizationPolicy", policy => policy.Requirements.Add(new deliveryApp.Policies.AuthorizationPolicy()));
});
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


builder.Services.AddSingleton<IAuthorizationHandler, AuthorizationPolicyHandler>();

builder.Services.AddScoped<IAddressService, AddressService>();
builder.Services.AddScoped<IDishService, DishService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IBasketService, BasketService>();
builder.Services.AddScoped<IOrderService, OrderService>();
builder.Services.AddHttpContextAccessor();

//uses package Quartz.AspNetCore 3.7
//configuration example https://www.quartz-scheduler.net/documentation/quartz-3.x/tutorial/using-quartz.html#configure-program-cs
builder.Services.AddQuartz(q =>
{
    q.UseMicrosoftDependencyInjectionJobFactory();
});
builder.Services.AddQuartzHostedService(q => q.WaitForJobsToComplete = true);
var job = JobBuilder.Create<TokenCleanerJob>()
    .WithIdentity("myJob", "group1")
    .Build();
var trigger = TriggerBuilder.Create()
    .WithIdentity("myTrigger", "group1")
    .StartNow()
    .WithSimpleSchedule(x => x
        .WithIntervalInSeconds(40)
        .RepeatForever())
    .Build();


var app = builder.Build();

//Continuation of Quartz configuration
var schedulerFactory = app.Services.GetRequiredService<ISchedulerFactory>();
var scheduler = await schedulerFactory.GetScheduler();
await scheduler.ScheduleJob(job, trigger);

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
