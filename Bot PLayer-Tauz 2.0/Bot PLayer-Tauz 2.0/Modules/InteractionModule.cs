using Discord.Interactions;
using Lavalink4NET;
using Lavalink4NET.Players.Vote;
using Lavalink4NET.Players;
using Lavalink4NET.Rest.Entities.Tracks;
using Lavalink4NET.DiscordNet;
using Microsoft.Extensions.Logging;

namespace Bot_PLayer_Tauz_2._0.Modules
{
    public class InteractionModule : InteractionModuleBase<SocketInteractionContext>
    {
        private readonly IAudioService _audioService;
        private readonly ILogger<InteractionModule> _logger;

        public InteractionModule(IAudioService audioService, ILogger<InteractionModule> logger)
        {
            _audioService = audioService;
            _logger = logger;
        }

        [SlashCommand("play", "Plays track", runMode: RunMode.Async)]
        public async Task PlayMusicAsync(string musicName)
        {

            await ReplyAsync("Processando ação");

            //await DeferAsync().ConfigureAwait(false);

            var player = await GetPlayerAsync(connectToVoiceChannel: true).ConfigureAwait(false);

            await ReplyAsync("Ação concluída");


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
