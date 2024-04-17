using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.Lavalink;
using DSharpPlus.SlashCommands;


namespace Bot_PLayer_Tauz_2._0.Modules
{
    public class InteractionModule : ApplicationCommandModule
    {

        [SlashCommand("join", "Join VoiceChannel")]
        public async Task JoinAsync(InteractionContext ctx, [Option("DiscordChannel", "Channel to join")]DiscordChannel voiceChannel)
        {
            var lavaClient = ctx.Client.GetLavalink();
            var lavaNode = lavaClient.ConnectedNodes.Values.First();

            if (!await ValidateJoinAsync(ctx, lavaClient, voiceChannel)) return;

            await lavaNode.ConnectAsync(voiceChannel);
            await ctx.Channel.SendMessageAsync("Join " + voiceChannel.Name);
        }

        [SlashCommand("leave", "Leave to channel")]
        public async Task LeaveAsync(InteractionContext ctx, [Option("DiscordChannel", "Channel to leave")]DiscordChannel voiceChannel)
        {
            var lavaClient = ctx.Client.GetLavalink();
            var lavaNode = lavaClient.ConnectedNodes.Values.First();
            var conn = lavaNode.GetGuildConnection(ctx.Member.VoiceState.Guild);

            if(!await ValidateLeaveAsync(ctx, lavaClient, conn, voiceChannel)) return;

            await conn.DisconnectAsync();
            await ctx.Channel.SendMessageAsync("Leave " + voiceChannel.Name);
            
        }

        [SlashCommand("play", "Plays Track")]
        public async Task PlayTrack(InteractionContext ctx, [Option("MusicName", "Music to Play")] string musicName)
        {

            var lavaClient = ctx.Client.GetLavalink();
            var node = lavaClient.ConnectedNodes.Values.First();
            var conn = node.GetGuildConnection(ctx.Member.VoiceState.Guild);

            if (!await ValidatePlayTrackAsync(ctx, conn)) return;

            var loadResult = await node.Rest.GetTracksAsync(musicName);

            if (!await ValidateTrackAsync(ctx, loadResult, musicName)) return;

            var track = loadResult.Tracks.First();

            await conn.PlayAsync(track);
            await ctx.Channel.SendMessageAsync("Tocando " + track.Title + " url: "+ track.Uri);

        }

        private async ValueTask<bool> ValidateJoinAsync(InteractionContext ctx, LavalinkExtension lavaClient, DiscordChannel voiceChannel)
        {

            if (!lavaClient.ConnectedNodes.Any())
            {
                await ctx.Channel.SendMessageAsync("Conexão com LavaLink não foi estabelecida");
                return false;
            }

            if (voiceChannel.Type != ChannelType.Voice)
            {
                await ctx.Channel.SendMessageAsync("Apenas canais de voz");
                return false;
            }

            return true;
        }

        private async ValueTask<bool> ValidateLeaveAsync(InteractionContext ctx, LavalinkExtension lavaClient, LavalinkGuildConnection conn, DiscordChannel voiceChannel)
        {

            if (!lavaClient.ConnectedNodes.Any())
            {
                await ctx.Channel.SendMessageAsync("Conexão com LavaLink não foi estabelecida");
                return false;
            }

            if (conn == null)
            {
                await ctx.Channel.SendMessageAsync("LavaLink não está conectado");
                return false;
            }

            if (voiceChannel.Type != ChannelType.Voice)
            {
                await ctx.Channel.SendMessageAsync("Apenas canais de voz");
                return false;
            }

            return true;
        }

        private async ValueTask<bool> ValidatePlayTrackAsync(InteractionContext ctx, LavalinkGuildConnection conn)
        {
            if (ctx.Member.VoiceState == null || ctx.Member.VoiceState.Channel == null)
            {
                await ctx.Channel.SendMessageAsync("É necessário estar em um canal de voz");
                return false;
            }

            if (conn == null)
            {
                await ctx.Channel.SendMessageAsync("LavaLink nao está conectado");
                return false;
            }

            

            return true;
        }

        private async ValueTask<bool> ValidateTrackAsync(InteractionContext ctx ,LavalinkLoadResult loadResult, string musicName)
        {
            if (loadResult.LoadResultType == LavalinkLoadResultType.LoadFailed || loadResult.LoadResultType == LavalinkLoadResultType.NoMatches)
            {
                await ctx.Channel.SendMessageAsync("Falha em encontrar " + musicName);
                return false;
            }

            return true;
        }
    }
}
