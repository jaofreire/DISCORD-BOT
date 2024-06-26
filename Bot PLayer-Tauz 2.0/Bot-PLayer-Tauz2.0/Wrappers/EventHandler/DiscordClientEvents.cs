﻿using DisCatSharp;
using DisCatSharp.EventArgs;
using DisCatSharp.Lavalink;

namespace Bot_PLayer_Tauz_2._0.Wrappers.EventHandler
{
    public class DiscordClientEvents
    {
        public static List<ulong> guildsIdList = new List<ulong>();

        public static List<DiscordShardedClient> discordClientsList = new List<DiscordShardedClient>();

        public static List<LavalinkConfiguration> lavaLinkConfigurationList = new List<LavalinkConfiguration>();

        public static DiscordClient? discordClient {  get; set; }

        public static async Task IsReady(DiscordClient sender, ReadyEventArgs e)
        {

            guildsIdList.Clear();
            var guilds = sender.Guilds;
            
            discordClient = sender;

            foreach (var guildIds in guilds)
            {
                guildsIdList.Add(guildIds.Key);
            }

            //Console.WriteLine($"ShardCount: {sender.ShardCount}");
            Console.WriteLine($"Total Guilds Connected: {guildsIdList.Count}");

        }

    }
}
