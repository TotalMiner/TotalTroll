using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using TotalTroll.Modules;
using TotalTroll.Services;

namespace TotalTroll
{
    class Program
    {
        public static void Main(string[] args)
            => new Program().MainAsync().GetAwaiter().GetResult();

        public const int HourlyThankLimit = 5;
        public static HashSet<string> Thanks;
        public static Dictionary<ulong, Bucket> ThankLimit;
        private CommandService _commands;
        private ThankService _thanks;
        private DiscordSocketClient _client;
        private IServiceProvider _services;


        public async Task MainAsync()
        {
            Thanks = File.Exists("thanks.txt") ? new HashSet<string>(await File.ReadAllLinesAsync("thanks.txt")) : new HashSet<string>(new string[] { "thank", "thanks" });
            ThankLimit = new Dictionary<ulong, Bucket>();
            _client = new DiscordSocketClient();
            _commands = new CommandService();
            _thanks = new ThankService("thanks.txt");

            _services = new ServiceCollection()
                .AddSingleton(_client)
                .AddSingleton(_commands)
                .AddSingleton(_thanks)
                .BuildServiceProvider();

            await InstallCommandsAsync();

            _client.Log += Log;
            _client.MessageReceived += MessageReceived;

            string token = File.ReadAllText("SESSION");
            await _client.LoginAsync(TokenType.Bot, token);
            await _client.StartAsync();

            // Block this task until the program is closed.
            await Task.Delay(-1);
        }

        public async Task InstallCommandsAsync()
        {
            // Hook the MessageReceived Event into our Command Handler
            _client.MessageReceived += HandleCommandAsync;
            // Discover all of the commands in this assembly and load them.
            await _commands.AddModulesAsync(Assembly.GetEntryAssembly());
        }

        private async Task HandleCommandAsync(SocketMessage messageParam)
        {
            // Don't process the command if it was a System Message
            var message = messageParam as SocketUserMessage;
            if (message == null) return;
            // Create a number to track where the prefix ends and the command begins
            int argPos = 0;
            // Determine if the message is a command, based on if it starts with '!' or a mention prefix
            if (!(message.HasCharPrefix('!', ref argPos) || message.HasMentionPrefix(_client.CurrentUser, ref argPos))) return;
            // Create a Command Context
            var context = new SocketCommandContext(_client, message);
            // Execute the command. (result does not indicate a return value, 
            // rather an object stating if the command executed successfully)
            var result = await _commands.ExecuteAsync(context, argPos, _services);
            if (!result.IsSuccess)
                await context.Channel.SendMessageAsync(result.ErrorReason);
        }

        private async Task MessageReceived(SocketMessage message)
        {
            try
            {
                await Log(new LogMessage(LogSeverity.Info, message.Author.Username, message.Content));
                string[] args = message.Content.Split(' ');
                string command = args[0].ToLower();
                if (command == ":rates")
                {
                    ulong user = args.Length > 1  && MentionUtils.TryParseUser(args[1], out ulong userid) ? userid : message.Author.Id;
                    await message.Channel.SendMessageAsync($"{MentionUtils.MentionUser(user)} has {await Rates.Get(user)} rates.");
                } else if (command == ":nom")
                {
                    await Rates.Subtract(message.Author.Id, 1);
                    await message.Channel.SendMessageAsync($"{message.Author.Mention} consumed a rate!");
                }
                else if (Thanks.Contains(command) && args.Length > 1)
                {
                    List<ulong> users = new List<ulong>();
                    for(int i = 1; i < args.Length; i++)
                    {
                        if(MentionUtils.TryParseUser(args[i], out ulong userid))
                        {
                            if (userid == message.Author.Id)
                            {
                                await message.Channel.SendMessageAsync("You cannot thank yourself.");
                                return;
                            }
                            users.Add(userid);
                        }
                    }
                    if (users.Count > 0 && CanThank(message.Author.Id, users.Count))
                    {
                        StringBuilder str = new StringBuilder();
                        foreach(ulong userid in users)
                        {
                            await Rates.Add(userid, 1);
                            str.Append($" {MentionUtils.MentionUser(userid)}");
                        }
                        
                        await message.Channel.SendMessageAsync($"{message.Author.Mention} thanked{str}");
                    } else if (users.Count < 1) {
                        await message.Channel.SendMessageAsync("Could not find user.");
                    } else
                    {
                        await message.Channel.SendMessageAsync($"You cannot exceed the thank limit ({HourlyThankLimit} thank/hr). " + (users.Count > Allowance(message.Author.Id) ? string.Format("Your current allowance is {0}.", Allowance(message.Author.Id)) : string.Format("You can thank again in {0}", new TimeSpan(0, 0, (int)Math.Ceiling(ThankTime(message.Author.Id))).ToString(@"mm\:ss"))));
                    }
                }
                else if (command == ":give" && args.Length > 2)
                {
                    if (MentionUtils.TryParseUser(args[1], out ulong userid))
                    {
                        if(int.TryParse(args[2], out int amt))
                        {
                            await Rates.Subtract(message.Author.Id, amt);
                            await Rates.Add(userid, amt);
                            await message.Channel.SendMessageAsync($"{message.Author.Mention} gave {MentionUtils.MentionUser(userid)} {amt} rate{Plural(amt)}.");
                        } else
                        {
                            await message.Channel.SendMessageAsync("Invalid amount.");
                        }
                    } else
                    {
                        await message.Channel.SendMessageAsync("Could not find user.");
                    }
                }
            } catch (Exception e)
            {
                await Log(new LogMessage(LogSeverity.Error, "Error", e.ToString()));
            }
        }

        private bool CanThank(ulong user, int count)
        {
            if (ThankLimit.TryGetValue(user, out Bucket bucket))
            {
                return bucket.CanSpend(count);
            }
            else
            {
                Bucket newbucket = new Bucket(HourlyThankLimit, 3600);
                ThankLimit.Add(user, newbucket);
                return newbucket.CanSpend(count);
            }
        }

        private double ThankTime(ulong user)
        {
            if (ThankLimit.TryGetValue(user, out Bucket bucket))
            {
                return bucket.WaitTime();
            }
            else
            {
                return 0D;
            }
        }

        private string Plural(int amount)
        {
            return amount > 1 ? "s" : "";
        }

        private int Allowance(ulong user)
        {
            if (ThankLimit.TryGetValue(user, out Bucket bucket))
            {
                return (int)Math.Floor(bucket.Allowance());
            }
            else
            {
                Bucket newbucket = new Bucket(HourlyThankLimit, 3600);
                ThankLimit.Add(user, newbucket);
                return HourlyThankLimit;
            }
        }

        private Task Log(LogMessage msg)
        {
            Console.WriteLine(msg.ToString());
            return Task.CompletedTask;
        }
    }
}
