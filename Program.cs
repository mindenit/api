using System.Reflection;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using nure_api;
using Nure.NET;
using Serilog;

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

/*builder.Services.AddDbContext<Context>(options =>
    options.UseNpgsql(File.ReadAllText("dbConnection")));*/


var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();
app.UseHttpsRedirection();

app.MapGet("/lists/groups", async (HttpContext x) => {
    using (var context = new Context())
    {
        context.Groups.AddRange(Cist.GetGroups());
        context.SaveChanges();
    }
    var json = JsonSerializer.Serialize(Cist.GetGroups(), new JsonSerializerOptions { WriteIndented = true });
    return Results.Content(json, "application/json");
});

app.MapGet("/lists/teachers", async (HttpContext x) => {
    var json = JsonSerializer.Serialize(Cist.GetTeachers(), new JsonSerializerOptions { WriteIndented = true });
    return Results.Content(json, "application/json");
});

app.MapGet("/lists/auditories", async (HttpContext x) => {
    var json = JsonSerializer.Serialize(Cist.GetAuditories(), new JsonSerializerOptions { WriteIndented = true });
    return Results.Content(json, "application/json");
});

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