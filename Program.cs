using System.Text.Json;
using Nure.NET;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();

app.MapGet("/lists/groups", async (HttpContext x) => {
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

app.Run();