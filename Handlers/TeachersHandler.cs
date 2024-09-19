using Api.Classes;
using Api.Contexts;
using Nure.NET;
using Nure.NET.Types;
using System.Text.Json;
using Discord.Webhook;


namespace Api.Handlers
{
    class TeachersHandler
    {
        public static string GetJson()
        {
            return JsonSerializer.Serialize(Get(), new JsonSerializerOptions { 
                WriteIndented = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
           });
        }

        public static List<ScheduleTeacher> Get()
        {
            List<ScheduleTeacher> teachers = new List<ScheduleTeacher>();

            using (var context = new Context())
            {
                teachers = context.Teachers.ToList();
            }

            return teachers;
        }
        
        public static bool IsExist(long id)
        {
            using (var context = new Context())
            {
                return context.Teachers.Any(t => t.Id == id);
            }
        }

        public static async Task Update()
        {
            using (var context = new Context())
            {
                try
                {
                    var teachers = Cist.GetTeachers();
                    List<ScheduleTeacher> scheduleTeachers = new List<ScheduleTeacher>();
                    scheduleTeachers = ScheduleTeacher.Convert(teachers);
                    
                    if(context.Teachers.ToList().Count != scheduleTeachers.Count)
                    {
                        foreach (var teacher in scheduleTeachers)
                        {
                            if (!context.Teachers.Any(t => t.Id == teacher.Id))
                            {
                                context.Teachers.Add(teacher);
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
                context.Teachers.RemoveRange(context.Teachers);
                context.SaveChanges();
            }
        }
    }
}