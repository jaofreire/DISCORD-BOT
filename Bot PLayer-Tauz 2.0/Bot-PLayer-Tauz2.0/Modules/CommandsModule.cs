using Bot_PLayer_Tauz_2._0.Data;
using Bot_PLayer_Tauz_2._0.Data.Models;
using Bot_PLayer_Tauz_2._0.Wrappers.EventHandler;
using DisCatSharp.CommandsNext;
using DisCatSharp.CommandsNext.Attributes;
using DisCatSharp.Entities;
using DisCatSharp.Enums;
using DisCatSharp.Interactivity.Extensions;
using DisCatSharp.Lavalink;
using DisCatSharp.Lavalink.Entities;
using DisCatSharp.Lavalink.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using MongoDB.Bson;
using System;
using System.IO.Pipelines;
using System.Text.Json;





namespace Bot_PLayer_Tauz_2._0.Modules
{
    public class CommandsModule : BaseCommandModule
    {

        private readonly MongoContext _mongoContext;
        private readonly IDistributedCache _cache;
        public static List<MusicModel>? musicListCache { get; set; }
        public LavaLinkEvents? lavaLinkEvents { get; set; }

        public CommandsModule(MongoContext mongoContext, IDistributedCache cache)
        {
            _mongoContext = mongoContext;
            _cache = cache;

            musicListCache = new List<MusicModel>();
 
        }


        #region JOIN/LEAVE

        [Command("Join")]
        [Aliases("j")]
        public async Task JoinAsync(CommandContext ctx ,[RemainingText]DiscordChannel voiceChannel)
        {
            try
            {

                var currentClient = DiscordClientEvents.discordClientsList[0].ShardClients[0];
                var lavaClient = currentClient.GetLavalink();
                var lavaNode = lavaClient.ConnectedSessions.Values.First();
                foreach (var nodes in lavaClient.ConnectedSessions.Keys)
                {
                    Console.WriteLine(nodes.ToString());
                }

                if (!await ValidateJoinAsync(ctx, lavaClient, voiceChannel)) return;

                await lavaNode.ConnectAsync(voiceChannel);
                await ctx.Channel.SendMessageAsync("Join " + voiceChannel.Name);

                var listInitializate = new List<MusicModel>();

                var listJson = JsonSerializer.Serialize(listInitializate);

                await _cache.SetStringAsync(ctx.Guild.Id.ToString(), listJson);
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.Message);
                Console.WriteLine(ex.InnerException);
                return;
            }
            
        }

        [Command("Leave")]
        [Aliases("l")]
        public async Task LeaveAsync(CommandContext ctx, [RemainingText] DiscordChannel voiceChannel)
        {
            try
            {
                var currentClient = DiscordClientEvents.discordClientsList[0].ShardClients[0];
                var lavaClient = currentClient.GetLavalink();
                var lavaNode = lavaClient.ConnectedSessions.Values.First();
                var conn = lavaClient.GetGuildPlayer(ctx.Member.VoiceState.Guild);

                if (!await ValidateLeaveAsync(ctx, lavaClient, conn, voiceChannel)) return;

                await conn.DisconnectAsync();
                await ctx.Channel.SendMessageAsync("Leave " + voiceChannel.Name);

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Console.WriteLine(ex.InnerException);
                return;
            }
        }

        #endregion

        #region DATA HANDLER

        [Command("RegisterNewMusic")]
        [Aliases("rm")]
        public async Task AddNewMusicAsync(CommandContext ctx, [RemainingText]string musicName)
        {
            try
            {
                var lavaClient = ctx.Client.GetLavalink();
                var node = lavaClient.ConnectedSessions.Values.First();
                var conn = lavaClient.GetGuildPlayer(ctx.Member.VoiceState.Guild);

                var loadResult = await conn.LoadTracksAsync(LavalinkSearchType.Youtube,musicName);

                await ctx.Channel.SendMessageAsync("Buscando música...");

                if (!await ValidateTrackAsync(ctx, loadResult, musicName)) return;

                var track = loadResult.LoadType switch
                {
                    LavalinkLoadResultType.Track => loadResult.GetResultAs<LavalinkTrack>(),
                    _ => throw new InvalidOperationException("Unexpected load result type")
                };

                var musicModel = new MusicModel()
                {
                    Name = musicName,
                    Url = track.Info.Uri.ToString()
                };

                await ctx.Channel.SendMessageAsync("Adicionando...");

                await _mongoContext.Musics.AddAsync(musicModel);
                await _mongoContext.SaveChangesAsync();

                await ctx.Channel.SendMessageAsync("Nova música " + musicName + " adicionada");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Console.WriteLine(ex.InnerException);
                return;
            }
        }

        [Command("ListMusics")]
        [Aliases("lm")]
        public async Task ListAllMusicsAsync(CommandContext ctx)
        {
            try
            {
                List<MusicModel> allMusics = await _mongoContext.Musics.ToListAsync();

                await ctx.Channel.SendMessageAsync("Buscando...!");

                foreach (var musics in allMusics)
                {
                    await ctx.Channel.SendMessageAsync($"Nome: {musics.Name.ToString().ToUpper()}\n Url: {musics.Url.ToString()}\n ----------------");
                }

                await ctx.Channel.SendMessageAsync("Músicas encontradas!!");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Console.WriteLine(ex.InnerException);
                return;
            }

        }

        [Command("SearchMusics")]
        [Aliases("sm")]
        public async Task SearchMusicsAsync(CommandContext ctx, [RemainingText]string musicName)
        {
            try
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
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Console.WriteLine(ex.InnerException);
                return;
            }

        }

        #endregion

        #region TRACK HANDLER

        [Command("Play")]
        [Aliases("p")]
        public async Task PlayTrackWithName(CommandContext ctx, [RemainingText]string musicName)
        {
            try
            {

                var currentClient = DiscordClientEvents.discordClientsList[0].ShardClients[0];
                var lavaClient = currentClient.GetLavalink();
                var node = lavaClient.ConnectedSessions.Values.First();
                var conn = node.GetGuildPlayer(ctx.Member.VoiceState.Guild);
                foreach (var nodes in lavaClient.ConnectedSessions.Keys)
                {
                    Console.WriteLine(nodes.ToString());
                }

                if (conn == null)
                {
                    await ctx.Channel.SendMessageAsync("É necessário estar em um canal de voz");
                    return;
                }

                if (conn.CurrentTrack == null) lavaLinkEvents = new LavaLinkEvents(conn, _cache);

                var getMusics = await _mongoContext.Musics.Where(x => x.Name.Contains(musicName)).ToListAsync();

                if (!await ValidateIfUserInTheChannel(ctx)) return;

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

                                await ctx.Channel.SendMessageAsync($"Escolha uma das músicas encontradas com o nome {musicName}\n Digite o Id da música desejada");

                                foreach (var musics in getMusics)
                                {
                                    await ctx.Channel.SendMessageAsync($" Id: {musics.Id} - \n Nome: {musics.Name}\n Url: {musics.Url}\n ---------------");

                                    IdList.Add(musics.Id.ToString());
                                }

                                var responseMessageInTheList = await ctx.Message.GetNextMessageAsync(m =>
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

                                    if (conn.CurrentTrack != null)
                                    {
                                        AddMusicInTheQueue(ctx, node, conn, musicName);
                                        return;
                                    }

                                    var id = new ObjectId(responseMessageInTheList.Result.Content);
                                    var getMusicById = await _mongoContext.Musics.FirstOrDefaultAsync(x => x.Id == id);

                                    var uriId = new Uri(getMusicById.Url);
                                    PlayMusicWithUrlAsync(ctx, node, conn, uriId);

                                    return;
                                }

                                await ctx.Channel.SendMessageAsync("Tempo de resposta excedido");
                                
                            }

                            if (conn.CurrentTrack != null)
                            {
                                AddMusicInTheQueue(ctx, node, conn, musicName);
                                return;
                            }

                            var uri = new Uri(getMusics[0].Url);

                            PlayMusicWithUrlAsync(ctx, node, conn, uri);
                            return;
                        }


                    }

                }

                if (conn.CurrentTrack != null)
                {
                    AddMusicInTheQueue(ctx, node, conn, musicName);
                    return;
                }

                PlayMusicAsync(ctx, node, conn, musicName);

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Console.WriteLine(ex.InnerException);
                return;
            }

        }


        [Command("PlayUrl")]
        [Aliases("purl")]
        public async Task PlayTrackWithUrl(CommandContext ctx, Uri url)
        {
            try
            {
                var currentClient = DiscordClientEvents.discordClientsList[0].ShardClients[0];
                var lavaClient = currentClient.GetLavalink();
                var node = lavaClient.ConnectedSessions.Values.First();
                var conn = node.GetGuildPlayer(ctx.Member.VoiceState.Guild);

                if (!await ValidatePlayTrackAsync(ctx, conn)) return;

                var uri = url.ToString();
                var loadResult = await conn.LoadTracksAsync(LavalinkSearchType.Youtube, uri);

                if (!await ValidateTrackWithUrlAsync(ctx, loadResult, url)) return;

                var track = loadResult.LoadType switch
                {
                    LavalinkLoadResultType.Track => loadResult.GetResultAs<LavalinkTrack>(),
                    LavalinkLoadResultType.Playlist => loadResult.GetResultAs<LavalinkPlaylist>().Tracks.First(),
                    LavalinkLoadResultType.Search => loadResult.GetResultAs<List<LavalinkTrack>>().First(),
                    _ => throw new InvalidOperationException("Unexpected load result type")
                };

                await conn.PlayAsync(track);
                await ctx.Channel.SendMessageAsync("Tocando " + track.Info.Title + " url: " + track.Info.Uri);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Console.WriteLine(ex.InnerException);
                return;
            }

        }

        [Command("Pause")]
        [Aliases("pa")]
        public async Task PauseAsync(CommandContext ctx)
        {
            try
            {
                var currentClient = DiscordClientEvents.discordClientsList[0].ShardClients[0];
                var lavaClient = currentClient.GetLavalink();
                var node = lavaClient.ConnectedSessions.Values.First();
                var conn = node.GetGuildPlayer(ctx.Member.VoiceState.Guild);

                if (!await ValidatePauseResumeAsync(ctx, conn)) return;

                await conn.PauseAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Console.WriteLine(ex.InnerException);
                return;
            }
        }

        [Command("Resume")]
        [Aliases("re")]
        public async Task ResumeAsync(CommandContext ctx)
        {
            try
            {
                var currentClient = DiscordClientEvents.discordClientsList[0].ShardClients[0];
                var lavaClient = currentClient.GetLavalink();
                var node = lavaClient.ConnectedSessions.Values.First();
                var conn = node.GetGuildPlayer(ctx.Member.VoiceState.Guild);

                if (!await ValidatePauseResumeAsync(ctx, conn)) return;

                await conn.ResumeAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Console.WriteLine(ex.InnerException);

                return;
            }

        }

        [Command("Stop")]
        [Aliases("st")]
        public async Task StopAsync(CommandContext ctx)
        {
            try
            {
                var currentClient = DiscordClientEvents.discordClientsList[0].ShardClients[0];
                var lavaClient = currentClient.GetLavalink();
                var node = lavaClient.ConnectedSessions.Values.First();
                var conn = node.GetGuildPlayer(ctx.Member.VoiceState.Guild);

                if (!await ValidateStopAsync(ctx, conn)) return;

                await conn.StopAsync();


            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.Message);
                Console.WriteLine(ex.InnerException);

                return;
            }
        }



        private async void PlayMusicAsync(CommandContext ctx, LavalinkSession node, LavalinkGuildPlayer conn, string musicName)
        {
            if (!await ValidatePlayTrackAsync(ctx, conn)) return;

            var loadResult = await conn.LoadTracksAsync(LavalinkSearchType.Youtube, musicName);

            if (!await ValidateTrackAsync(ctx, loadResult, musicName)) return;

            var track = loadResult.LoadType switch
            {
                LavalinkLoadResultType.Track => loadResult.GetResultAs<LavalinkTrack>(),
                LavalinkLoadResultType.Playlist => loadResult.GetResultAs<LavalinkPlaylist>().Tracks.First(),
                LavalinkLoadResultType.Search => loadResult.GetResultAs<List<LavalinkTrack>>().First(),
                _ => throw new InvalidOperationException("Unexpected load result type")
            };

            var model = new MusicModel()
            {
                Name = track.Info.Title,
                Url = track.Info.Uri.ToString()
            };

            musicListCache.Add(model);

            await conn.PlayAsync(track);
            await ctx.Channel.SendMessageAsync("Tocando " + track.Info.Title + " url: " + track.Info.Uri);
        }

        private async void PlayMusicWithUrlAsync(CommandContext ctx, LavalinkSession node, LavalinkGuildPlayer conn, Uri Url)
        {
            if (!await ValidatePlayTrackAsync(ctx, conn)) return;

            var uri = Url.ToString();
            var loadResult = await conn.LoadTracksAsync(LavalinkSearchType.Youtube, uri);

            if (!await ValidateTrackWithUrlAsync(ctx, loadResult, Url)) return;

            var track = loadResult.LoadType switch
            {
                LavalinkLoadResultType.Track => loadResult.GetResultAs<LavalinkTrack>(),
                LavalinkLoadResultType.Playlist => loadResult.GetResultAs<LavalinkPlaylist>().Tracks.First(),
                LavalinkLoadResultType.Search => loadResult.GetResultAs<List<LavalinkTrack>>().First(),
                _ => throw new InvalidOperationException("Unexpected load result type")
            };

            await conn.PlayAsync(track);
            await ctx.Channel.SendMessageAsync("Tocando " + track.Info.Title + " url: " + track.Info.Uri);
        }

        private async void AddMusicInTheQueue(CommandContext ctx ,LavalinkSession node, LavalinkGuildPlayer conn, string musicName)
        {

            var loadResult = await conn.LoadTracksAsync(LavalinkSearchType.Youtube, musicName);

            if (!await ValidateTrackAsync(ctx, loadResult, musicName)) return;

            var track = loadResult.LoadType switch
            {
                LavalinkLoadResultType.Track => loadResult.GetResultAs<LavalinkTrack>(),
                LavalinkLoadResultType.Playlist => loadResult.GetResultAs<LavalinkPlaylist>().Tracks.First(),
                LavalinkLoadResultType.Search => loadResult.GetResultAs<List<LavalinkTrack>>().First(),
                _ => throw new InvalidOperationException("Unexpected load result type")
            };

            var model = new MusicModel()
            {
                Name = track.Info.Title,
                Url = track.Info.Uri.ToString()
            };

            var queueJson = await _cache.GetStringAsync(ctx.Guild.Id.ToString());

            if (queueJson == null)
            {
                await ctx.Channel.SendMessageAsync("Não há nenhuma lista de espera");
                return;
            }

            var queue = JsonSerializer.Deserialize<List<MusicModel>>(queueJson);

            if (queue.Count == 0)
            {
                musicListCache.Clear();
                queue = musicListCache;
                queue.Add(model);

                var queueJsonSerialize = JsonSerializer.Serialize(queue);

                await _cache.SetStringAsync(ctx.Guild.Id.ToString(), queueJsonSerialize);

                await ctx.Channel.SendMessageAsync("Adicionando música á lista de música em espera");

                await ctx.Channel.SendMessageAsync("Sucesso!");

                await ctx.Channel.SendMessageAsync("Buscando lista de espera...");

                await Task.Delay(4000);
                int countIfEqualZero = 0;
                foreach (var musics in queue)
                {
                    countIfEqualZero++;
                    await ctx.Channel.SendMessageAsync($"- {countIfEqualZero}  Nome: {musics.Name}");
                }

                return;
            }

            if (queue.Count >= 1)
            {
                queueJson = await _cache.GetStringAsync(ctx.Guild.Id.ToString());
                queue = JsonSerializer.Deserialize<List<MusicModel>>(queueJson);

                queue.Add(model);

                await ctx.Channel.SendMessageAsync("Adicionando música á lista de música em espera");

                await ctx.Channel.SendMessageAsync("Sucesso!");

                await ctx.Channel.SendMessageAsync("Buscando lista de espera...");

                await Task.Delay(4000);

                int countIfMoreThanOne = 0;
                foreach (var musics in queue)
                {
                    countIfMoreThanOne++;
                    await ctx.Channel.SendMessageAsync($"- {countIfMoreThanOne}  Nome: {musics.Name}");
                }

                queueJson = JsonSerializer.Serialize(queue);

                await _cache.SetStringAsync(ctx.Guild.Id.ToString(), queueJson);

                return;
            }
           
        }

        #endregion

        #region QUEUE HANDLER

        [Command("Queue")]
        [Aliases("q")]
        public async Task ShowQueueAsync(CommandContext ctx)
        {
            try
            {
                var queueJson = await _cache.GetStringAsync(ctx.Guild.Id.ToString());

                if (queueJson == null)
                {
                    await ctx.Channel.SendMessageAsync("Não há nenhuma lista de espera");
                    return;
                }

                var queue = JsonSerializer.Deserialize<List<MusicModel>>(queueJson);

                if (queue.Count == 0)
                {
                    await ctx.Channel.SendMessageAsync("Lista de espera vazia");
                    return;
                }

                await ctx.Channel.SendMessageAsync("Listando...");

                await Task.Delay(4000);

                int count = 0;
                foreach (var musics in queue)
                {
                    count++;
                    await ctx.Channel.SendMessageAsync($"- {count}  Nome: {musics.Name}");
                }


            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Console.WriteLine(ex.InnerException);

                return;
            }
        }

        [Command("ClearQueue")]
        [Aliases("cq")]
        public async Task ClearQueue(CommandContext ctx)
        {
            var queueJson = await _cache.GetStringAsync(ctx.Guild.Id.ToString());

            if (queueJson == null)
            {
                await ctx.Channel.SendMessageAsync("Não há nenhuma lista de espera");
                return;
            }

            var queue = JsonSerializer.Deserialize<List<MusicModel>>(queueJson);

            if (queue.Count == 0 || queue == null)
            {
                await ctx.Channel.SendMessageAsync("Não há musicas listadas");
                return;
            }

            await ctx.Channel.SendMessageAsync("Removando TODAS as músicas da lista de espera");

            queue.Clear();

            var queueCleardJson = JsonSerializer.Serialize(queue);

            await _cache.SetStringAsync(ctx.Guild.Id.ToString(), queueCleardJson);
        }

        #endregion

        #region VALIDATIONS

        private async ValueTask<bool> ValidateJoinAsync(CommandContext ctx, LavalinkExtension lavaClient, DiscordChannel voiceChannel)
        {

            if (!lavaClient.ConnectedSessions.Any())
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

        private async ValueTask<bool> ValidateLeaveAsync(CommandContext ctx, LavalinkExtension lavaClient, LavalinkGuildPlayer conn, DiscordChannel voiceChannel)
        {

            if (!lavaClient.ConnectedSessions.Any())
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

        private async ValueTask<bool> ValidatePlayTrackAsync(CommandContext ctx, LavalinkGuildPlayer conn)
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

        private async ValueTask<bool> ValidateIfUserInTheChannel(CommandContext ctx)
        {
            if (ctx.Member.VoiceState == null || ctx.Member.VoiceState.Channel == null)
            {
                await ctx.Channel.SendMessageAsync("É necessário estar em um canal de voz");

                return false;
            }

            return true;
        }

        private async ValueTask<bool> ValidatePauseResumeAsync(CommandContext ctx, LavalinkGuildPlayer conn)
        {
            if (ctx.Member.VoiceState == null || ctx.Member.VoiceState.Channel == null)
            {
                await ctx.Channel.SendMessageAsync("É necessário estar em um canal de voz");
                return false;
            }

            if (conn == null)
            {
                await ctx.Channel.SendMessageAsync("LavaLink não esta conectado");
                return false;
            }

            if (conn.CurrentTrack == null)
            {
                await ctx.Channel.SendMessageAsync("Nenhuma música esta tocando");
                return false;
            }

            return true;
        }

        private async ValueTask<bool> ValidateStopAsync(CommandContext ctx, LavalinkGuildPlayer conn)
        {
            if (ctx.Member.VoiceState == null || ctx.Member.VoiceState.Channel == null)
            {
                await ctx.Channel.SendMessageAsync("É necessário estar em um canal de voz");
                return false;
            }

            if (conn == null)
            {
                await ctx.Channel.SendMessageAsync("LavaLink não esta conectado");
                return false;
            }

            if (conn.CurrentTrack == null)
            {
                await ctx.Channel.SendMessageAsync("Nenhuma música esta tocando");
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

        private async ValueTask<bool> ValidateTrackAsync(CommandContext ctx, LavalinkTrackLoadingResult loadResult, string musicName)
        {
            if (loadResult.LoadType == LavalinkLoadResultType.Empty || loadResult.LoadType == LavalinkLoadResultType.Error)
            {
                await ctx.Channel.SendMessageAsync("Falha em encontrar " + musicName);
                return false;
            }

            return true;
        }
        
        private async ValueTask<bool> ValidateTrackWithUrlAsync(CommandContext ctx, LavalinkTrackLoadingResult loadResult, Uri url)
        {
            if (loadResult.LoadType == LavalinkLoadResultType.Empty || loadResult.LoadType == LavalinkLoadResultType.Error)
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
