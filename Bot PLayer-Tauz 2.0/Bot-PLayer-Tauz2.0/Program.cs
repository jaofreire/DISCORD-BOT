using Bot_PLayer_Tauz_2._0;
using Bot_PLayer_Tauz_2._0.Data;
using Bot_PLayer_Tauz_2._0.Modules;
using Bot_PLayer_Tauz_2._0.Wrappers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
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

        string state = builder.Configuration["ProjectStage"];

        //var discordClient = new DiscordClient(new DiscordConfiguration()
        //{
        //    Token = state == "production" ? builder.Configuration["DiscordEnv:Token"] : builder.Configuration["DiscordEnv:TokenTest"],
        //    TokenType = TokenType.Bot,
        //    Intents = DiscordIntents.All,
        //    MinimumLogLevel = LogLevel.Debug
        //});


        var discordShardedClient = new DiscordShardedClient(new DiscordConfiguration()
        {
            Token = state == "production" ? builder.Configuration["DiscordEnv:Token"] : builder.Configuration["DiscordEnv:TokenTest"],
            TokenType = TokenType.Bot,
            Intents = DiscordIntents.All,
            MinimumLogLevel = LogLevel.Debug,
            AutoReconnect = true,
            UseRelativeRatelimit = true
        });

        //var discordShardedClient2 = new DiscordShardedClient(new DiscordConfiguration()
        //{
        //    Token = state == "production" ? builder.Configuration["DiscordEnv:Token"] : builder.Configuration["DiscordEnv:TokenTest"],
        //    TokenType = TokenType.Bot,
        //    Intents = DiscordIntents.All,
        //    MinimumLogLevel = LogLevel.Debug,
        //    AutoReconnect = true,
        //    UseRelativeRatelimit = true
        //});

        List<DiscordShardedClient> shardedClientsList = [discordShardedClient];


        var dependenciesServices = new ServiceCollection()
            .AddDbContext<MongoContext>(options =>
            {

                if (state == "production")
                {
                    options.UseMongoDB(builder.Configuration["MongoDbProd:ConnectionStrings"], builder.Configuration["MongoDb:DataBase"]);
                }
                else if (state == "development")
                {
                    options.UseMongoDB(builder.Configuration["MongoDb:ConnectionStrings"], builder.Configuration["MongoDb:DataBase"]);
                }

            })
            .AddStackExchangeRedisCache(options =>
            {
                if (state == "production")
                {
                    options.Configuration = Environment.GetEnvironmentVariable("REDIS_CONNECTION_STRINGS");
                }
                else if (state == "development")
                {
                    options.Configuration = builder.Configuration["Redis:ConnectionStrings"];
                }

            })
            .AddSingleton<DiscordClientEvents>()
            .BuildServiceProvider();

        foreach (var shardsClient in shardedClientsList)
        {
            shardsClient.Ready += DiscordClientEvents.IsReady;
            shardsClient.GuildAvailable += DiscordClientEvents.IsGuildAvaliable;

            var commands = await shardsClient.UseCommandsNextAsync(new CommandsNextConfiguration()
            {
                StringPrefixes = [state == "production" ? builder.Configuration["DiscordEnv:Prefix"] : builder.Configuration["DiscordEnv:PrefixTest"]],
                ServiceProvider = dependenciesServices
            });

            commands.RegisterCommands<CommandsModule>();

            await shardsClient.UseInteractivityAsync(new InteractivityConfiguration()
            {
                Timeout = TimeSpan.FromMinutes(3)
            });
        }
        //discordShardedClient.Ready += DiscordClientEvents.IsReady;
        //discordShardedClient.GuildAvailable += DiscordClientEvents.IsGuildAvaliable;


        //var commands = await discordShardedClient.UseCommandsNextAsync(new CommandsNextConfiguration()
        //{
        //    StringPrefixes = [state == "production" ? builder.Configuration["DiscordEnv:Prefix"] : builder.Configuration["DiscordEnv:PrefixTest"]],
        //    Services = dependenciesServices
        //});

        //commands.RegisterCommands<CommandsModule>();

        //await discordShardedClient.UseInteractivityAsync(new InteractivityConfiguration()
        //{
        //    PollBehaviour = PollBehaviour.KeepEmojis,
        //    Timeout = TimeSpan.FromMinutes(3)
        //});


        var lavaLinkConfig1 = builder.Services.GetLavaLinkConfiguration(state == "production" ? Environment.GetEnvironmentVariable("LAVA_LINK_HOST_NAME") : builder.Configuration["LavaLink1:Hostname"]
            , state == "production" ? int.Parse(Environment.GetEnvironmentVariable("LAVA_LINK_PORT")) : builder.Configuration.GetValue<int>("LavaLink1:Port")
            , state == "production" ? Environment.GetEnvironmentVariable("LAVA_LINK_PASSWORD") : builder.Configuration["LavaLink1:Password"]);

        var lavaLinkConfig2 = builder.Services.GetLavaLinkConfiguration(state == "production" ? Environment.GetEnvironmentVariable("LAVA_LINK_HOST_NAME2") : builder.Configuration["LavaLink2:Hostname"]
            , state == "production" ? int.Parse(Environment.GetEnvironmentVariable("LAVA_LINK_PORT2")) : builder.Configuration.GetValue<int>("LavaLink2:Port")
            , state == "production" ? Environment.GetEnvironmentVariable("LAVA_LINK_PASSWORD2") : builder.Configuration["LavaLink2:Password"]);


        List<LavalinkConfiguration> lavaLinkServers = [lavaLinkConfig1, lavaLinkConfig2];

        await discordShardedClient.StartAsync();
        //await discordShardedClient2.StartAsync();


        var discClient1 = discordShardedClient.ShardClients[0];
        //var discClient2 = discordShardedClient2.ShardClients[0];

        var lavaLinkClient1 = discClient1.UseLavalink();
        //var lavaLinkClient2 = discClient2.UseLavalink();

        //await discordShardedClient.UseLavalinkAsync();


        await lavaLinkClient1.ConnectAsync(lavaLinkServers[0]);
        //await lavaLinkClient2.ConnectAsync(lavaLinkServers[1]);

        DiscordClientEvents.discordClientsList.Add(discordShardedClient);
        //DiscordClientEvents.discordClientsList.Add(discordShardedClient2);



        await Task.Delay(-1);

        var host = builder.Build();

        await host.RunAsync();

    }

}

