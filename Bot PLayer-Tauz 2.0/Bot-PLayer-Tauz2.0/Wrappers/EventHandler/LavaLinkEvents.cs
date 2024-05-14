using Bot_PLayer_Tauz_2._0.Modules;
using DSharpPlus.Lavalink;
using DSharpPlus.Lavalink.EventArgs;


namespace Bot_PLayer_Tauz_2._0.Wrappers.EventHandler
{
    public class LavaLinkEvents
    {
        public LavalinkGuildConnection? _guildConnection { get; set; }
        private int musiCount = 0;

        public LavaLinkEvents(LavalinkGuildConnection guildConnection)
        {
            _guildConnection = guildConnection;
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

                var musicQueue = CommandsModule.musicListCache;


                if (musicQueue.Count > 1)
                {
                    musicQueue.Remove(musicQueue[0]);

                    int count = 0;
                    foreach (var musics in musicQueue)
                    {
                        count++;
                        await sender.Channel.SendMessageAsync($"- {count}  Nome:{musics.Name}");
                    }

                    await Task.Delay(4000);

                    var loudResult = node.Rest.GetTracksAsync(musicQueue[0].Name);

                    if (loudResult.Result.LoadResultType == LavalinkLoadResultType.LoadFailed || loudResult.Result.LoadResultType == LavalinkLoadResultType.NoMatches)
                    {
                        await sender.Channel.SendMessageAsync("Ocorreu um erro ao buscar a música da lista!");
                        return;
                    }

                    var track = loudResult.Result.Tracks.First();

                    await sender.PlayAsync(track);
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
