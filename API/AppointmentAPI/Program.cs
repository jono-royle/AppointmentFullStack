using AppointmentAPI.Data;
using AppointmentAPI.Repositories;
using AppointmentAPI.Services;
using Microsoft.OpenApi.Models;
using System;
using System.Reflection;
using Microsoft.EntityFrameworkCore;
using AppointmentAPI.Properties;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Appointment API",
        Version = "v1",
        Description = "API for ingesting and retrieving appointments"
    });
    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    options.IncludeXmlComments(xmlPath);
});
builder.Services.AddScoped<IAppointmentIngestionService, AppointmentIngestionService>();

//Uncomment this line and comment out the two lines below to switch to an in memory dictionary
//builder.Services.AddSingleton<IAppointmentRepository, DictionaryAppointmentRepository>();

builder.Services.AddDbContext<AppointmentDbContext>(options => options.UseInMemoryDatabase("AppointmentsDb"));
builder.Services.AddScoped<IAppointmentRepository, EfCoreAppointmentRepository>();
builder.Services.Configure<AppointmentIngestionOptions>(builder.Configuration.GetSection("AppointmentIngestion"));
builder.Services
    .AddOptions<AppointmentIngestionOptions>()
    .Bind(builder.Configuration.GetSection("AppointmentIngestion"))
    .ValidateDataAnnotations()
    .Validate(
        o => o.FutureAppointmentTimeThresholdMinutes >= 0,
        "Future appointment time threshold must be greater than or equal to zero")
    .Validate(o => o.DefaultServiceDurationMinutes > 0,
    "Default service duration must be greater than 0");

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
