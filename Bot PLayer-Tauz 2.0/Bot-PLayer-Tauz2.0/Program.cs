using Bot_PLayer_Tauz_2._0;
using Bot_PLayer_Tauz_2._0.Modules;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using WebHostExtensions;
using Bot_PLayer_Tauz_2._0.Wrappers.EventHandler;
using DisCatSharp;
using DisCatSharp.Enums;
using DisCatSharp.CommandsNext;
using DisCatSharp.Interactivity.Extensions;
using DisCatSharp.Interactivity;
using DisCatSharp.Lavalink;


class Program
{

    static async Task Main(string[] args)
    {
        var builder = Host.CreateApplicationBuilder();

        string stage = Configurations.ProductionStage;


        var discordShardedClient = new DiscordShardedClient(new DiscordConfiguration()
        {
            Token = stage == Configurations.ProductionStage ? builder.Configuration["DiscordEnv:Token"] : builder.Configuration["DiscordEnv:TokenTest"],
            TokenType = TokenType.Bot,
            Intents = DiscordIntents.All,
            MinimumLogLevel = LogLevel.Debug,
            AutoReconnect = true,
            UseRelativeRatelimit = true
        });

        List<DiscordShardedClient> shardedClientsList = [discordShardedClient];

        var dependenciesServices = new ServiceCollection()
            .AddStackExchangeRedisCache(options =>
            {
                if (stage == Configurations.ProductionStage)
                {
                    options.Configuration = builder.Configuration["RedisProd:ConnectionStrings"];
                }
                else if (stage == Configurations.DevelopmentStage)
                {
                    options.Configuration = builder.Configuration["Redis:ConnectionStrings"];
                }

            })
            .AddSingleton<DiscordClientEvents>()
            .BuildServiceProvider();

        foreach (var shardsClient in shardedClientsList)
        {
            shardsClient.Ready += DiscordClientEvents.IsReady;

            var commands = await shardsClient.UseCommandsNextAsync(new CommandsNextConfiguration()
            {
                StringPrefixes = [stage == Configurations.ProductionStage ? builder.Configuration["DiscordEnv:Prefix"] : builder.Configuration["DiscordEnv:PrefixTest"]],
                ServiceProvider = dependenciesServices
            });

            commands.RegisterCommands<CommandsModule>();

            await shardsClient.UseInteractivityAsync(new InteractivityConfiguration()
            {
                Timeout = TimeSpan.FromMinutes(3)
            });
        }

        var lavaLinkConfig1 = builder.Services.GetLavaLinkConfiguration(builder.Configuration["LavaLink1:Hostname"]
            , builder.Configuration.GetValue<int>("LavaLink1:Port")
            , builder.Configuration["LavaLink1:Password"]);


        List<LavalinkConfiguration> lavaLinkServers = [lavaLinkConfig1];
        Console.WriteLine($"LavaLink Credentials1: Hostname: {builder.Configuration["LavaLink1:Hostname"]}," +
            $" Port:{builder.Configuration.GetValue<int>("LavaLink1:Port")}" +
            $" Password: {builder.Configuration["LavaLink1:Password"]}");

        await discordShardedClient.StartAsync();

        var discClient1 = discordShardedClient.ShardClients[0];

        var lavaLinkClient1 = discClient1.UseLavalink();
        await lavaLinkClient1.ConnectAsync(lavaLinkServers[0]);

        DiscordClientEvents.discordClientsList.Add(discordShardedClient);


        await Task.Delay(-1);

        var host = builder.Build();

        await host.RunAsync();

    }

}

