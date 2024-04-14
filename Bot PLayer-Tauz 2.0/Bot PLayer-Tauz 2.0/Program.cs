using Bot_PLayer_Tauz_2._0.DiscordHandlers;
using Discord;
using Discord.Addons.Hosting;
using Discord.WebSocket;
using Lavalink4NET.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = Host.CreateApplicationBuilder();

builder.Services.AddDiscordHost((config, _) =>
{
    config.SocketConfig = new DiscordSocketConfig()
    {
        LogLevel = LogSeverity.Debug,
        AlwaysDownloadUsers = true,
        MessageCacheSize = 1000,
        GatewayIntents = GatewayIntents.All
    };

    config.Token = builder.Configuration["DiscordEnv:Token"];
});

builder.Services.AddInteractionService((config, _) =>
{
    config.LogLevel = LogSeverity.Debug;
});

builder.Services.AddHostedService<CommandHandler>();

builder.Services.AddLavalink();

var host = builder.Build();

await host.RunAsync();