using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace DudesBot.Services
{
    public class MessageSentWorker
    {
        private readonly ConcurrentQueue<DiscordMessage> _workQueue = new();
        private readonly DiscordClient _client; //Used to get services from CommandNext

        public MessageSentWorker(DiscordClient client)
        {
            _client = client;
            Thread workerThread = new(new ThreadStart(OnStart));
            workerThread.IsBackground = true;
            workerThread.Start();
        }

        public void Enqueue(DiscordMessage toQueue)
        {
            _workQueue.Enqueue(toQueue);
        }

        private async void OnStart()
        {
            while (true)
            {
                if (_workQueue.TryDequeue(out DiscordMessage message))
                {
                    if (message.Author.IsBot) { continue; }
                    if (message.Content is null) { continue; }
                    if (message.Content is "") { continue; }
                    await MessageHandlers.CustomCommandHandler(message, _client.GetCommandsNext().Services);
                    if (message.Content.Contains("armpit") || message.Content.Contains("arm pit"))
                    {
                        await message.DeleteAsync();
                    }
                }
            }
        }
    }

    static class MessageHandlers
    {
        public static async Task CustomCommandHandler(DiscordMessage message, IServiceProvider services)
        {
            if (message.Content[0] != '-') { return; }

            string commandMessage = message.Content.TrimStart('-');
            var DBContext = services.GetService<IDbContextFactory<DudesDBContext>>().CreateDbContext();
            var returnedCommandQuery = DBContext.CustomCommandObjects.Where(command => command.CommandPhrase == commandMessage.Trim());
            CustomCommandObject returnedCommand;
            try
            {
                returnedCommand = returnedCommandQuery.First();
            }
            catch
            {
                return;
            }


            await message.Channel.SendMessageAsync(returnedCommand.Response);
        }
    }
}