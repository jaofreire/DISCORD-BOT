using Bot_PLayer_Tauz_2._0.Data;
using Bot_PLayer_Tauz_2._0.Data.Models;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity.Extensions;
using DSharpPlus.Lavalink;
using Microsoft.EntityFrameworkCore;
using MongoDB.Bson;
using System.Collections.Generic;



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

        [Command("Join")]
        [Aliases("j")]
        public async Task JoinAsync(CommandContext ctx ,[RemainingText]DiscordChannel voiceChannel)
        {
            var lavaClient = ctx.Client.GetLavalink();
            var lavaNode = lavaClient.ConnectedNodes.Values.First();

            if (!await ValidateJoinAsync(ctx, lavaClient, voiceChannel)) return;

            await lavaNode.ConnectAsync(voiceChannel);
            await ctx.Channel.SendMessageAsync("Join " + voiceChannel.Name);
        }

        [Command("Leave")]
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
        [Aliases("rm")]
        public async Task AddNewMusicAsync(CommandContext ctx, [RemainingText]string musicName)
        {
            var lavaClient = ctx.Client.GetLavalink();
            var node = lavaClient.ConnectedNodes.Values.First();

            var loadResult = await node.Rest.GetTracksAsync(musicName);
            await ctx.Channel.SendMessageAsync("Buscando música...");

            if (!await ValidateTrackAsync(ctx, loadResult, musicName)) return;

            var track =  loadResult.Tracks.First();

            var musicModel = new MusicModel()
            {
                Name = musicName,
                Url = track.Uri.ToString()
            };

            await ctx.Channel.SendMessageAsync("Adicionando...");

            await _mongoContext.Musics.AddAsync(musicModel);
            await _mongoContext.SaveChangesAsync();

            await ctx.Channel.SendMessageAsync("Nova música " + musicName + " adicionada");
        }

        [Command("ListMusics")]
        [Aliases("lm")]
        public async Task ListAllMusicsAsync(CommandContext ctx)
        {
            List<MusicModel> allMusics = await _mongoContext.Musics.ToListAsync();

            await ctx.Channel.SendMessageAsync("Buscando...!");

            foreach (var musics in allMusics)
            {
                await ctx.Channel.SendMessageAsync($"Nome: {musics.Name.ToString().ToUpper()}\n Url: {musics.Url.ToString()}\n ----------------");
            }

            await ctx.Channel.SendMessageAsync("Músicas encontradas!!");

        }
        
        [Command("SearchMusics")]
        [Aliases("sm")]
        public async Task SearchMusicsAsync(CommandContext ctx, [RemainingText]string musicName)
        {
            List<MusicModel> musicList = await _mongoContext.Musics.Where(x => x.Name.Contains(musicName)).ToListAsync();

            await ctx.Channel.SendMessageAsync($"Buscando músicas com o nome: {musicName} !!");

            if (!await ValidateSearchMusicsAsync(ctx, musicList, musicName)) return;
            
            foreach (var musics in musicList)
            {
                await ctx.Channel.SendMessageAsync($"Nome: {musics.Name.ToString().ToUpper()}\n Url: {musics.Url.ToString()}\n ----------------");
            }

            await ctx.Channel.SendMessageAsync("Músicas encontradas!!");

        }

        #endregion

        #region TRACK HANDLER

        [Command("Play")]
        [Aliases("p")]
        public async Task PlayTrackWithName(CommandContext ctx, [RemainingText]string musicName)
        {

            var lavaClient = ctx.Client.GetLavalink();
            var node = lavaClient.ConnectedNodes.Values.First();
            var conn = node.GetGuildConnection(ctx.Member.VoiceState.Guild);

            var getMusics = await _mongoContext.Musics.Where(x => x.Name.Contains(musicName)).ToListAsync();

            if (ValidateExistMusicWithName(ctx, getMusics))
            {
                await ctx.Channel.SendMessageAsync($"Uma música com o nome {musicName} foi encontrada na sua lista, deseja toca-la?");
                await ctx.Channel.SendMessageAsync("*sim* para confirmar, *nao* para tocar a música fora da lista");

                var responseMessage = await ctx.Message.GetNextMessageAsync(m =>
                {
                    return m.Content.ToLower() == "sim" || m.Content.ToLower() == "nao";
                });

                if (!responseMessage.TimedOut)
                {
                    if (responseMessage.Result.Content.Contains("sim"))
                    {
                        if (getMusics.Count > 1)
                        {
                            List<string> IdList = new List<string>();

                            await ctx.Channel.SendMessageAsync($"Escolha uma das músicas encontradas com o nome {musicName}");

                            foreach (var musics in getMusics)
                            {
                                await ctx.Channel.SendMessageAsync($" Id: {musics.Id} - \n Nome: {musics.Name.ToString()}\n Url: {musics.Url.ToString()}\n ---------------");

                                IdList.Add(musics.Id.ToString());
                            }

                            var responseMessageInTheList = await ctx.Message.GetNextMessageAsync( m =>
                            {
                                foreach (var Idmusics in IdList)
                                {
                                    if (m.Content == Idmusics)
                                    {
                                        return true;
                                    }
                                }
                                return false;
                            });


                            if (!responseMessageInTheList.TimedOut)
                            {
                                var id = new ObjectId(responseMessageInTheList.Result.Content);
                                var getMusicById = await _mongoContext.Musics.FirstOrDefaultAsync(x => x.Id == id);

                                var uriId = new Uri(getMusicById.Url);
                                PlayMusicWithUrlAsync(ctx, node, conn, uriId);

                                return;
                            }

                        }

                        var uri = new Uri(getMusics[0].Url);

                        PlayMusicWithUrlAsync(ctx, node, conn, uri);
                        return;
                    }
                }

            }

             PlayMusicAsync(ctx, node, conn, musicName);

        }


        [Command("PlayUrl")]
        [Aliases("purl")]
        public async Task PlayTrackWithUrl(CommandContext ctx, Uri url)
        {

            var lavaClient = ctx.Client.GetLavalink();
            var node = lavaClient.ConnectedNodes.Values.First();
            var conn = node.GetGuildConnection(ctx.Member.VoiceState.Guild);

            if (!await ValidatePlayTrackAsync(ctx, conn)) return;

            var loadResult = await node.Rest.GetTracksAsync(url);

            if (!await ValidateTrackWithUrlAsync(ctx, loadResult, url)) return;

            var track = loadResult.Tracks.First();

            await conn.PlayAsync(track);
            await ctx.Channel.SendMessageAsync("Tocando " + track.Title + " url: " + track.Uri);

        }

        private async void PlayMusicAsync(CommandContext ctx, LavalinkNodeConnection node, LavalinkGuildConnection conn, string musicName)
        {
            if (!await ValidatePlayTrackAsync(ctx, conn)) return;

            var loadResult = await node.Rest.GetTracksAsync(musicName);

            if (!await ValidateTrackAsync(ctx, loadResult, musicName)) return;

            var track = loadResult.Tracks.First();

            await conn.PlayAsync(track);
            await ctx.Channel.SendMessageAsync("Tocando " + track.Title + " url: " + track.Uri);
        }

        private async void PlayMusicWithUrlAsync(CommandContext ctx, LavalinkNodeConnection node, LavalinkGuildConnection conn, Uri Url)
        {
            if (!await ValidatePlayTrackAsync(ctx, conn)) return;

            var loadMusic = await node.Rest.GetTracksAsync(Url);

            if (!await ValidateTrackWithUrlAsync(ctx, loadMusic, Url)) return;

            var track = loadMusic.Tracks.First();

            await conn.PlayAsync(track);
            await ctx.Channel.SendMessageAsync("Tocando " + track.Title + " url: " + track.Uri);
        }

        #endregion

        #region VALIDATIONS

        private async ValueTask<bool> ValidateJoinAsync(CommandContext ctx, LavalinkExtension lavaClient, DiscordChannel voiceChannel)
        {

            if (!lavaClient.ConnectedNodes.Any())
            {
                await ctx.Channel.SendMessageAsync("Conexão com LavaLink não foi estabelecida");
                return false;
            }

            if (voiceChannel == null)
            {
                await ctx.Channel.SendMessageAsync("É necessário expecificar o canal de voz");
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

            if (voiceChannel == null)
            {
                await ctx.Channel.SendMessageAsync("É necessário expecificar o canal de voz");
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

        private bool ValidateExistMusicWithName(CommandContext ctx, List<MusicModel> musicList)
        {
            if (!musicList.Any())
            {
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
        
        private async ValueTask<bool> ValidateTrackWithUrlAsync(CommandContext ctx, LavalinkLoadResult loadResult, Uri url)
        {
            if (loadResult.LoadResultType == LavalinkLoadResultType.LoadFailed || loadResult.LoadResultType == LavalinkLoadResultType.NoMatches)
            {
                await ctx.Channel.SendMessageAsync("Falha em encontrar música com a URl: " + url);
                return false;
            }

            return true;
        }

        private async ValueTask<bool> ValidateSearchMusicsAsync(CommandContext ctx, List<MusicModel> musicList, string musicName)
        {
            if (musicList == null || !musicList.Any())
            {
                await ctx.Channel.SendMessageAsync("Nenhuma música foi encontrada!");
                return false;
            }

            if (musicName == null || musicName == "")
            {
                await ctx.Channel.SendMessageAsync("É necessário uma palavra-chave para a pesquisa!");
                return false;
            }

            return true;
        }

        #endregion
    }
}
