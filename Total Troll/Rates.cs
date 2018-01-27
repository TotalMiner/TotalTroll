using Discord;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Discord.WebSocket;

namespace TotalTroll
{
    class Rates
    {
        public static async Task<int> Get(ulong userid)
        {
            CheckDirectory();
            if(File.Exists(Path.Combine("database", userid.ToString())))
            {
                return int.Parse(await File.ReadAllTextAsync(Path.Combine("database", userid.ToString())));
            } else
            {
                await Set(userid, 0);
                return int.Parse(await File.ReadAllTextAsync(Path.Combine("database", userid.ToString())));
            }
        }

        public static async Task<int> Get(SocketUser user) => await Get(user.Id);

        public static async Task Set(ulong userid, int amount)
        {
            CheckDirectory();
            await File.WriteAllTextAsync(Path.Combine("database", userid.ToString()), amount.ToString());
        }

        public static async Task Set(SocketUser user, int amount) => await Set(user.Id, amount);

        public static async Task Add(ulong userid, int amount)
        {
            await Set(userid, await Get(userid) + amount);
        }

        public static async Task Add(SocketUser user, int amount) => await Add(user.Id, amount);

        public static async Task Subtract(ulong userid, int amount)
        {
            await Set(userid, await Get(userid) - amount);
        }

        public static async Task Subtract(SocketUser user, int amount) => await Subtract(user.Id, amount);

        public static async Task Give(ulong fromuserid, ulong touserid, int amount)
        {
            await Subtract(fromuserid, amount);
            await Add(touserid, amount);
        }

        public static async Task Give(SocketUser fromuser, SocketUser touser, int amount) => await Give(fromuser.Id, touser.Id, amount);

        private static void CheckDirectory()
        {
            if (!Directory.Exists("database"))
            {
                Directory.CreateDirectory("database");
            }
        }
    }
}
