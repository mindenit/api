using System.Reflection;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using Api.Contexts;
using Nure.NET;
using Serilog;
using Api.Handlers;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddCors(options =>
{
    options.AddPolicy(name: "CORS",
        policy =>
        {
            policy.AllowAnyOrigin()
                .AllowAnyHeader()
                .AllowAnyMethod();
        });
});
builder.Host.UseSerilog();
builder.Services.AddEndpointsApiExplorer();

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Debug()
    .WriteTo.Console()
    .WriteTo.File(
        $"Logs/{DateTime.Now.Day}_{DateTime.Now.Month}_{DateTime.Now.Year}-{DateTime.Now.TimeOfDay}.txt",
        rollingInterval: RollingInterval.Infinite)
    .CreateLogger();

Log.Information("Application started!");

builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Version = "v1.0.1",
        Title = "Mindenit API",
        Description = "The NURE schedule API",
        License = new OpenApiLicense
        {
            Name = "License",
            Url = new Uri("https://www.gnu.org/licenses/gpl-3.0.uk.html")
        }
    });
});

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();
app.UseHttpsRedirection();


Task.Run(() =>
{
    while (true)
    {
        using (var context = new Context())
        {
            if (!context.Groups.Any() || DateTime.Now.Hour == 6 && DateTime.Now.Minute == 0 && DateTime.Now.Second == 0)
            {
                GroupsHandler.Update();
            }

            if (!context.Teachers.Any() || DateTime.Now.Hour == 6 && DateTime.Now.Minute == 0 && DateTime.Now.Second == 0)
            {
                TeachersHandler.Update();
            }

            if (!context.Auditories.Any() || DateTime.Now.Hour == 6 && DateTime.Now.Minute == 0 && DateTime.Now.Second == 0)
            {
                AuditoriesHandler.Update();
            }
        }
    }
});

app.MapGet("/lists/groups", async (HttpContext x) => { return Results.Content(GroupsHandler.GetJson(), "application/json"); });

app.MapGet("/lists/teachers", async (HttpContext x) => { return Results.Content(TeachersHandler.GetJson(), "application/json"); });

app.MapGet("/lists/auditories", async (HttpContext x) => { return Results.Content(AuditoriesHandler.GetJson(), "application/json"); });

app.UseCors("CORS");

app.UseSwagger(options =>
{
    options.SerializeAsV2 = true;
});

// change swagger endpoint to /
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Mindenit API");
    c.RoutePrefix = "";
});

app.Run();