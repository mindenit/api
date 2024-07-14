using Api.Classes;
using Api.Contexts;
using Nure.NET;
using Nure.NET.Types;
using Serilog;
using System.Text.Json;

namespace Api.Handlers
{
    class GroupsHandler
    {
        public static string GetJson()
        {
            return JsonSerializer.Serialize(Get(), new JsonSerializerOptions { 
                WriteIndented = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
           });
        }

        public static List<ScheduleGroup> Get()
        {
            List<ScheduleGroup> groups = new List<ScheduleGroup>();

            using (var context = new Context())
            {
                groups = context.Groups.ToList();
                Log.Information($"Count of groups: {groups.Count}");
            }

            return groups;
        }

        public static void Update()
        {
            using (var context = new Context())
            {
                var groups = Cist.GetGroups();
                List<ScheduleGroup> scheduleGroups = new List<ScheduleGroup>();
                scheduleGroups = ScheduleGroup.Convert(groups);
                Log.Information($"Count of groups: {scheduleGroups.Count}");
                context.Groups.AddRange(scheduleGroups);
                context.SaveChanges();
            }
        }

        public static void Clear()
        {
            using (var context = new Context())
            {
                context.Groups.RemoveRange(context.Groups);
                context.SaveChanges();
            }
        }
    }
}