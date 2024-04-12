using Discord;
using Discord.Audio;
using Discord.Commands;
using Discord.WebSocket;


namespace Bot_PlayerTauz.Services
{
    public class AudioService
    {
        public async Task<IAudioClient> ConnectVoiceChannel(SocketCommandContext context)
        {
            SocketGuildUser user = context.User as SocketGuildUser;

            IVoiceChannel voiceChannel = user.VoiceChannel;

            if (voiceChannel == null)
            {
               await context.Channel.SendMessageAsync("É necessário estar em um canal de voz");
                return null;
            }

            return await voiceChannel.ConnectAsync();

        }

        public async Task DisconnectVoiceChannel(SocketCommandContext context)
        {
            SocketGuildUser user = context.User as SocketGuildUser;

            IVoiceChannel voiceChannel = user.VoiceChannel;

            if (voiceChannel == null)
            {
                await context.Channel.SendMessageAsync("É necessário estar em um canal de voz");
                return;
            }

             await voiceChannel.DisconnectAsync();
        }
    }
}
