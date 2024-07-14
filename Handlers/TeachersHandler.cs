using Api.Classes;
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

        public static List<ScheduleTeacher> Get()
        {
            List<ScheduleTeacher> teachers = new List<ScheduleTeacher>();

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
                var teachers = Cist.GetTeachers();
                List<ScheduleTeacher> scheduleTeachers = new List<ScheduleTeacher>();
                scheduleTeachers = teachers.Select(x => new ScheduleTeacher { Id = x.Id, FullName = x.FullName, ShortName = x.ShortName }).ToList();
                context.Teachers.AddRange(scheduleTeachers);
                context.SaveChanges();
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