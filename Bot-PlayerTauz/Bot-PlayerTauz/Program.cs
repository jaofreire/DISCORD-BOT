
using Bot_PlayerTauz.Discord;
using Discord;
using Discord.Addons.Hosting;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

class Program
{

    static async Task Main()
    {
        var builder = new HostBuilder()
            .ConfigureAppConfiguration(x =>
            {
                var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", false, true)
                .Build();

                x.AddConfiguration(configuration);
            })
            .ConfigureLogging(x =>
            {
                x.AddConsole();
                x.SetMinimumLevel(LogLevel.Debug);
            })
            .ConfigureServices((context, services) =>
            {
     
                services.AddDiscordHost((config, _) =>
                {
                    config.SocketConfig = new DiscordSocketConfig()
                    {
                        LogLevel = LogSeverity.Verbose,
                        MessageCacheSize = 1000,
                        AlwaysDownloadUsers = true,
                    };

                    config.Token = context.Configuration["DiscordEnv:Token"];
                });

                services.AddCommandService((config, _) =>
                {
                    config.CaseSensitiveCommands = false;
                    config.LogLevel = LogSeverity.Debug;
                    config.DefaultRunMode = RunMode.Async;
                });

                
                services.AddHostedService<CommandHandler>();

            })
            .UseConsoleLifetime();

        var host = builder.Build();

        await host.RunAsync();









    }
}