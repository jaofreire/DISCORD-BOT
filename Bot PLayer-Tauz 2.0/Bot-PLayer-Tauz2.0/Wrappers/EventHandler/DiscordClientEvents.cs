
using DSharpPlus;
using DSharpPlus.EventArgs;
using DSharpPlus.Lavalink;


namespace Bot_PLayer_Tauz_2._0.Wrappers.EventHandler
{
    public class DiscordClientEvents
    {
        public static List<ulong> guildsIdList = new List<ulong>();
        public static DiscordClient? discordClient {  get; set; }

        public static async Task IsReady(DiscordClient sender, ReadyEventArgs args)
        {
            var guilds = sender.Guilds;

            discordClient = sender;

            foreach (var guildIds in guilds)
            {
                guildsIdList.Add(guildIds.Key);
            }

            int count = 0;
            foreach (var allGuilsId in guildsIdList)
            {
                count++;
                Console.WriteLine($"Guild:{count}, Id: {allGuilsId}");
            }

            Console.WriteLine($"Total Guilds Connected: {count}");
        }
    }
}
