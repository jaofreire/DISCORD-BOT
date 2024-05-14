using Bot_PLayer_Tauz_2._0;
using Bot_PLayer_Tauz_2._0.Data;
using Bot_PLayer_Tauz_2._0.Modules;
using Bot_PLayer_Tauz_2._0.Wrappers;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.Interactivity.Enums;
using DSharpPlus.Interactivity;
using DSharpPlus.Lavalink;
using DSharpPlus.Lavalink.EventArgs;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using WebHostExtensions;
using DSharpPlus.Interactivity.Extensions;
using Bot_PLayer_Tauz_2._0.Wrappers.EventHandler;


class Program
{
    static async Task Main(string[] args)
    {
        var builder = Host.CreateApplicationBuilder();

        string state = builder.Configuration["ProjectStage"];

        var discordClient = new DiscordClient(new DiscordConfiguration()
        {
            Token = state == "production" ? builder.Configuration["DiscordEnv:Token"] : builder.Configuration["DiscordEnv:TokenTest"],
            TokenType = TokenType.Bot,
            Intents = DiscordIntents.All,
            MinimumLogLevel = LogLevel.Debug
        });



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
                options.Configuration = builder.Configuration["Redis:ConnectionStrings"];
            })
            .AddSingleton<DiscordClientEvents>()
            .BuildServiceProvider();


        discordClient.Ready += DiscordClientEvents.IsReady;

        var commands = discordClient.UseCommandsNext(new CommandsNextConfiguration()
        {
            StringPrefixes = [state == "production" ? builder.Configuration["DiscordEnv:Prefix"] : builder.Configuration["DiscordEnv:PrefixTest"]],
            Services = dependenciesServices
        });

        commands.RegisterCommands<CommandsModule>();

        discordClient.UseInteractivity(new InteractivityConfiguration()
        {
            PollBehaviour = PollBehaviour.KeepEmojis,
            Timeout = TimeSpan.FromMinutes(3)
        });


        var lavaLinkClient = discordClient.UseLavalink();

        var lavaLinkConfig = builder.Services.GetLavaLinkConfiguration(builder.Configuration["LavaLink1:Hostname"]
            , builder.Configuration.GetValue<int>("LavaLink1:Port")
            , builder.Configuration["LavaLink1:Password"]);



        builder.Services.UseDiscordBotMusicSDK(builder.Configuration, discordClient, lavaLinkClient, lavaLinkConfig);

       
        await Task.Delay(-1);

        var host = builder.Build();

        await host.RunAsync();

    }

}

