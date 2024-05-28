using Google.Apis.Services;
using Google.Apis.YouTube.v3;
using Microsoft.Extensions.Configuration;

namespace Bot_PLayer_Tauz_2._0.Services
{
    public class YoutubeApiService
    {

        public async Task<string> SearchMusicUrlAsync(string url)
        {
            var youtubeService = new YouTubeService(new BaseClientService.Initializer()
            {
                ApiKey = Configurations.YoutubeApiKey,
                ApplicationName = "Bot-Music"
            });


            var searchListRequest = youtubeService.Search.List("snippet");
            searchListRequest.Q = url;
            searchListRequest.MaxResults = 1;

            var searchListResponse = await searchListRequest.ExecuteAsync();

            return searchListResponse.Items.First().Snippet.Title;
        }
    }
}
