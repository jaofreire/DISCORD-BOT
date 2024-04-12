using Bot_PlayerTauz.Services;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.Logging;

namespace Bot_PlayerTauz.Modules
{
    public class General : ModuleBase<SocketCommandContext>
    {
        private ILogger<General> _logger;
        private readonly AudioService _audioService;

        public General(ILogger<General> logger, AudioService audioService)
        {
            _logger = logger;
            _audioService = audioService;
        }

        [Command("play")]
        [Alias("p")]
        public async Task PlayMusicAsync()
        {
            await _audioService.ConnectVoiceChannel(Context);

            _logger.LogInformation("User {user} used the play command", Context.User.Username);
           
            await ReplyAsync("Music is playing!");

        }

        [Command("stop")]
        [Alias("s")]
        public async Task StopMusicAsync()
        {
            await _audioService.DisconnectVoiceChannel(Context);

            _logger.LogInformation("User {user} used the stop command", Context.User.Username);

            await ReplyAsync("Stop Music!");
        }
    }
}
