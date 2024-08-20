using Microsoft.OpenApi.Models;
using Coravel;
using Coravel.Scheduling.Schedule;
using Api.Contexts;
using Serilog;
using Api.Handlers;
using Api.Processors;
using Api.Tasks;
using Nure.NET.Types;
using Discord.Webhook;

var builder = WebApplication.CreateBuilder(args);
var allowAny = "_any";
builder.Services.AddCors(options =>
{
    options.AddPolicy(name: allowAny,
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

try
{
    var client = new DiscordWebhookClient(Environment.GetEnvironmentVariable("DISCORD_WEBHOOK_URL"));
    await client.SendMessageAsync("Application started!");
}
catch (Exception e)
{
    Log.Error(e, "Error while sending message to discord");
}

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

using (var context = new Context())
{
    if(!context.Groups.Any())
    {
        Log.Information("No groups found, updating...");
        GroupsHandler.Update();
    }
    if (!context.Teachers.Any())
    {
        Log.Information("No teachers found, updating...");
        TeachersHandler.Update();
    }
    if (!context.Auditories.Any())
    {
        Log.Information("No auditories found, updating...");
        AuditoriesHandler.Update();
    }
}

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();
app.UseHttpsRedirection();

var provider = app.Services;
provider.UseScheduler(scheduler =>
{
    scheduler.Schedule<UpdateTask>()
    .DailyAtHour(0)
    .PreventOverlapping("UpdateTask");
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

app.MapGet("schedule/groups/{id}", async (HttpContext x, long id) =>
{
    var group_id = id;
    var start = x.Request.Query.ContainsKey("start") ? long.Parse(x.Request.Query["start"]) : 0;
    var end = x.Request.Query.ContainsKey("end") ? long.Parse(x.Request.Query["end"]) : 0;

    if (start < 0)
        start = 0;
    if (end < 0)
        end = 0;
        
    if(start > end)
    {
        return Results.BadRequest("Start time must be less than end time");
    }
    
    if(GroupsHandler.IsExist(group_id))
    {
        return Results.Content(GroupProcessor.GetJson(group_id, start, end), "application/json");    
    }
    else
    {
        return Results.NotFound($"Group with id {group_id} not found");
    }
})
.WithOpenApi(generatedOperation =>
{
    generatedOperation.Description = "Get group schedule";
    generatedOperation.Summary = "Get group schedule";
    generatedOperation.Parameters.Add(new OpenApiParameter
    {
        Name = "start",
        In = ParameterLocation.Query,
        Required = false,
        Description = "Start time, in unix timestamp",
        Schema = new OpenApiSchema
        {
            Type = "integer"
        }
    });
    generatedOperation.Parameters.Add(new OpenApiParameter
    {
        Name = "end",
        In = ParameterLocation.Query,
        Required = false,
        Description = "End time, in unix timestamp",
        Schema = new OpenApiSchema
        {
            Type = "integer"
        }
    });
    return generatedOperation;
})
.Produces<IList<Event>>();

app.MapGet("schedule/teachers/{id}", async (HttpContext x, long id) =>
{
    var teacher_id = id;
    var start = x.Request.Query.ContainsKey("start") ? long.Parse(x.Request.Query["start"]) : 0;
    var end = x.Request.Query.ContainsKey("end") ? long.Parse(x.Request.Query["end"]) : 0;

    if (start < 0)
        start = 0;
    if (end < 0)
        end = 0;
        
    if(start > end)
    {
        return Results.BadRequest("Start time must be less than end time");
    }

    if (TeachersHandler.IsExist(teacher_id))
    {
        return Results.Content(TeacherProcessor.GetJson(teacher_id, start, end), "application/json");
    }
    else
    {
        return Results.NotFound($"Teacher with id {teacher_id} not found");
    }
})
.WithOpenApi(generatedOperation =>
{
    generatedOperation.Description = "Get teacher schedule";
    generatedOperation.Summary = "Get teacher schedule";
    generatedOperation.Parameters.Add(new OpenApiParameter
    {
        Name = "start",
        In = ParameterLocation.Query,
        Required = false,
        Description = "Start time, in unix timestamp",
        Schema = new OpenApiSchema
        {
            Type = "integer"
        }
    });
    generatedOperation.Parameters.Add(new OpenApiParameter
    {
        Name = "end",
        In = ParameterLocation.Query,
        Required = false,
        Description = "End time, in unix timestamp",
        Schema = new OpenApiSchema
        {
            Type = "integer"
        }
    });
    return generatedOperation;
})
.Produces<IList<Event>>();

app.MapGet("schedule/auditories/{id}", async (HttpContext x, long id) =>
{
    var auditory_id = id;
    var start = x.Request.Query.ContainsKey("start") ? long.Parse(x.Request.Query["start"]) : 0;
    var end = x.Request.Query.ContainsKey("end") ? long.Parse(x.Request.Query["end"]) : 0;

    if (start < 0)
        start = 0;
    if (end < 0)
        end = 0;
        
    if(start > end)
    {
        return Results.BadRequest("Start time must be less than end time");
    }

    if (AuditoriesHandler.IsExist(auditory_id))
    {
        return Results.Content(AuditoryProcessor.GetJson(auditory_id, start, end), "application/json");
    }
    else
    {
        return Results.NotFound($"Auditory with id {auditory_id} not found");
    }
})
.WithOpenApi(generatedOperation =>
{
    generatedOperation.Description = "Get auditory schedule";
    generatedOperation.Summary = "Get auditory schedule";
    generatedOperation.Parameters.Add(new OpenApiParameter
    {
        Name = "start",
        In = ParameterLocation.Query,
        Required = false,
        Description = "Start time, in unix timestamp",
        Schema = new OpenApiSchema
        {
            Type = "integer"
        }
    });
    generatedOperation.Parameters.Add(new OpenApiParameter
    {
        Name = "end",
        In = ParameterLocation.Query,
        Required = false,
        Description = "End time, in unix timestamp",
        Schema = new OpenApiSchema
        {
            Type = "integer"
        }
    });
    return generatedOperation;
})
.Produces<IList<Event>>();

app.UseCors(allowAny);

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
