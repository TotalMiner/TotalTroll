using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace TotalTroll.Modules
{
    [Group("rates")]
    public class RatesModule : ModuleBase<SocketCommandContext>
    {
        [Command]
        [Summary("Gets users total rates.")]
        public async Task Default() => await User(this.Context.User);

        [Command("user")]
        [Summary("Gets total rates of a specified user.")]
        public async Task User(SocketUser user)
        {
            await this.ReplyAsync("", embed: new EmbedBuilder()
                .WithAuthor(new EmbedAuthorBuilder()
                    .WithIconUrl(user.GetAvatarUrl())
                    .WithName(user.Username))
                .WithColor(Color.Red)
                .AddField(new EmbedFieldBuilder()
                    .WithName("Rates")
                    .WithValue($"{await Rates.Get(user)}"))
                .Build());
        }

        [Command("give")]
        [Summary("Gives rates to a specified user.")]
        public async Task Give(SocketUser user, int amount)
        {
            if (amount <= await Rates.Get(this.Context.User))
            {
                await Rates.Give(this.Context.User, user, amount);
                await this.ReplyAsync($"{this.Context.User.Mention} gave {user.Mention} {amount} rates.");
            } else
            {
                await this.ReplyAsync("Insufficent rates.");
            }
        }

        [Command("nom")]
        [Summary("Noms rates.")]
        public async Task Nom(int amount)
        {
            if (amount <= await Rates.Get(this.Context.User))
            {
                await Rates.Subtract(this.Context.User, amount);
                await this.ReplyAsync(amount > 1 ?
                    $"{this.Context.User.Mention} nommed {amount} rates!" :
                    $"{this.Context.User.Mention} nommed a rate!");
            } else
            {
                await this.ReplyAsync("Insufficent rates.");
            }
        }

        [Command("nom")]
        [Summary("Noms a rate.")]
        public async Task Nom() => await Nom(1);

        /*
         * TODO: Rewrite rates w/ DB support so we can get top rates.
        [Command("Leaderboard")]
        [Summary("Shows the rates leaderboard.")]
        public async Task Leaderboard()
        {
            var emb = new EmbedBuilder()
                .WithTitle("Rates Leaderboard")
                .WithColor(Color.Red);
            var top = 
            await this.ReplyAsync("", embed: emb.Build());
        }
        */

        [RequireOwner]
        [Command("spawn")]
        [Summary("Spawns in rates from thin air.")]
        public async Task Spawn(int amount)
        {
            await Rates.Add(this.Context.User, amount);
            await this.ReplyAsync($"Spawning in {amount} rates from thin air...");
        }
    }
}
