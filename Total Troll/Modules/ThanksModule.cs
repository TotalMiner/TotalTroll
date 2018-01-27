using Discord.Commands;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using TotalTroll.Services;

namespace TotalTroll.Modules
{
    public class ThanksModule : ModuleBase<SocketCommandContext>
    {
        private readonly ThankService _thanks;

        public ThanksModule(ThankService thanks)
        {
            _thanks = thanks;
        }

        [Command("thank")]
        [Summary("Thanks a user.")]
        [Alias("rate", "thanks", "danke")]
        public async Task Thank(SocketUser user)
        {
            if(user == this.Context.User)
            {
                await this.ReplyAsync("You cannot thank yourself.");
            } else if (_thanks.CanThank(user))
            {
                await Rates.Add(user, 1);
                await this.ReplyAsync($"{this.Context.User.Mention} thanked {user.Mention}!");
            } else
            {
                await this.ReplyAsync($"You must wait {new TimeSpan(0, 0, (int)Math.Ceiling(_thanks.WaitTime(user))).ToString(@"mm\:ss")} to thank again.");
            }
        }
    }
}
