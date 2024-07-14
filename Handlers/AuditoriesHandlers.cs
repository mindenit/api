using Api.Classes;
using Api.Contexts;
using Nure.NET;
using Nure.NET.Types;
using System.Text.Json;

namespace Api.Handlers
{
    class AuditoriesHandler
    {
        public static string GetJson()
        {
            return JsonSerializer.Serialize(Get(), new JsonSerializerOptions { WriteIndented = true });
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

        public static void Update()
        {
            using (var context = new Context())
            {
                var auditories = Cist.GetAuditories();
                List<ScheduleAuditory>  scheduleAuditories = new List<ScheduleAuditory>();
                scheduleAuditories = auditories.Select(x => new ScheduleAuditory { Id = x.Id, Name = x.Name }).ToList();
                context.Auditories.AddRange(scheduleAuditories);
                context.SaveChanges();
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