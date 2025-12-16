using AppointmentAPI.Data;
using AppointmentAPI.Repositories;
using AppointmentAPI.Services;
using Microsoft.OpenApi.Models;
using System;
using System.Reflection;
using Microsoft.EntityFrameworkCore;
using AppointmentAPI.Properties;
using AppointmentAPI.Logging;
using AppointmentAPI.Responses;
using Microsoft.AspNetCore.Diagnostics;

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
builder.Services.AddSingleton<IApiLogger, ConsoleLogger>();

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

app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.UseExceptionHandler(errorApp =>
{
    errorApp.Run(async context =>
    {
        var exceptionHandler = context.Features.Get<IExceptionHandlerFeature>();

        var exception = exceptionHandler?.Error;

        var logger = context.RequestServices.GetRequiredService<IApiLogger>();

        logger.Log($"Unhandled exception occurred: {exception}");

        context.Response.StatusCode = StatusCodes.Status500InternalServerError;
        context.Response.ContentType = "application/json";

        var response = new ErrorResponse
        {
            Errors = new List<string> { "An unexpected error occurred." }
        };

        await context.Response.WriteAsJsonAsync(response);
    });
});

app.Run();
