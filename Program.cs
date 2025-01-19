using System;
using System.Linq;
using Nure.NET;
using Nure.NET.Types;
using Sisk.Core.Http;
using System.Threading.Tasks;
using Api.Services;

class Program
{
    static async Task Main(string[] args)
    {
        var groups = Cist.GetGroups();
        var teachers = Cist.GetTeachers();
        var auditories = Cist.GetAuditories();

        var events = Cist.GetEvents(EventType.Group, 10304333);

        var databaseService = new DatabaseService($"Host=localhost;Database=schedule;Username=postgres;Password=fast;");

        await databaseService.InitializeDatabaseAsync();
        
        Console.WriteLine($"Groups from cist: {groups.Count}");
        await databaseService.AddGroupsAsync(groups);
        var db_groups = await databaseService.GetGroupsAsync();
        Console.WriteLine($"Groups from DB: {db_groups.Count()}");
        
        Console.WriteLine($"Teachers from cist: {teachers.Count}");
        await databaseService.AddTeachersAsync(teachers);
        var db_teachers = await databaseService.GetTeachersAsync();
        Console.WriteLine($"Teacher from DB: {db_teachers.Count()}");
        
        Console.WriteLine($"Auditorie from cist: {auditories.Count}");
        await databaseService.AddAuditoriesAsync(auditories);
        var db_auditories = await databaseService.GetAuditoriesAsync();
        Console.WriteLine($"Auditorie from DB: {db_auditories.Count()}");
        
        Console.WriteLine($"Events for choosen group: {events.Count}");
        await databaseService.AddEventsAsync(EventType.Group, "10304333", events);
        var db_events = await databaseService.GetEventsByTypeAsync(EventType.Group, "10304333");
        Console.WriteLine($"Events from DB: {db_events.Count()}");
        
        using var app = HttpServer.CreateBuilder(5555).Build();

        app.Router.MapGet("/", request => { return new HttpResponse("Hello, world!"); });

        await app.StartAsync();
    }
}