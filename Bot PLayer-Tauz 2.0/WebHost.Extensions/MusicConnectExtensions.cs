using DisCatSharp.Net;
using DisCatSharp.Lavalink;
using Microsoft.Extensions.DependencyInjection;



namespace WebHostExtensions
{
    public static class MusicConnectExtensions
    {

        public static LavalinkConfiguration GetLavaLinkConfiguration(this IServiceCollection services,string hostName, int port ,string password)
        {
            var endpoint = new ConnectionEndpoint()
            {
                Hostname = hostName,
                Port = port,
                Secured = true
                
            };

            var lavaLinkConfig = new LavalinkConfiguration()
            {
                Password = password,
                RestEndpoint = endpoint,
                SocketEndpoint = endpoint
            };

            return lavaLinkConfig;
        }

       
        

    }

}
