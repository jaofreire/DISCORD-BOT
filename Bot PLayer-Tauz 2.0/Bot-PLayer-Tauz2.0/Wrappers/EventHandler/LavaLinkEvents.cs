using Bot_PLayer_Tauz_2._0.Data.Models;
using DisCatSharp.Lavalink;
using DisCatSharp.Lavalink.Entities;
using DisCatSharp.Lavalink.Enums;
using DisCatSharp.Lavalink.EventArgs;
using Microsoft.Extensions.Caching.Distributed;
using System.IO.Pipelines;
using System.Text.Json;


namespace Bot_PLayer_Tauz_2._0.Wrappers.EventHandler
{
    public class LavaLinkEvents
    {
        public LavalinkGuildPlayer? _guildConnection { get; set; }
        private readonly IDistributedCache _cache;


        public LavaLinkEvents(LavalinkGuildPlayer guildConnection, IDistributedCache cache)
        {
            _guildConnection = guildConnection;
            _cache = cache;
            _guildConnection.TrackEnded += OnTrackEnded;
            
        }

        private async Task OnTrackEnded(LavalinkGuildPlayer sender, LavalinkTrackEndedEventArgs e)
        {
            try
            {
                var discordClient = DiscordClientEvents.discordClient;
                var lavaLinkClient = discordClient.GetLavalink();
                var node = lavaLinkClient.ConnectedSessions.Values.First();


                Console.WriteLine("FIM DA MÚSICA!!");

                var queueJson = await _cache.GetStringAsync(sender.Guild.Id.ToString());

                var musicQueue = JsonSerializer.Deserialize<List<MusicModel>>(queueJson);


                if (musicQueue.Count > 1)
                {
                    var loudResult = await sender.LoadTracksAsync(LavalinkSearchType.Youtube, musicQueue[0].Name);

                    if (loudResult.LoadType == LavalinkLoadResultType.Empty || loudResult.LoadType == LavalinkLoadResultType.Error)
                        return;


                    musicQueue.Remove(musicQueue[0]);

                    queueJson = JsonSerializer.Serialize(musicQueue);
                    await _cache.SetStringAsync(sender.Guild.Id.ToString(), queueJson);

                    var track = loudResult.LoadType switch
                    {
                        LavalinkLoadResultType.Track => loudResult.GetResultAs<LavalinkTrack>(),
                        LavalinkLoadResultType.Playlist => loudResult.GetResultAs<LavalinkPlaylist>().Tracks.First(),
                        LavalinkLoadResultType.Search => loudResult.GetResultAs<List<LavalinkTrack>>().First(),
                        _ => throw new InvalidOperationException("Unexpected load result type")
                    };

                    await sender.PlayAsync(track);

                }
                else if (musicQueue.Count == 1)
                {

                    var loudResult = await sender.LoadTracksAsync(LavalinkSearchType.Youtube, musicQueue[0].Name);

                    if (loudResult.LoadType == LavalinkLoadResultType.Empty || loudResult.LoadType == LavalinkLoadResultType.Error)
                        return;

                    musicQueue.Remove(musicQueue[0]);

                    var track = loudResult.LoadType switch
                    {

                        LavalinkLoadResultType.Track => loudResult.GetResultAs<LavalinkTrack>(),
                        LavalinkLoadResultType.Playlist => loudResult.GetResultAs<LavalinkPlaylist>().Tracks.First(),
                        LavalinkLoadResultType.Search => loudResult.GetResultAs<List<LavalinkTrack>>().First(),
                        _ => throw new InvalidOperationException("Unexpected load result type")
                    };

                    await sender.PlayAsync(track);

                    queueJson = JsonSerializer.Serialize(musicQueue);
                    await _cache.SetStringAsync(sender.Guild.Id.ToString(), queueJson);

                    return;
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Console.WriteLine(ex.InnerException);

                return;
            }
        }

       
    }
}
