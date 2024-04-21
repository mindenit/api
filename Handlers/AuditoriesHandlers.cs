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

        public static List<Auditory> Get()
        {
            List<Auditory> auditories = new List<Auditory>();

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
                context.Auditories.AddRange(Cist.GetAuditories());
                context.SaveChanges();
            }
        }
    }
}