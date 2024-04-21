using Api.Contexts;
using Nure.NET;
using Nure.NET.Types;
using System.Text.Json;

namespace Api.Handlers
{
    class TeachersHandler
    {
        public static string GetJson()
        {
            return JsonSerializer.Serialize(Get(), new JsonSerializerOptions { WriteIndented = true });
        }

        public static List<Teacher> Get()
        {
            List<Teacher> teachers = new List<Teacher>();

            using (var context = new Context())
            {
                teachers = context.Teachers.ToList();
            }

            return teachers;
        }

        public static void Update()
        {
            using (var context = new Context())
            {
                context.Teachers.AddRange(Cist.GetTeachers());
                context.SaveChanges();
            }
        }
    }
}