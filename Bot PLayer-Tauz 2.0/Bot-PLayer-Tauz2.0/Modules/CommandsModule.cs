using Bot_PLayer_Tauz_2._0.Models;
using Bot_PLayer_Tauz_2._0.Wrappers.EventHandler;
using DisCatSharp.CommandsNext;
using DisCatSharp.CommandsNext.Attributes;
using DisCatSharp.Entities;
using DisCatSharp.Enums;
using DisCatSharp.Lavalink;
using DisCatSharp.Lavalink.Entities;
using DisCatSharp.Lavalink.Enums;
using Microsoft.Extensions.Caching.Distributed;
using System.Text.Json;





namespace Bot_PLayer_Tauz_2._0.Modules
{
    public class CommandsModule : BaseCommandModule
    {

        private readonly IDistributedCache _cache;
        public static List<MusicModel>? musicListCache { get; set; }
        public LavaLinkEvents? lavaLinkEvents { get; set; }
        

        public CommandsModule(IDistributedCache cache)
        {
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

                var listInitializate = new List<LavalinkTrack>();

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

        #region TRACK HANDLER

        [Command("Play")]
        [Aliases("p")]
        public async Task PlayTrackWithName(CommandContext ctx, [RemainingText]string musicQuery)
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

                if (conn.CurrentTrack == null) lavaLinkEvents = new LavaLinkEvents(conn, _cache, this);

                if (!await ValidateIfUserInTheChannel(ctx)) return;

                if (conn.CurrentTrack != null)
                {
                   await AddMusicInTheQueue(ctx, conn, musicQuery);
                    return;
                }

               await PlayMusicAsync(ctx, musicQuery);

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

        public async Task EventPlayMusicAsync(DiscordGuild guild, LavalinkTrack track)
        {
            var currentClient = DiscordClientEvents.discordClientsList[0].ShardClients[0];
            var lavaClient = currentClient.GetLavalink();
            var node = lavaClient.ConnectedSessions.Values.First();
            var conn = node.GetGuildPlayer(guild);

            await conn.PlayAsync(track);
 
        }


        private async Task PlayMusicAsync(CommandContext ctx, string musicName)
        {

            var currentClient = DiscordClientEvents.discordClientsList[0].ShardClients[0];
            var lavaClient = currentClient.GetLavalink();
            var node = lavaClient.ConnectedSessions.Values.First();
            var conn = node.GetGuildPlayer(ctx.Member.VoiceState.Guild);

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


        private async Task AddMusicInTheQueue(CommandContext ctx , LavalinkGuildPlayer conn, string musicName)
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


            var queue = await DeserializeMusicList(ctx);
            if (queue == null) return;

            if (queue.Count == 0)
            {
                musicListCache.Clear();
                queue = musicListCache;
                queue.Add(model);

                await SerializeMusicList(ctx, queue);

                await ctx.Channel.SendMessageAsync("Adicionando música á lista de música em espera\n" +
                    "Sucesso\n" +
                    "Buscando lista de espera...");


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
                queue = await DeserializeMusicList(ctx);

                queue.Add(model);

                await ctx.Channel.SendMessageAsync("Adicionando música á lista de música em espera\n" +
                     "Sucesso\n" +
                     "Buscando lista de espera...");


                await Task.Delay(4000);

                int countIfMoreThanOne = 0;
                foreach (var musics in queue)
                {
                    countIfMoreThanOne++;
                    await ctx.Channel.SendMessageAsync($"- {countIfMoreThanOne}  Nome: {musics.Name}");
                }

                await SerializeMusicList(ctx, queue);

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
                var queue = await DeserializeMusicList(ctx);
                if (queue == null) return;

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
            var queue = await DeserializeMusicList(ctx);
            if(queue == null) return;

            await ctx.Channel.SendMessageAsync("Removando TODAS as músicas da lista de espera");

            queue.Clear();

            await SerializeMusicList(ctx, queue);
        }



        #endregion

        #region JSON SERIALIZER/DESERIALIZER
        private async ValueTask<List<MusicModel>> DeserializeMusicList(CommandContext ctx)
        {
            var queueJson = await _cache.GetStringAsync(ctx.Guild.Id.ToString());

            if (queueJson == null)
            {
                await ctx.Channel.SendMessageAsync("Não há nenhuma lista de espera");
                return null;
            }

            var queue = JsonSerializer.Deserialize<List<MusicModel>>(queueJson);

            return queue;
        }

        private async Task SerializeMusicList(CommandContext ctx, List<MusicModel> queue)
        {
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

        private async ValueTask<bool> ValidateTrackAsync(CommandContext ctx, LavalinkTrackLoadingResult loadResult, string musicName)
        {
            if (loadResult.LoadType == LavalinkLoadResultType.Empty || loadResult.LoadType == LavalinkLoadResultType.Error)
            {
                await ctx.Channel.SendMessageAsync("Falha em encontrar " + musicName);
                return false;
            }

            return true;
        }


        #endregion

    }
}
