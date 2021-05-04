using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Exceptions;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using Newtonsoft.Json;
using DSharpPlus.CommandsNext.Converters;
using DSharpPlus.Interactivity;

namespace Auvitau
{
    class Program
    {
        public static DiscordClient Client { get; set; }
        public static CommandsNextModule Commands { get; set; }
        public static InteractivityModule Interactivity { get; set; }

        public static GlobalStorage Storage { get; set; }

        public static ulong CreatorID { get { return 348696829648437249; } }

        static void Main(string[] args)
        {
            Start().GetAwaiter().GetResult();
        }

        public static async Task Start()
        {
            Storage = new GlobalStorage(System.IO.File.ReadAllText("./storage.json"));

            var cfg = new DiscordConfiguration
            {
                Token = "<REDACTED>",
                TokenType = TokenType.Bot,

                AutoReconnect = true,
                LogLevel = LogLevel.Debug,
                UseInternalLogHandler = true
            };

            Client = new DiscordClient(cfg);

            Client.Ready += async (e) =>
            {
                e.Client.DebugLogger.LogMessage(LogLevel.Info, "Gateway", "Ready Signal Received", DateTime.Now);
                await Task.Delay(0);
            };

            Client.GuildAvailable += async (e) =>
            {
                await Storage.Add(e.Guild);
            };

            var ccfg = new CommandsNextConfiguration
            {
                StringPrefix = "!",
                EnableDms = true,
                EnableMentionPrefix = true,
                EnableDefaultHelp = false
            };
            Commands = Client.UseCommandsNext(ccfg);

            Commands.CommandErrored += async (e) =>
            {
                await e.Context.Message.CreateReactionAsync(DiscordEmoji.FromName(e.Context.Client, ":no_entry:"));
                if (e.Exception.Message != "Specified command was not found.")
                {
                    await e.Context.Channel.SendMessageAsync(null, false, new DiscordEmbedBuilder()
                        .WithDescription($"**{e.Exception.Message}**")
                        .WithColor(new DiscordColor(0xFF0000))
                        .Build());
                    e.Context.Client.DebugLogger.LogMessage(LogLevel.Warning, "Commands", $"Error occured while running command {e.Command.QualifiedName} in guild {e.Context.Guild.Name} ({e.Context.Guild.Id.ToString()}) in channel #{e.Context.Channel.Name} with exception {e.Exception.Message} and stacktrace {e.Exception.StackTrace}", DateTime.Now);
                }
            };

            Commands.RegisterCommands<Commands>();

            //Commands.SetHelpFormatter<HelpFormatter>();

            var icfg = new InteractivityConfiguration();

            Interactivity = Client.UseInteractivity(icfg);

            await Client.ConnectAsync();

            await Task.Delay(-1);
        }
    }
}
