using Api.Classes;
using Api.Contexts;
using Nure.NET;
using Nure.NET.Types;
using System.Text.Json;

namespace Api.Processors
{
    class GroupProcessor
    {
        public static string GetJson(long id, long start = 0, long end = 0)
        {
            return JsonSerializer.Serialize(Get(EventType.Group, id, start, end), new JsonSerializerOptions { WriteIndented = true });
        }

        public static List<Event> Get(EventType type, long id, long start = 0, long end = 0)
        {
            using (var context = new Context())
            {
                var group = context.Groups.Find(id);
                if (group != null)
                {
                    if (group.Events != "")
                    {
                        var events = JsonSerializer.Deserialize<List<Event>>(group.Events);
                        if (start == 0 && end == 0)
                            return events;
                        else
                        {
                            return events.Where(e => e.StartTime >= start && e.StartTime <= end)
                            .OrderBy(x => x.StartTime)
                            .ToList();
                        }
                    }
                    else
                    {
                        List<Event> events = new List<Event>();
                        events = Cist.GetEvents(type, id, start, end);
                        group.Events = JsonSerializer.Serialize(events);
                        context.SaveChanges();
                        if (start == 0 && end == 0)
                            return events;
                        else
                        {
                            return events.Where(e => e.StartTime >= start && e.StartTime <= end)
                            .OrderBy(x => x.StartTime)
                            .ToList();
                        }
                    }
                }
            }
            return new List<Event>();            
        }
    }
}