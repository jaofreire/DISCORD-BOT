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

namespace WebHostExtensions
{
    public static class MusicConnectExtensions
    {

        public static async void UseDiscordBotMusicSDK(this IServiceCollection services, IConfiguration configuration, DiscordClient discordClient, LavalinkExtension lavaLinkClient, LavalinkConfiguration lavaLinkConfig)
        {
            await discordClient.ConnectAsync();
            await lavaLinkClient.ConnectAsync(lavaLinkConfig);
        }


        public static DiscordClient AddDiscordClientServices(this IServiceCollection services, IConfiguration configuration, string token)
        {
            var discord = new DiscordClient(new DiscordConfiguration()
            {
                Token = token,
                TokenType = TokenType.Bot,
                Intents = DiscordIntents.All,
                MinimumLogLevel = LogLevel.Debug
            });

            AddDiscordClientEventHandler(discord);


            return discord;

        }

        public static LavalinkExtension AddLavaLinkServices(this IServiceCollection services, DiscordClient discord)
        {
            var lavaLink = discord.UseLavalink();

            return lavaLink;
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

        public static void AddClientSlashCommands<T>(this IServiceCollection services,DiscordClient discord, ulong? guildID)
            where T : ApplicationCommandModule
        {
            
            var slashCommands = discord.UseSlashCommands();

            slashCommands.RegisterCommands<T>(guildID);

            discord.UseInteractivity(new InteractivityConfiguration()
            {
                Timeout = TimeSpan.FromSeconds(30)
            });
        }

        public static void AddDiscordClientEventHandler(DiscordClient discord)
        {
            discord.Ready += IsClientReady;

            static Task IsClientReady(DiscordClient sender, ReadyEventArgs args)
            {
                return Task.CompletedTask;
            }
        }
    }

}
