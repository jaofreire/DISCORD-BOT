using Bot_PLayer_Tauz_2._0.Modules;
using DSharpPlus;
using DSharpPlus.EventArgs;
using DSharpPlus.Interactivity;
using DSharpPlus.Interactivity.Extensions;
using DSharpPlus.Lavalink;
using DSharpPlus.Net;
using DSharpPlus.SlashCommands;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

class Program
{
    static async Task Main(string[] args)
    {
        var builder = Host.CreateApplicationBuilder();

        var discord = new DiscordClient(new DiscordConfiguration()
        {
            Token = builder.Configuration["DiscordEnv:Token"],
            TokenType = DSharpPlus.TokenType.Bot,
            Intents = DiscordIntents.All,
            MinimumLogLevel = LogLevel.Debug
        });

        discord.Ready += IsClientReady;

        static Task IsClientReady(DiscordClient sender, ReadyEventArgs args)
        {
            return Task.CompletedTask;
        }

        var slashCommands = discord.UseSlashCommands();

        slashCommands.RegisterCommands<InteractionModule>(builder.Configuration.GetValue<ulong>("DiscordEnv:GuildId"));

        discord.UseInteractivity( new InteractivityConfiguration()
        {
            Timeout = TimeSpan.FromSeconds(30)
        });

        var endpoint = new ConnectionEndpoint()
        {
            Hostname = builder.Configuration["LavaLink:Hostname"],
            Port = builder.Configuration.GetValue<int>("LavaLink:Port"),
            Secured = true,
        };

        var lavaLinkConfig = new LavalinkConfiguration()
        {
            Password = builder.Configuration["LavaLink:Password"],
            RestEndpoint = endpoint,
            SocketEndpoint = endpoint
        };

        var lavaLink = discord.UseLavalink();

        await discord.ConnectAsync();
        await lavaLink.ConnectAsync(lavaLinkConfig);

        builder.Services.AddLogging(x =>
        {
            x.AddConsole().SetMinimumLevel(LogLevel.Debug);
        });

        await Task.Delay(-1);

        var host = builder.Build();

        await host.RunAsync();

    }

   
}

