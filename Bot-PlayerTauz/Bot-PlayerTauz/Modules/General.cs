using Discord.Commands;

namespace Bot_PlayerTauz.Modules
{
    public class General : ModuleBase<SocketCommandContext>
    {
        [Command("play")]
        public async Task PlayMusicAsync()
        {
            await ReplyAsync("Music is playing");
        }
    }
}
