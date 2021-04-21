using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using System.Threading.Tasks;
using DSharpPlus.VoiceNext;
using System.Diagnostics;

namespace DiscordTTSBot
{
    class Commands : BaseCommandModule
    {
        [Command("tts")]
        public async Task TTS(CommandContext ctx, string message)
        {
            VoiceNextExtension voiceNext = ctx.Client.GetVoiceNext();

            VoiceNextConnection voiceNextConnection = voiceNext.GetConnection(ctx.Guild);
            if (voiceNextConnection != null)
                throw new InvalidOperationException("Already connected in this guild.");

            DiscordChannel channel = ctx.Member.VoiceState.Channel;
            if (channel == null)
                throw new InvalidOperationException("You need to be in a voice channel.");

            voiceNextConnection = await voiceNext.ConnectAsync(channel);
            //await ctx.Message.CreateReactionAsync(DiscordEmoji.FromName(ctx.Client, ":white_check_mark: "));

            string filePath = File.ReadAllText("path.txt");//@"C:\Users\home\source\repos\DiscordTTSBot\DiscordTTSBot\TTSTest\cock.wav";

            Console.WriteLine($"Received message: {ctx.RawArgumentString}");
            OutputTTS(ctx.RawArgumentString, filePath, true);

            await voiceNextConnection.SendSpeakingAsync(true);

            ProcessStartInfo processStartInfo = new ProcessStartInfo
            {
                FileName = "ffmpeg",
                Arguments = $@"-i ""{filePath}"" -ac 2 -f s16le -ar 48000 pipe:1",
                RedirectStandardOutput = true,
                UseShellExecute = false
            };

            Process ffmpeg = Process.Start(processStartInfo);
            Stream ffout = ffmpeg.StandardOutput.BaseStream;

            VoiceTransmitSink voiceTransmitSink = voiceNextConnection.GetTransmitSink();

            await ffout.CopyToAsync(voiceTransmitSink);

            voiceNextConnection.Disconnect();

            //voiceNextConnection = voiceNext.GetConnection(ctx.Guild);

            //byte[] buffer = new byte[3840];
            //var br = 0;
            //while ((br = ffout.Read(buffer, 0, buffer.Length)) > 0)
            //{
            //    if (br < buffer.Length)
            //        for (int i = br; i < buffer.Length; i++)
            //            buffer[i] = 0;

                
            //}
        }
        private void OutputTTS(string textToSpeech, string path, bool wait = false)
        {
            if (File.Exists(path))
                File.Delete(path);

            string command = $@"$filePath=""{path}"";$text=""{textToSpeech}"";Add-Type -AssemblyName System.Speech;$s=New-Object System.Speech.Synthesis.SpeechSynthesizer;$s.SetOutputToWaveFile($filePath);$s.Speak($text)";

            Execute(command); // Embedd text  

            void Execute(string command)
            {
                //var cFile = System.IO.Path.GetTempPath() + Guid.NewGuid() + ".ps1";


                //File.WriteAllText(cFile, command);
                var start =
                    new System.Diagnostics.ProcessStartInfo()
                    {
                        FileName = "C:\\windows\\system32\\windowspowershell\\v1.0\\powershell.exe",
                        LoadUserProfile = false,
                        UseShellExecute = false,
                        CreateNoWindow = false,
                        Arguments = $"-c {command}",
                        //Arguments = $"-executionpolicy bypass -File {cFile}",
                        WindowStyle = System.Diagnostics.ProcessWindowStyle.Maximized
                    };

                var p = System.Diagnostics.Process.Start(start);
                if (wait) p.WaitForExit();
            }
        }
    }
}
