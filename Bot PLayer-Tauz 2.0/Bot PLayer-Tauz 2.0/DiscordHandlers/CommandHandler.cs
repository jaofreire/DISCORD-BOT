using Discord.Addons.Hosting;
using Discord.Interactions;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Reflection;


namespace Bot_PLayer_Tauz_2._0.DiscordHandlers
{
    public class CommandHandler : DiscordClientService
    {
        private readonly InteractionService _interactionService;
        private readonly DiscordSocketClient _socketClient;
        private readonly IConfiguration _configuration;
        private readonly IServiceProvider _serviceProvider;

        public CommandHandler(InteractionService interactionService, DiscordSocketClient socketClient, IConfiguration configuration, IServiceProvider serviceProvider, ILogger<CommandHandler> logger) : base(socketClient, logger)
        {
            _interactionService = interactionService;
            _socketClient = socketClient;
            _configuration = configuration;
            _serviceProvider = serviceProvider;
        }

        protected async override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _socketClient.InteractionCreated += OnInteractionCreated;
            _socketClient.Ready += IsClientReady;

            await _interactionService
                .AddModulesAsync(Assembly.GetEntryAssembly(), _serviceProvider)
                .ConfigureAwait(false);
        }

        private async Task OnInteractionCreated(SocketInteraction interaction)
        {
            var context = new SocketInteractionContext(_socketClient, interaction);
            await _interactionService.ExecuteCommandAsync(context, _serviceProvider).ConfigureAwait(false);
        }

        private async Task IsClientReady()
        {
           
            await _interactionService
                .RegisterCommandsToGuildAsync(_configuration.GetValue<ulong>("DiscordEnv:GuildId"))
                .ConfigureAwait(false);
        }

       
    }
}
