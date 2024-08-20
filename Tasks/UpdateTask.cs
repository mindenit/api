using Api.Contexts;
using Api.Handlers;
using Api.Processors;
using Coravel.Invocable;
using Serilog;
using Discord.Webhook;

namespace Api.Tasks
{
    public class UpdateTask : IInvocable
    {
        public async Task Invoke()
        {
            
            // send log message with discord webhook by Discord.NET
             
            try
            {
                var client = new DiscordWebhookClient(Environment.GetEnvironmentVariable("DISCORD_WEBHOOK_URL"));
                await client.SendMessageAsync("Start updating information...");
            }
            catch (Exception e)
            {
                Log.Error(e, "Error while sending message to discord");
            }
            
            Log.Information("Start updating information...");

            try
            {
                GroupsHandler.Update();
                TeachersHandler.Update();
                AuditoriesHandler.Update();

                AuditoryProcessor.Update();
                GroupProcessor.Update();
                TeacherProcessor.Update();
            }
            catch (Exception e)
            {
                var client = new DiscordWebhookClient(Environment.GetEnvironmentVariable("DISCORD_WEBHOOK_URL"));
                await client.SendMessageAsync("Error while updating information: >" + e.Message);
                Log.Error(e, "Error while updating information");
            }

            Log.Information("Information updated!");
            
            try
            {
                var client = new DiscordWebhookClient(Environment.GetEnvironmentVariable("DISCORD_WEBHOOK_URL"));
                await client.SendMessageAsync("Information updated!");
            }
            catch (Exception e)
            {
                Log.Error(e, "Error while sending message to discord");
            }

        }
    }
}
