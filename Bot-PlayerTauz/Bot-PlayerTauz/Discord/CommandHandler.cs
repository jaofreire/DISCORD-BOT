using Discord;
using Discord.Addons.Hosting;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Reflection;


namespace Bot_PlayerTauz.Discord
{
    public class CommandHandler : DiscordClientService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly CommandService _commandService;
        private readonly DiscordSocketClient _socketClient;
        private readonly IConfiguration _configuration;

        public CommandHandler(IServiceProvider serviceProvider, DiscordSocketClient socketClient, CommandService commandService, IConfiguration configuration, ILogger<CommandHandler> logger) : base(socketClient, logger)
        {
            _socketClient = socketClient;
            _serviceProvider = serviceProvider;
            _commandService = commandService;
            _configuration = configuration;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _socketClient.MessageReceived += OnMessageReceived;
            await _commandService.AddModulesAsync(Assembly.GetEntryAssembly(), _serviceProvider);
        }

        private async Task OnMessageReceived(SocketMessage socketMessage)
        {
            if (socketMessage is not SocketUserMessage message) return;
            if (message.Source != MessageSource.User) return;

            int argPos = 0;
            if (!message.HasStringPrefix(_configuration["DiscordEnv:Prefix"], ref argPos) && !message.HasMentionPrefix(_socketClient.CurrentUser, ref argPos)) return;

            var context = new SocketCommandContext(_socketClient, message);

            await _commandService.ExecuteAsync(context, argPos, _serviceProvider);

        }
    }
}
