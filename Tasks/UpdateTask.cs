using Api.Contexts;
using Api.Handlers;
using Api.Processors;
using Coravel.Invocable;
using Serilog;

namespace Api.Tasks
{
    public class UpdateTask : IInvocable
    {
        public async Task Invoke()
        {
            Log.Information("Start updating information...");

            GroupsHandler.Clear();
            TeachersHandler.Clear();
            AuditoriesHandler.Clear();

            GroupsHandler.Update();
            TeachersHandler.Update();
            AuditoriesHandler.Update();

            AuditoryProcessor.Update();
            GroupProcessor.Update();
            TeacherProcessor.Update();

            Log.Information("Information updated!");

        }
    }
}
