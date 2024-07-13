using System.Text.Json.Serialization;
using Nure.NET.Types;

namespace Api.Classes
{
    public class ScheduleTeacher : Teacher
    {
        [JsonIgnore]
        public string Events { get; set; } = "";

        public static List<ScheduleTeacher> Convert(List<Teacher> teachers)
        {
            List<ScheduleTeacher> scheduleTeachers = new List<ScheduleTeacher>();
            foreach (var teacher in teachers)
            {
                scheduleTeachers.Add(new ScheduleTeacher { Id = teacher.Id, FullName = teacher.FullName, ShortName = teacher.ShortName, Events = "" });
            }
            return scheduleTeachers;
        }

    }
}