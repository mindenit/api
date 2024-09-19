using Api.Classes;
using Api.Contexts;
using Nure.NET;
using Nure.NET.Types;
using Serilog;
using System.Text.Json;
using Discord.Webhook;


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
        
        public static bool IsExist(long id)
        {
            using (var context = new Context())
            {
                return context.Groups.Any(t => t.Id == id);
            }
        }

        public static async Task Update()
        {
            using (var context = new Context())
            {
                try
                {
                    var groups = Cist.GetGroups();
                    List<ScheduleGroup> scheduleGroups = new List<ScheduleGroup>();
                    scheduleGroups = ScheduleGroup.Convert(groups);
                    
                    if(context.Groups.ToList().Count != scheduleGroups.Count)
                    {
                        foreach (var group in scheduleGroups)
                        {
                            if (!context.Groups.Any(t => t.Id == group.Id))
                            {
                                context.Groups.Add(group);
                            }
                        }
                        context.SaveChanges();
                    }
                }
                catch (Exception e)
                {
                    var client = new DiscordWebhookClient(Environment.GetEnvironmentVariable("DISCORD_WEBHOOK_URL"));
                    await client.SendMessageAsync("Error while updating information: \n> " + e.Message + "\n" + "> " + e.StackTrace);
                }
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