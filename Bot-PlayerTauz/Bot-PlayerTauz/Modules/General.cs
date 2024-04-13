using Discord.Commands;
using Lavalink4NET;
using Lavalink4NET.DiscordNet;
using Lavalink4NET.Players;
using Lavalink4NET.Players.Vote;
using Lavalink4NET.Rest.Entities.Tracks;
using Microsoft.Extensions.Logging;


namespace Bot_PlayerTauz.Modules
{
    public class General : ModuleBase<SocketCommandContext>
    {
        private ILogger<General> _logger;
        private readonly IAudioService _audioService;

        public General(ILogger<General> logger, IAudioService audioService)
        {
            _logger = logger;
            _audioService = audioService;
        }

        [Command("play")]
        [Alias("p")]
        public async Task PlayMusicAsync([Remainder]string musicName)
        {
            var player = await GetPlayerAsync(connectToVoiceChannel : true).ConfigureAwait(false);

            if (player == null) return;

            var track = await _audioService.Tracks
                .LoadTrackAsync(musicName, TrackSearchMode.YouTube)
                .ConfigureAwait(false);

            if (track == null)
            {
                await ReplyAsync("Não foi possível encontrar a música").ConfigureAwait(false);
                return;
            }

            var position = await player.PlayAsync(track).ConfigureAwait(false);

            if (position is 0)
            {
                await ReplyAsync("Música tocando " + track.Uri).ConfigureAwait(false);
            }

            _logger.LogInformation("User {user} used the play command", Context.User.Username);
           
            await ReplyAsync("Music is playing!");

        }

        [Command("stop")]
        [Alias("s")]
        public async Task StopMusicAsync()
        {
            var player = await GetPlayerAsync(connectToVoiceChannel : false).ConfigureAwait(false);

            if(player == null) return;
            
            _logger.LogInformation("User {user} used the stop command", Context.User.Username);


            await player.StopAsync().ConfigureAwait(false);
            await player.DisconnectAsync().ConfigureAwait(false);
            await ReplyAsync("Stop Music and Disconnect!");
        }


        private async ValueTask<VoteLavalinkPlayer?> GetPlayerAsync(bool connectToVoiceChannel = true)
        {
            var retrieveOptions = new PlayerRetrieveOptions(
                ChannelBehavior: connectToVoiceChannel ? PlayerChannelBehavior.Join : PlayerChannelBehavior.None);

            var result = await _audioService.Players
                .RetrieveAsync(Context, playerFactory: PlayerFactory.Vote, retrieveOptions)
                .ConfigureAwait(false);

            if (!result.IsSuccess)
            {
                var errorMessage = result.Status switch
                {
                    PlayerRetrieveStatus.UserNotInVoiceChannel => "É necessário estar em um canal de voz!!",
                    PlayerRetrieveStatus.BotNotConnected => "O bot esta offline por enquanto!!",
                    _ => "Erro Desconhecido"
                };

                await ReplyAsync(errorMessage).ConfigureAwait(false);
                return null;
            }

            return result.Player;

        }


    }
}
