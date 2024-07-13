using Microsoft.OpenApi.Models;
using Coravel;
using Coravel.Scheduling.Schedule;
using Api.Contexts;
using Serilog;
using Api.Handlers;
using Api.Processors;
using Api.Tasks;
using Nure.NET.Types;

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
builder.Services.AddScheduler();
builder.Services.AddTransient<UpdateTask>();

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

var provider = app.Services;
provider.UseScheduler(scheduler =>
{
    scheduler.Schedule<UpdateTask>()
    .DailyAtHour(0);
}); 

app.MapGet("/lists/groups", async (HttpContext x) => { return Results.Content(GroupsHandler.GetJson(), "application/json"); })
.WithOpenApi(generatedOperation =>
{
    generatedOperation.Description = "List all groups";
    generatedOperation.Summary = "List all groups";
    return generatedOperation;
})
.Produces<IList<Group>>();

app.MapGet("/lists/teachers", async (HttpContext x) => { return Results.Content(TeachersHandler.GetJson(), "application/json"); })
.WithOpenApi(generatedOperation =>
{
    generatedOperation.Description = "List all teachers";
    generatedOperation.Summary = "List all teachers";
    return generatedOperation;
})
.Produces<IList<Teacher>>();

app.MapGet("/lists/auditories", async (HttpContext x) => { return Results.Content(AuditoriesHandler.GetJson(), "application/json"); })
.WithOpenApi(generatedOperation =>
{
    generatedOperation.Description = "List all auditories";
    generatedOperation.Summary = "List all auditories";
    return generatedOperation;
})
.Produces<IList<Auditory>>();

app.MapGet("schedule/groups/{group}", async (HttpContext x, string group) =>
{
    var group_id = long.Parse(group);
    var start = x.Request.Query.ContainsKey("start") ? long.Parse(x.Request.Query["start"]) : 0;
    var end = x.Request.Query.ContainsKey("end") ? long.Parse(x.Request.Query["end"]) : 0;

    if (start < 0)
        start = 0;
    if (end < 0)
        end = 0;

    return Results.Content(GroupProcessor.GetJson(group_id, start, end), "application/json");
})
.WithOpenApi(generatedOperation =>
{
    generatedOperation.Description = "Get group schedule";
    generatedOperation.Summary = "Get group schedule";
    return generatedOperation;
})
.Produces<IList<Event>>();

app.MapGet("schedule/teachers/{teacher}", async (HttpContext x, string teacher) =>
{
    var group_id = long.Parse(teacher);
    var start = x.Request.Query.ContainsKey("start") ? long.Parse(x.Request.Query["start"]) : 0;
    var end = x.Request.Query.ContainsKey("end") ? long.Parse(x.Request.Query["end"]) : 0;

    if (start < 0)
        start = 0;
    if (end < 0)
        end = 0;

    return Results.Content(TeacherProcessor.GetJson(group_id, start, end), "application/json");
})
.WithOpenApi(generatedOperation =>
{
    generatedOperation.Description = "Get teacher schedule";
    generatedOperation.Summary = "Get teacher schedule";
    return generatedOperation;
})
.Produces<IList<Event>>();

app.MapGet("schedule/auditories/{auditory}", async (HttpContext x, string auditory) =>
{
    var group_id = long.Parse(auditory);
    var start = x.Request.Query.ContainsKey("start") ? long.Parse(x.Request.Query["start"]) : 0;
    var end = x.Request.Query.ContainsKey("end") ? long.Parse(x.Request.Query["end"]) : 0;

    if (start < 0)
        start = 0;
    if (end < 0)
        end = 0;

    return Results.Content(AuditoryProcessor.GetJson(group_id, start, end), "application/json");
})
.WithOpenApi(generatedOperation =>
{
    generatedOperation.Description = "Get auditory schedule";
    generatedOperation.Summary = "Get auditory schedule";
    return generatedOperation;
})
.Produces<IList<Event>>();

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
