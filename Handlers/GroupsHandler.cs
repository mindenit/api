using Api.Contexts;
using Nure.NET;
using Nure.NET.Types;
using System.Text.Json;

namespace Api.Handlers
{
    class GroupsHandler
    {
        public static string GetJson()
        {
            return JsonSerializer.Serialize(Get(), new JsonSerializerOptions { WriteIndented = true });
        }

        public static List<Group> Get()
        {
            List<Group> groups = new List<Group>();

            using (var context = new Context())
            {
                groups = context.Groups.ToList();
            }

            return groups;
        }

        public static void Update()
        {
            using (var context = new Context())
            {
                context.Groups.AddRange(Cist.GetGroups());
                context.SaveChanges();
            }
        }
    }
}