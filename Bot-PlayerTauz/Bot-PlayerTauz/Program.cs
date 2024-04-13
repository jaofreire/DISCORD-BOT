using Bot_PlayerTauz.Discord;
using Discord;
using Discord.Addons.Hosting;
using Discord.Commands;
using Discord.WebSocket;
using Lavalink4NET.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;


var builder = Host.CreateApplicationBuilder();

builder.Services.AddDiscordHost((config, _) =>
{
    config.SocketConfig = new DiscordSocketConfig()
    {
        LogLevel = LogSeverity.Verbose,
        MessageCacheSize = 1000,
        AlwaysDownloadUsers = true,
        GatewayIntents = GatewayIntents.All
    };

    config.Token = builder.Configuration["DiscordEnv:Token"];

});

builder.Services.AddCommandService((config, _) =>
{
    config.CaseSensitiveCommands = false;
    config.LogLevel = LogSeverity.Debug;
    config.DefaultRunMode = RunMode.Async;
});


builder.Services.AddHostedService<CommandHandler>();

builder.Services.AddLavalinkCore();

var host = builder.Build();

await host.RunAsync();
