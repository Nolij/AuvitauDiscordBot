using System;
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using System.Collections.Generic;
using NLua;
using DSharpPlus.Interactivity;

namespace Auvitau
{
    class Commands
    {
        [Command("ping"), Description("Pong!")]
        public async Task Ping(CommandContext Context)
        {
            await Context.TriggerTypingAsync();

            await Context.RespondAsync(null, false, new DiscordEmbedBuilder()
                .WithDescription($"{DiscordEmoji.FromName(Context.Client, ":ping_pong:")} Pong! **{Context.Client.Ping}ms**")
                .WithColor(Context.Guild.CurrentMember.Color)
                .Build());
        }

        [Command("logout"), Description("Logs out and closes the program."), Aliases("stop"), RequireOwner]
        public async Task Stop(CommandContext Context)
        {
            await Context.RespondAsync(null, false, new DiscordEmbedBuilder()
                .WithDescription("Logging out...")
                .WithColor(Context.Guild.CurrentMember.Color)
                .Build());
            await Context.Client.UpdateStatusAsync(null, UserStatus.Invisible);
            System.IO.File.WriteAllText("./storage.json", Program.Storage.Serialize());
            Environment.Exit(0);
        }

        [Command("help"), Description("Help command"), Aliases("h", "?")]
        public async Task Help(CommandContext Context, string Command = null)
        {
            if (Command == null)
            {

            }
        }

        [Command("lua"), Description("Runs Lua Code"), RequireOwner]
        public async Task Lua(CommandContext Context, [RemainingText] string code)
        {
            Lua State = new Lua();
            var Response = State.DoString(code);
            var Out = "";
            foreach (object x in Response)
            {
                Out = Out + x.ToString() + "\t";
            }
            await Context.RespondAsync(null, false, new DiscordEmbedBuilder()
                .WithDescription(Out)
                .WithColor(Context.Guild.CurrentMember.Color)
                .Build());
        }

        [Group("bank", CanInvokeWithoutSubcommand = false), Description("Allows you to manage your bank account")]
        public class Bank
        {
            [Command("balance"), Description("View your current bank account balance"), Aliases("bal")]
            public async Task Balance(CommandContext Context, DiscordUser User = null)
            {
                if (User != null && !Context.Member.PermissionsIn(Context.Channel).HasPermission(Permissions.Administrator))
                {
                    User = Context.User;
                }
                else if (User == null)
                {
                    User = Context.User;
                }
                await Context.RespondAsync(null, false, new DiscordEmbedBuilder()
                    .WithDescription($"{User.Mention}'s Balance: **{(await Program.Storage.Get(User.Id)).Bux.ToString()} Bux**")
                    .WithColor(Context.Guild.CurrentMember.Color)
                    .Build());
            }

            [Command("pay"), Description("Make a payment to another user"), Aliases("transfer", "send")]
            public async Task Pay(CommandContext Context, DiscordUser User, [RemainingText] string Amount)
            {
                var _Amount = new MoneyLib().ToNumber(Amount);
                if ((await Program.Storage.Get(Context.User.Id)).Transaction(TransactionType.Pay, _Amount, (await Program.Storage.Get(User.Id))))
                {
                    await Context.RespondAsync(null, false, new DiscordEmbedBuilder()
                        .WithDescription($"Successfully made payment of **{new MoneyLib().ToString(_Amount)} Bux** to {User.Mention}")
                        .WithColor(Context.Guild.CurrentMember.Color)
                        .Build());
                }
                else
                {
                    await Context.RespondAsync(null, false, new DiscordEmbedBuilder()
                        .WithDescription($"Payment failed. Please try again later or tell a developer if necessary.")
                        .WithColor(new DiscordColor(0xFF0000))
                        .Build());
                }
            }

            [Command("reset"), Description("Reset _all_ stats. (WARNING: this action is **permanent**)"), RequireOwner]
            public async Task Reset(CommandContext Context)
            {
                await Context.RespondAsync(null, false, new DiscordEmbedBuilder()
                    .WithDescription("**WARNING:** This action _cannot_ be undone. Enter \"confirm\" within 10 seconds to proceed.")
                    .WithColor(new DiscordColor(0xFF0000))
                    .Build());
                var Interactivity = Context.Client.GetInteractivityModule();
                var Msg = await Interactivity.WaitForMessageAsync(msg => msg.Author.Id == Context.User.Id && msg.Content == "confirm", TimeSpan.FromSeconds(10));
                if (Msg == null)
                {
                    await Context.RespondAsync(null, false, new DiscordEmbedBuilder()
                        .WithDescription("Action cancelled.")
                        .WithColor(new DiscordColor(0xFF0000))
                        .Build());
                    return;
                }
                foreach (User U in Program.Storage.UserStorage.Values)
                {
                    U.Bux = 50;
                }
                await Context.RespondAsync(null, false, new DiscordEmbedBuilder()
                    .WithDescription("Bank reset.")
                    .WithColor(new DiscordColor(0xFF0000))
                    .Build());
            }
        }

        [Group("admin", CanInvokeWithoutSubcommand = false), Description("Everything administration")]
        public class Admin
        {
            [Command("reset"), Description("Resets _all_ data."), RequireOwner]
            public async Task Reset(CommandContext Context)
            {
                await Context.RespondAsync(null, false, new DiscordEmbedBuilder()
                    .WithDescription("**WARNING:** This action _cannot_ be undone. Enter \"confirm\" within 10 seconds to proceed.")
                    .WithColor(new DiscordColor(0xFF0000))
                    .Build());
                var Interactivity = Context.Client.GetInteractivityModule();
                var Msg = await Interactivity.WaitForMessageAsync(msg => msg.Author.Id == Context.User.Id && msg.Content == "confirm", TimeSpan.FromSeconds(10));
                if (Msg == null)
                {
                    await Context.RespondAsync(null, false, new DiscordEmbedBuilder()
                        .WithDescription("Action cancelled.")
                        .WithColor(new DiscordColor(0xFF0000))
                        .Build());
                    return;
                }
                Program.Storage = new GlobalStorage();
                await Context.RespondAsync(null, false, new DiscordEmbedBuilder()
                    .WithDescription("Data reset.")
                    .WithColor(new DiscordColor(0xFF0000))
                    .Build());
                
            }
        }

        [Command("test"), Description("test123"), RequireOwner, Hidden]
        public async Task Test(CommandContext Context, double num)
        {
            await Context.RespondAsync((await Program.Storage.Get(Context.User.Id)).Bux.ToString());
            (await Program.Storage.Get(Context.User.Id)).Transaction(TransactionType.RawSet, num);
            await Context.RespondAsync((await Program.Storage.Get(Context.User.Id)).Bux.ToString());
        }


    }
}
