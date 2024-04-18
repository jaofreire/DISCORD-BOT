using Bot_PLayer_Tauz_2._0.Data;
using Bot_PLayer_Tauz_2._0.Modules;
using DSharpPlus.CommandsNext;
using DSharpPlus.Interactivity;
using DSharpPlus.Interactivity.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using WebHostExtensions;


class Program
{
    static async Task Main(string[] args)
    {
        var builder = Host.CreateApplicationBuilder();

        var discordClient = builder.Services.AddDiscordClientServices(builder.Configuration, builder.Configuration["DiscordEnv:Token"]);

        var dependenciesServices = new ServiceCollection()
            .AddDbContext<MongoContext>(options =>
            {
                options.UseMongoDB(builder.Configuration["MongoDb:ConnectionStrings"], builder.Configuration["MongoDb:DataBase"]);
            })
            .BuildServiceProvider();

        builder.Services.AddClientCommandsNext<CommandsModule>(discordClient, builder.Configuration["DiscordEnv:Prefix"], dependenciesServices);

        var lavaLinkClient = builder.Services.AddLavaLinkServices(discordClient);

        var lavaLinkConfig = builder.Services.GetLavaLinkConfiguration(builder.Configuration["LavaLink:Hostname"]
            , builder.Configuration.GetValue<int>("LavaLink:Port")
            , builder.Configuration["LavaLink:Password"]);


        builder.Services.UseDiscordBotMusicSDK(builder.Configuration, discordClient, lavaLinkClient, lavaLinkConfig);

       
        await Task.Delay(-1);

        var host = builder.Build();

        await host.RunAsync();

    }

   
}

