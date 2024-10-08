using Api.Classes;
using Api.Contexts;
using Nure.NET;
using Nure.NET.Types;
using Serilog;
using System.Text.Json;
using Discord.Webhook;

namespace Api.Processors
{
    class TeacherProcessor
    {
        public static string GetJson(long id, long start = 0, long end = 0)
        {
            return JsonSerializer.Serialize(Get(EventType.Teacher, id, start, end), new JsonSerializerOptions { 
                WriteIndented = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });
        }

        public static List<Event> Get(EventType type, long id, long start = 0, long end = 0)
        {
            using (var context = new Context())
            {
                var teacher = context.Teachers.Find(id);
                if (teacher != null)
                {
                    if (teacher.Events != "")
                    {
                        var events = JsonSerializer.Deserialize<List<Event>>(teacher.Events);
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
                        teacher.Events = JsonSerializer.Serialize(events);
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

        public static async Task Update()
        {
            using (var context = new Context())
            {
                try
                {
                    foreach (var teacher in context.Teachers)
                    {
                        try
                        {
                            teacher.Events = JsonSerializer.Serialize(Cist.GetEvents(EventType.Teacher, teacher.Id));
                            Thread.Sleep(1000);
                        }
                        catch (Exception e)
                        {
                            var client = new DiscordWebhookClient(Environment.GetEnvironmentVariable("DISCORD_WEBHOOK_URL"));
                            await client.SendMessageAsync($"Error while updating information: \n > {e.Message} \n Stack \n > {e.StackTrace}"
                                                            + $"\n > Additional information:"
                                                            + $"\n > Teacher ID: {teacher.Id}"
                                                            + $"\n > Teacher: {teacher.FullName}");
                        }

                    }
                }
                catch (Exception e)
                {
                    Log.Error("Error while updating teachers");
                }
                context.SaveChanges();
            }
        }
    }
}