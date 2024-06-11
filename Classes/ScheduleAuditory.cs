using System.Text.Json.Serialization;
using Nure.NET.Types;

namespace Api.Classes
{
    public class ScheduleAuditory: Auditory
    {
        [JsonIgnore]
        public string Events { get; set; } = "";


        public static List<ScheduleAuditory> Convert(List<Auditory> auditories)
        {
            List<ScheduleAuditory> scheduleAuditories = new List<ScheduleAuditory>();
            foreach (var auditory in auditories)
            {
                scheduleAuditories.Add(new ScheduleAuditory { Id = auditory.Id, Name = auditory.Name, Events = "" });

            }
            return scheduleAuditories;
        }
    }
}