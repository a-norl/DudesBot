using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using Microsoft.EntityFrameworkCore;

namespace DudesBot.Commands
{
    public class CustomCommands : BaseCommandModule
    {
        public IDbContextFactory<DudesDBContext> ContextFactory { private get; set; }

        [Command("addcommand"), Hidden, RequireOwner]
        public async Task AddCommandCommand(CommandContext context, string commandTrigger, string commandResponse)
        {
            var BotDBContext = ContextFactory.CreateDbContext();
            var inputCommand = new CustomCommandObject
            {
                authorId = context.Member.Id,
                CommandPhrase = commandTrigger,
                Response = commandResponse,
            };
            var AddCommandTask = BotDBContext.CustomCommandObjects.AddAsync(inputCommand);
            await AddCommandTask;
            BotDBContext.SaveChanges();
            await context.RespondAsync($"added command {inputCommand.CommandPhrase} : {inputCommand.Response}");
        }
    }
}