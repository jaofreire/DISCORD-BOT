using Bot_PLayer_Tauz_2._0.Data;
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
using WebHostExtensions;

class Program
{
    static async Task Main(string[] args)
    {
        var builder = Host.CreateApplicationBuilder();

        var discordClient = builder.Services.AddDiscordClientServices(builder.Configuration, builder.Configuration["DiscordEnv:Token"]);

        builder.Services.AddClientSlashCommands<InteractionModule>(discordClient, builder.Configuration.GetValue<ulong>("DiscordEnv:GuildId"));

        var lavaLinkClient = builder.Services.AddLavaLinkServices(discordClient);

        var lavaLinkConfig = builder.Services.GetLavaLinkConfiguration(builder.Configuration["LavaLink:Hostname"]
            , builder.Configuration.GetValue<int>("LavaLink:Port")
            , builder.Configuration["LavaLink:Password"]);

        builder.Services.AddLogging(x =>
        {
            x.AddConsole().SetMinimumLevel(LogLevel.Debug);
        });

        builder.Services.UseDiscordBotMusicSDK(builder.Configuration, discordClient, lavaLinkClient, lavaLinkConfig);

        builder.Services.AddSingleton<MongoContext>();

        await Task.Delay(-1);

        var host = builder.Build();

        await host.RunAsync();

    }

   
}

