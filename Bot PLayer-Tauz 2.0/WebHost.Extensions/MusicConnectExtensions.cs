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

        public static void AddClientSlashCommands<T>(this IServiceCollection services,DiscordClient discord, ulong? guildId)
            where T : ApplicationCommandModule
        {

            var slashCommands = discord.UseSlashCommands();
         
            slashCommands.RegisterCommands<T>(guildId);

            discord.UseInteractivity(new InteractivityConfiguration()
            {
                PollBehaviour = PollBehaviour.KeepEmojis,
                Timeout = TimeSpan.FromSeconds(30)
            });
        }

        public static void AddClientSlashCommands<T>(this IServiceCollection services,DiscordClient discord, ulong? guildId, ServiceProvider dependenciesServices)
            where T : ApplicationCommandModule
        {

            var slashCommands = discord.UseSlashCommands(new SlashCommandsConfiguration()
            {
                Services = dependenciesServices,
            });
         
            slashCommands.RegisterCommands<T>(guildId);

            discord.UseInteractivity(new InteractivityConfiguration()
            {
                PollBehaviour = PollBehaviour.KeepEmojis,
                Timeout = TimeSpan.FromSeconds(30)
            });
        }
        
        public static void AddClientCommandsNext<T>(this IServiceCollection services,DiscordClient discord, string prefix)
            where T : BaseCommandModule
        {
            var commands = discord.UseCommandsNext(new CommandsNextConfiguration()
            {
                StringPrefixes = new[] { prefix }
            });

            commands.RegisterCommands<T>();

            discord.UseInteractivity(new InteractivityConfiguration()
            {
                PollBehaviour = PollBehaviour.KeepEmojis,
                Timeout = TimeSpan.FromSeconds(30)
            });
        }

        public static void AddClientCommandsNext<T>(this IServiceCollection services,DiscordClient discord, string prefix, ServiceProvider dependenciesServices)
            where T : BaseCommandModule
        {
            var commands = discord.UseCommandsNext(new CommandsNextConfiguration()
            {
                StringPrefixes = new[] { prefix },
                Services = dependenciesServices
                
            });

            commands.RegisterCommands<T>();

            discord.UseInteractivity(new InteractivityConfiguration()
            {
                PollBehaviour = PollBehaviour.KeepEmojis,
                Timeout = TimeSpan.FromMinutes(3)
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
