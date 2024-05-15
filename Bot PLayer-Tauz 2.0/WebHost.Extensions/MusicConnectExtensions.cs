using DSharpPlus;
using DSharpPlus.EventArgs;
using DSharpPlus.Interactivity.Extensions;
using DSharpPlus.Interactivity;
using DSharpPlus.SlashCommands;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using DSharpPlus.Lavalink;
using DSharpPlus.Net;
using Microsoft.Extensions.DependencyInjection;
using DSharpPlus.CommandsNext;
using Microsoft.EntityFrameworkCore;
using DSharpPlus.Interactivity.Enums;
using DSharpPlus.Lavalink.EventArgs;

namespace WebHostExtensions
{
    public static class MusicConnectExtensions
    {

        public static async void UseDiscordBotMusicSDK(this IServiceCollection services, IConfiguration configuration, DiscordClient discordClient, LavalinkExtension lavaLinkClient, LavalinkConfiguration lavaLinkConfig)
        {
            await discordClient.ConnectAsync();
            await lavaLinkClient.ConnectAsync(lavaLinkConfig);
        }


        public static LavalinkConfiguration GetLavaLinkConfiguration(this IServiceCollection services,string hostName, int port ,string password)
        {
            var endpoint = new ConnectionEndpoint()
            {
                Hostname = hostName,
                Port = port,
                Secured = true,
            };

            var lavaLinkConfig = new LavalinkConfiguration()
            {
                Password = password,
                RestEndpoint = endpoint,
                SocketEndpoint = endpoint
            };

            return lavaLinkConfig;
        }

       
        

    }

}
