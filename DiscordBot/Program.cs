using Discord;
using Discord.Commands.Builders;
using Discord.Net;
using Discord.WebSocket;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
internal class Program
{
    class MainClass
    {

        //A lot of boiler plate stuff and some global variables
        public static void Main(string[] args)
        => new MainClass().MainAsync().GetAwaiter().GetResult();

        private DiscordSocketClient? _client;
        private SocketGuild? _guild;

        int totalMembers, totalVoiceChannels;

        Random random = new Random();

        List<SocketVoiceChannel> guildVoiceChannels = new List<SocketVoiceChannel> { };
        List<SocketGuildUser> guildUsers = new List<SocketGuildUser> { };

        public async Task MainAsync()
        {
            //Configuring Gateway intents for bot permissions
            var config = new DiscordSocketConfig
            {
                AlwaysDownloadUsers = true,
                GatewayIntents = GatewayIntents.AllUnprivileged | GatewayIntents.MessageContent | GatewayIntents.GuildMembers | GatewayIntents.DirectMessages | GatewayIntents.Guilds
            };

            //Adding discord events to functions
            _client = new DiscordSocketClient(config);
            _client.MessageReceived += MessageHandler;
            _client.UserVoiceStateUpdated += VoiceHandler;
            _client.SlashCommandExecuted += SlashCommandHandler;
            _client.Ready += OnReady;
            _client.Log += Log;

            var token = "OTU4NTUyNTg4MjQzMDU0NjI0.YkO_qg.38C1mS8iOdTj0vSYBMLmkOaMAeM";

            await _client.LoginAsync(TokenType.Bot, token);
            await _client.StartAsync();

            await Task.Delay(-1);
        }

        private async Task OnReady()
        {
            //Gets the guild id from the client and assign it to a guild variable for later use
            _guild = _client.GetGuild(_client.Guilds.ElementAt<SocketGuild>(0).Id);

            if(_guild != null)
            {
                Console.WriteLine(_guild.Id);
                await _guild.DownloadUsersAsync();

                GetChannels();
                GetUsers();

                SlashCommandBuilder guildCommand = new SlashCommandBuilder();


                guildCommand.WithName("remove");
                guildCommand.WithDescription("Command to Remove Users from Voice Channels");

                guildCommand.WithName("move");
                guildCommand.WithDescription("Command to Move Users to different Voice Channels");

                try
                {
                    await _guild.CreateApplicationCommandAsync(guildCommand.Build());
                }
                catch (ApplicationCommandException exception)
                {
                    var json = JsonConvert.SerializeObject(exception.Errors, Formatting.Indented);
                    Console.WriteLine(json);
                }
            }
        }

        private Task Log(LogMessage msg)
        {
            Console.WriteLine(msg.ToString());

            return Task.CompletedTask;
        }

        private Task SlashCommandHandler(SocketSlashCommand command)
        {
            return Task.CompletedTask;
        }

        private Task MessageHandler(SocketMessage message)
        {

            if(!message.Author.IsBot)
            {
                Console.WriteLine("User " + message.Author.Username + " Sent Message: " + message.Content.ToString());
            }

            return Task.CompletedTask;
        }

        private Task VoiceHandler(SocketUser user, SocketVoiceState oldState, SocketVoiceState newState)
        {
            //Getting a random channel from the list of voice channels
            int randomChannel = random.Next(guildVoiceChannels.Count());

            //Returning if a user leaves or joins a valid voice channel
            if(guildVoiceChannels.Contains(oldState.VoiceChannel))
            {
                return Task.CompletedTask;
            }
            if(guildVoiceChannels.Contains(newState.VoiceChannel))
            {
                return Task.CompletedTask;
            }

            return Task.CompletedTask;
        }

        //Function to move users between voice channels
        private async void Move(SocketGuildUser user)
        {
            int randomChannel = random.Next(guildVoiceChannels.Count());
            await user.ModifyAsync(x => { x.ChannelId = guildVoiceChannels[randomChannel].Id; });
        }


        //Function to remove users from voice channels
        private async void Remove(SocketGuildUser user)
        {
            await user.ModifyAsync(x => { x.ChannelId = null; });
        }

        public void GetChannels()
        {
            for (int i = 0; i < _guild.VoiceChannels.Count(); i++)
            {
                guildVoiceChannels.Add(_guild.VoiceChannels.ElementAt<SocketVoiceChannel>(i));
            }
            Console.WriteLine("Got Channels...");
        }
        public void GetUsers()
        {
            for (int i = 0; i < _guild.Users.Count(); i++)
            {
                guildUsers.Add(_guild.Users.ElementAt<SocketGuildUser>(i));
            }
            Console.WriteLine("Got Users...");
        }

    }
}