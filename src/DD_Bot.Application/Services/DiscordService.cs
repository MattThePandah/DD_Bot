﻿/* DD_Bot - A Discord Bot to control Docker containers*/

/*  Copyright (C) 2022 Maxim Kovac

    This program is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with this program.  If not, see <https://www.gnu.org/licenses/>.

*/

using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DD_Bot.Application.Commands;
using DD_Bot.Application.Interfaces;
using DD_Bot.Domain;
using Discord;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace DD_Bot.Application.Services
{
    public class DiscordService : IDiscordService
    {
        private readonly IConfigurationRoot _configuration;
        private readonly IServiceProvider _serviceProvider;
        private readonly DiscordSocketClient _discordClient;

        public DiscordService(IConfigurationRoot configuration, IServiceProvider serviceProvider)//Discord Initialising
        {
            var discordSocketConfig = new DiscordSocketConfig
            {
                GatewayIntents = GatewayIntents.Guilds | GatewayIntents.GuildMessages
            };

            _configuration = configuration;
            _serviceProvider = serviceProvider;
            _discordClient = new DiscordSocketClient(discordSocketConfig);
        }

        private Settings Setting => _configuration.Get<Settings>();

        private DockerService Docker => _serviceProvider.GetRequiredService<IDockerService>() as DockerService;
        private SettingsService SettingService => _serviceProvider.GetRequiredService<ISettingsService>() as SettingsService;
        
        public void Start() //Discord Start
        {
            
            _discordClient.Log += DiscordClient_Log;
            _discordClient.MessageReceived += DiscordClient_MessageReceived;
            _discordClient.GuildAvailable += DiscordClient_GuildAvailable;
            _discordClient.SlashCommandExecuted += DiscordClient_SlashCommandExecuted;
            _discordClient.ButtonExecuted += DiscordClient_ButtonExecuted;
            _discordClient.LoginAsync(TokenType.Bot, Setting.DiscordSettings.Token);
            _discordClient.StartAsync();
            while (true)
            {
                Thread.Sleep(1000);
            }
            // ReSharper disable once FunctionNeverReturns
        }

        private Task DiscordClient_SlashCommandExecuted(SocketSlashCommand arg)
        {
            switch (arg.CommandName)
            {
                case "minecraft":
                    TestCommand.Execute(arg, Docker, Setting.DiscordSettings);
                    return Task.CompletedTask;
                case "docker":
                    DockerCommand.Execute(arg, Docker, Setting.DiscordSettings);
                    return Task.CompletedTask;
                case "list":
                    ListCommand.Execute(arg, Docker, Setting.DiscordSettings);
                    return Task.CompletedTask;
                case "admin":
                    AdminCommand.Execute(arg, Setting, SettingService);
                    return Task.CompletedTask;
                case "user":
                    UserCommand.Execute(arg, Setting, SettingService);
                    return Task.CompletedTask;
                case "role":
                    RoleCommand.Execute(arg, Setting, SettingService);
                    return Task.CompletedTask;
                case "permission":
                    PermissionCommand.Execute(arg, Setting);
                    return Task.CompletedTask;
            }
            return Task.CompletedTask;
        }

        private Task DiscordClient_ButtonExecuted(SocketMessageComponent component)
        {
            switch (component.Data.CustomId)
            {
                case "mc-server-button-power":
                    component.UpdateAsync(woof => woof.Content = "boo");
                    //component.ModifyOriginalResponseAsync(woof => woof.Content = "boo.");
                    return Task.CompletedTask;
                    ////var menu = TestCommand.menuBuilder(true);
                    //component.ModifyOriginalResponseAsync(woof => woof.Content = "boo.");
                    ////component.ModifyOriginalResponseAsync(edit => edit.Components = menu.Build());
            }
            return Task.CompletedTask;
        }

        private async Task DiscordClient_GuildAvailable(SocketGuild guild)
        {
            await Task.Run(() =>
            {
                guild.CreateApplicationCommandAsync(DockerCommand.Create());
                guild.CreateApplicationCommandAsync(TestCommand.Create());
                guild.CreateApplicationCommandAsync(ListCommand.Create());
                guild.CreateApplicationCommandAsync(AdminCommand.Create());
                guild.CreateApplicationCommandAsync(UserCommand.Create());
                guild.CreateApplicationCommandAsync(RoleCommand.Create());
                guild.CreateApplicationCommandAsync(PermissionCommand.Create());
            });
            
        }

        private Task DiscordClient_MessageReceived(SocketMessage arg)
        {
            Console.WriteLine($"{arg.Author.Username}: {arg.Content}");
            return Task.CompletedTask;
        }

        private Task DiscordClient_Log(LogMessage arg)
        {
            Console.WriteLine($"{arg.Severity}:{arg.Message}");
            return Task.CompletedTask;
        }
    }
}
