using DSharpPlus.Lavalink;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bot_PLayer_Tauz_2._0.Wrappers
{
    public class LavaLinkConnectionManager
    {
        public Dictionary<ulong, LavalinkGuildConnection> connections = new Dictionary<ulong, LavalinkGuildConnection>();

       
        public void AddConnection(ulong guildId, LavalinkGuildConnection newConnection)
        {
            connections[guildId] = newConnection;
        }

        public LavalinkGuildConnection GetConnection(ulong guildId)
        {
            if(connections.ContainsKey(guildId))
                return connections[guildId];

            return null;
        }
    }
}
