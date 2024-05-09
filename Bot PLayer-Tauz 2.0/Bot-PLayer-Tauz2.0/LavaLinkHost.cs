using Discord.Addons.Hosting;
using Discord.WebSocket;
using DSharpPlus.Lavalink;
using DSharpPlus.Lavalink.EventArgs;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Bot_PLayer_Tauz_2._0
{
    public class LavaLinkHost
    {
        private readonly LavalinkGuildConnection? _guildConnection;

        public LavaLinkHost()
        { 
        }

        public LavaLinkHost(LavalinkGuildConnection? guildConnection)
        {
            _guildConnection = guildConnection;
        }

        

    }
}
