using System.Text.Json.Serialization;
using Nure.NET.Types;

namespace Api.Classes
{
    public class ScheduleGroup : Group
    {
        [JsonIgnore]
        public string Events { get; set; } = "";
        public static List<ScheduleGroup> Convert(List<Group> groups)
        {
            List<ScheduleGroup> scheduleGroups = new List<ScheduleGroup>();
            foreach (var group in groups)
            {
                scheduleGroups.Add(new ScheduleGroup { Id = group.Id, Name = group.Name, Events = "" });

            }
            return scheduleGroups;
        }

    }
}