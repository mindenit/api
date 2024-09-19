using Api.Classes;
using Api.Contexts;
using Nure.NET;
using Nure.NET.Types;
using Serilog;
using System.Text.Json;
using Discord.Webhook;


namespace Api.Processors
{
    class AuditoryProcessor
    {
        public static string GetJson(long id, long start = 0, long end = 0)
        {
            return JsonSerializer.Serialize(Get(EventType.Auditory, id, start, end), new JsonSerializerOptions
            {
                WriteIndented = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });
        }

        public static List<Event> Get(EventType type, long id, long start = 0, long end = 0)
        {
            using (var context = new Context())
            {
                var auditory = context.Auditories.Find(id);
                if (auditory != null)
                {
                    if (auditory.Events != "")
                    {
                        var events = JsonSerializer.Deserialize<List<Event>>(auditory.Events);
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
                        auditory.Events = JsonSerializer.Serialize(events);
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
                    foreach (var auditory in context.Auditories)
                    {
                        try
                        {
                            List<Event> events = new List<Event>();
                            events = Cist.GetEvents(EventType.Auditory, auditory.Id);
                            auditory.Events = JsonSerializer.Serialize(events);
                            Thread.Sleep(1000);
                        }
                        catch (Exception e)
                        {
                            var client = new DiscordWebhookClient(Environment.GetEnvironmentVariable("DISCORD_WEBHOOK_URL"));
                            await client.SendMessageAsync($"Error while updating information: \n > {e.Message} \n Stack \n > {e.StackTrace}"
                                                            + $"\n > Additional information:"
                                                            + $"\n > Auditory ID: {auditory.Id}"
                                                            + $"\n > Auditory: {auditory.Name}");
                        }
                    }
                }
                catch (Exception e)
                {
                    var client = new DiscordWebhookClient(Environment.GetEnvironmentVariable("DISCORD_WEBHOOK_URL"));
                    await client.SendMessageAsync("Error while updating information: \n> " + e.Message + "\n" + "> " + e.StackTrace);
                }
                context.SaveChanges();
            }
        }
    }
}