using Discord;
using Discord.Audio;
using Discord.WebSocket;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;

namespace CommunityBot.Features.Audio
{
    public class AudioService
    {
        private readonly ConcurrentDictionary<ulong, IAudioClient> ConnectedChannels = new ConcurrentDictionary<ulong, IAudioClient>();
        private Dictionary<ulong, AudioInStream> listenings = new Dictionary<ulong, AudioInStream>();


        public async Task JoinAudio(IGuild guild, IVoiceChannel target)
        {
            IAudioClient client;
            if (ConnectedChannels.TryGetValue(guild.Id, out client))
            {
                return;
            }
            if (target.Guild.Id != guild.Id)
            {
                return;
            }

            var audioClient = await target.ConnectAsync();

            if (ConnectedChannels.TryAdd(guild.Id, audioClient))
            {
                // If you add a method to log happenings from this service,
                // you can uncomment these commented lines to make use of that.
                //await Log(LogSeverity.Info, $"Connected to voice on {guild.Name}.");
            }
        }

        public async Task LeaveAudio(IGuild guild)
        {
            IAudioClient client;
            if (ConnectedChannels.TryRemove(guild.Id, out client))
            {
                await client.StopAsync();
                //await Log(LogSeverity.Info, $"Disconnected from voice on {guild.Name}.");
            }
        }

        public async Task SendAudioAsync(IGuild guild, IMessageChannel channel, string path)
        {
            // Your task: Get a full path to the file if the value of 'path' is only a filename.
            //if (!File.Exists(path))
            //{
            //    await channel.SendMessageAsync("File does not exist.");
            //    return;
            //}
            IAudioClient client;
            if (ConnectedChannels.TryGetValue(guild.Id, out client))
            {
                client.StreamCreated += MirrorAudio;
                client.StreamDestroyed += async (uID) => listenings.Remove(uID);
                //await Log(LogSeverity.Debug, $"Starting playback of {path} in {guild.Name}");

                var ffmpeg = CreateStream(path);
                var output = ffmpeg.StandardOutput.BaseStream;
                var stream = client.CreatePCMStream(AudioApplication.Voice);               

                await output.CopyToAsync(stream);
                await stream.FlushAsync();
            }
        }

        async public Task MirrorAudio(ulong uID, AudioInStream listeningStream)
        {
            listenings.Add(uID, listeningStream);
        }

        private Process CreateStream(string url)
        {
            var ffmpeg = new ProcessStartInfo
            {
                FileName = "youtube-dl.exe",
                Arguments = $"-o - {url} | ffmpeg.exe -i -ac 2 -f s16le -ar 48000 pipe:1",
                UseShellExecute = true,
                RedirectStandardOutput = true,
            };
            return Process.Start(ffmpeg);
        }
    }
}
