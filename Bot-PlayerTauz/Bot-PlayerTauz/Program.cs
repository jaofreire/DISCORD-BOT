
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
                //Discord Socket Service
                var discordConfig = new DiscordSocketConfig()
                {
                    LogLevel = LogSeverity.Verbose,
                    MessageCacheSize = 1000,
                    AlwaysDownloadUsers = true,
                };

                var discordSocketClient = new DiscordSocketClient(discordConfig);

                var token = context.Configuration["DiscordEnv:Token"];


                //Command Service
                var commandConfig = new CommandServiceConfig()
                {
                    CaseSensitiveCommands = false,
                    LogLevel = LogSeverity.Debug,
                    DefaultRunMode = RunMode.Sync
                };

                var commandService = new CommandService(commandConfig);


                services.AddSingleton(discordSocketClient);

                discordSocketClient.LoginAsync(TokenType.Bot, token);
                discordSocketClient.StartAsync();

            })
            .UseConsoleLifetime();
          

        var host = builder.Build();

        using (host)
        {
            await host.RunAsync();
        }
          
    }
}