using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

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

        public static async Task Set(ulong userid, int amount)
        {
            CheckDirectory();
            await File.WriteAllTextAsync(Path.Combine("database", userid.ToString()), amount.ToString());
        }

        public static async Task Add(ulong userid, int amount)
        {
            CheckDirectory();
            await Set(userid, await Get(userid) + amount);
        }

        public static async Task Subtract(ulong userid, int amount)
        {
            CheckDirectory();
            await Set(userid, await Get(userid) - amount);
        }

        private static void CheckDirectory()
        {
            if (!Directory.Exists("database"))
            {
                Directory.CreateDirectory("database");
            }
        }
    }
}
