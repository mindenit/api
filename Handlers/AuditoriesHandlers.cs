using Api.Classes;
using Api.Contexts;
using Nure.NET;
using Nure.NET.Types;
using System.Text.Json;
using Discord.Webhook;


namespace Api.Handlers
{
    class AuditoriesHandler
    {
        public static string GetJson()
        {
            return JsonSerializer.Serialize(Get(), new JsonSerializerOptions { 
                WriteIndented = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });
        }

        public static List<ScheduleAuditory> Get()
        {
            List<ScheduleAuditory> auditories = new List<ScheduleAuditory>();

            using (var context = new Context())
            {
                auditories = context.Auditories.ToList();
            }

            return auditories;
        }
        
        public static bool IsExist(long id)
        {
            using (var context = new Context())
            {
                return context.Auditories.Any(t => t.Id == id);
            }
        }

        public static async Task Update()
        {
            using (var context = new Context())
            {
                try
                {
                    var auditories = Cist.GetAuditories();
                    List<ScheduleAuditory> scheduleAuditories = new List<ScheduleAuditory>();
                    scheduleAuditories = ScheduleAuditory.Convert(auditories);
                    
                    if(context.Auditories.ToList().Count != scheduleAuditories.Count)
                    {
                        foreach (var auditory in scheduleAuditories)
                        {
                            if (!context.Auditories.Any(t => t.Id == auditory.Id))
                            {
                                context.Auditories.Add(auditory);
                            }
                        }
                        context.SaveChanges();
                    }
                }
                catch (Exception e){
                    var client = new DiscordWebhookClient(Environment.GetEnvironmentVariable("DISCORD_WEBHOOK_URL"));
                    await client.SendMessageAsync("Error while updating information: \n> " + e.Message + "\n" + "> " + e.StackTrace);
                }
            }
        }

        public static void Clear()
        {
            using (var context = new Context())
            {
                context.Auditories.RemoveRange(context.Auditories);
                context.SaveChanges();
            }
        }
    }
}