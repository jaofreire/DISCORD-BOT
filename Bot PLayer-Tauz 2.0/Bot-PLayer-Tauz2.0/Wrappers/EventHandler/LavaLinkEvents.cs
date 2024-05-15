using Bot_PLayer_Tauz_2._0.Data.Models;
using DSharpPlus.Lavalink;
using DSharpPlus.Lavalink.EventArgs;
using Microsoft.Extensions.Caching.Distributed;
using System.Text.Json;


namespace Bot_PLayer_Tauz_2._0.Wrappers.EventHandler
{
    public class LavaLinkEvents
    {
        public LavalinkGuildConnection? _guildConnection { get; set; }
        private readonly IDistributedCache _cache;

        private int musiCount = 0;

        public LavaLinkEvents(LavalinkGuildConnection guildConnection, IDistributedCache cache)
        {
            _guildConnection = guildConnection;
            _cache = cache;
            _guildConnection.PlaybackFinished += OnPlayBackFinished;
            
        }

        private async Task OnPlayBackFinished(LavalinkGuildConnection sender, TrackFinishEventArgs args)
        {
            try
            {
                var discordClient = DiscordClientEvents.discordClient;
                var lavaLinkClient = discordClient.GetLavalink();
                var node = lavaLinkClient.ConnectedNodes.Values.First();


                Console.WriteLine("FIM DA MÚSICA!!");

                var queueJson = await _cache.GetStringAsync(sender.Guild.Id.ToString());

                var musicQueue = JsonSerializer.Deserialize<List<MusicModel>>(queueJson);


                if (musicQueue.Count > 1)
                {
                    var loudResult = await node.Rest.GetTracksAsync(musicQueue[0].Name);

                    if (loudResult.LoadResultType == LavalinkLoadResultType.LoadFailed || loudResult.LoadResultType == LavalinkLoadResultType.NoMatches)
                        return;
                   

                    musicQueue.Remove(musicQueue[0]);

                    queueJson = JsonSerializer.Serialize(musicQueue);
                    await _cache.SetStringAsync(sender.Guild.Id.ToString(), queueJson);

                    var track = loudResult.Tracks.First();

                    await sender.PlayAsync(track);

                }
                else if (musicQueue.Count == 1)
                {

                    var loudResult = await node.Rest.GetTracksAsync(musicQueue[0].Name);

                    if (loudResult.LoadResultType == LavalinkLoadResultType.LoadFailed || loudResult.LoadResultType == LavalinkLoadResultType.NoMatches)
                        return;
                    
                    musicQueue.Remove(musicQueue[0]);

                    var track = loudResult.Tracks.First();

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
