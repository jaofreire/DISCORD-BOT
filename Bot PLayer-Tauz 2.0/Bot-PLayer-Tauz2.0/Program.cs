using Bot_PLayer_Tauz_2._0.Data;
using Bot_PLayer_Tauz_2._0.Modules;
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

        string state = Environment.GetEnvironmentVariable("State");

        var discordClient = builder.Services.AddDiscordClientServices(builder.Configuration
            , state == "production" ? builder.Configuration["DiscordEnv:Token"] : builder.Configuration["DiscordEnv:TokenTest"]);

        var dependenciesServices = new ServiceCollection()
            .AddDbContext<MongoContext>(options =>
            {

                if(state == "production")
                {
                    options.UseMongoDB(Environment.GetEnvironmentVariable("MongoConnectionString"), builder.Configuration["MongoDb:DataBase"]);
                    Console.WriteLine($"Connection String: {Environment.GetEnvironmentVariable("MongoConnectionString")}, DataBase: {builder.Configuration["MongoDb:DataBase"]}");
                }

                options.UseMongoDB(builder.Configuration["MongoDb:ConnectionStrings"], builder.Configuration["MongoDb:DataBase"]);
                
            })
            .BuildServiceProvider();

        builder.Services.AddClientCommandsNext<CommandsModule>(discordClient
            , state == "production"?builder.Configuration["DiscordEnv:Prefix"] : builder.Configuration["DiscordEnv:PrefixTest"]
            , dependenciesServices);

        var lavaLinkClient = builder.Services.AddLavaLinkServices(discordClient);

        var lavaLinkConfig = builder.Services.GetLavaLinkConfiguration(builder.Configuration["LavaLink2:Hostname"]
            , builder.Configuration.GetValue<int>("LavaLink2:Port")
            , builder.Configuration["LavaLink2:Password"]);


        builder.Services.UseDiscordBotMusicSDK(builder.Configuration, discordClient, lavaLinkClient, lavaLinkConfig);

       
        await Task.Delay(-1);

        var host = builder.Build();

        await host.RunAsync();

    }

   
}

