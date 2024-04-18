using Bot_PLayer_Tauz_2._0.Data;
using Bot_PLayer_Tauz_2._0.Data.Models;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.Lavalink;
using Microsoft.EntityFrameworkCore;



namespace Bot_PLayer_Tauz_2._0.Modules
{
    public class CommandsModule : BaseCommandModule
    {

        private readonly MongoContext _mongoContext;

        public CommandsModule(MongoContext mongoContext)
        {
            _mongoContext = mongoContext;
        }


        #region JOIN/LEAVE

        [Command("join")]
        [Aliases("j")]
        public async Task JoinAsync(CommandContext ctx ,[RemainingText]DiscordChannel voiceChannel)
        {
            var lavaClient = ctx.Client.GetLavalink();
            var lavaNode = lavaClient.ConnectedNodes.Values.First();

            if (!await ValidateJoinAsync(ctx, lavaClient, voiceChannel)) return;

            await lavaNode.ConnectAsync(voiceChannel);
            await ctx.Channel.SendMessageAsync("Join " + voiceChannel.Name);
        }

        [Command("leave")]
        [Aliases("l")]
        public async Task LeaveAsync(CommandContext ctx, [RemainingText] DiscordChannel voiceChannel)
        {
            var lavaClient = ctx.Client.GetLavalink();
            var lavaNode = lavaClient.ConnectedNodes.Values.First();
            var conn = lavaNode.GetGuildConnection(ctx.Member.VoiceState.Guild);

            if (!await ValidateLeaveAsync(ctx, lavaClient, conn, voiceChannel)) return;

            await conn.DisconnectAsync();
            await ctx.Channel.SendMessageAsync("Leave " + voiceChannel.Name);
        }

        #endregion

        #region DATA HANDLER

        [Command("RegisterNewMusic")]
        public async Task AddNewMusicAsync(CommandContext ctx, [RemainingText]string musicName)
        {
            var musicModel = new MusicModel()
            {
                Name = musicName,
                Url = "https://musica"
            };

            await _mongoContext.Musics.AddAsync(musicModel);
            await _mongoContext.SaveChangesAsync();

            await ctx.Channel.SendMessageAsync("Nova música " + musicName + " adicionada");
        }

        [Command("ListMusics")]
        public async Task ListAllMusicsAsync(CommandContext ctx)
        {
            List<MusicModel> allMusics = await _mongoContext.Musics.ToListAsync();

            await ctx.Channel.SendMessageAsync(allMusics.ToString());
        }

        #endregion

        #region TRACK HANDLER

        [Command("play")]
        [Aliases("p")]
        public async Task PlayTrack(CommandContext ctx, [RemainingText]string musicName)
        {

            var lavaClient = ctx.Client.GetLavalink();
            var node = lavaClient.ConnectedNodes.Values.First();
            var conn = node.GetGuildConnection(ctx.Member.VoiceState.Guild);

            if (!await ValidatePlayTrackAsync(ctx, conn)) return;

            var loadResult = await node.Rest.GetTracksAsync(musicName);

            if (!await ValidateTrackAsync(ctx, loadResult, musicName)) return;

            var track = loadResult.Tracks.First();

            await conn.PlayAsync(track);
            await ctx.Channel.SendMessageAsync("Tocando " + track.Title + " url: " + track.Uri);

        }

        #endregion


        private async ValueTask<bool> ValidateJoinAsync(CommandContext ctx, LavalinkExtension lavaClient, DiscordChannel voiceChannel)
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

        private async ValueTask<bool> ValidateLeaveAsync(CommandContext ctx, LavalinkExtension lavaClient, LavalinkGuildConnection conn, DiscordChannel voiceChannel)
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

        private async ValueTask<bool> ValidatePlayTrackAsync(CommandContext ctx, LavalinkGuildConnection conn)
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

        private async ValueTask<bool> ValidateTrackAsync(CommandContext ctx, LavalinkLoadResult loadResult, string musicName)
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
