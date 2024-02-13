/* DD_Bot - A Discord Bot to control Docker containers*/

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

using DD_Bot.Application.Interfaces;
using DD_Bot.Application.Services;
using DD_Bot.Domain;
using Discord;
using Discord.Commands;
using Discord.Interactions;
using Discord.WebSocket;
using Docker.DotNet.Models;
using System;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;

namespace DD_Bot.Application.Commands
{
    public class MinecraftCommand
    {

        private DiscordSocketClient _discord;

        public MinecraftCommand(DiscordSocketClient discord)
        {
            _discord = discord;
        }

        #region Create Minecraft Command

        public static ApplicationCommandProperties Create()
        {
            var builder = new SlashCommandBuilder()
            {
                Name = "minecraft",
                Description = "Creates minecraft menu"
            };
            builder.AddOption(
                "container",
                ApplicationCommandOptionType.String,
                "Docker container used",
                true
            );
            return builder.Build();
        }

        #endregion

        #region ExecuteCommand

        public static async void Execute(SocketSlashCommand arg, DockerService dockerService, DiscordSettings settings)
        {
            await arg.RespondAsync("Contacting Docker Service...");
            await dockerService.DockerUpdate();

            var containerName = arg.Data.Options.FirstOrDefault(option => option.Name == "container")?.Value as string;

            if (string.IsNullOrEmpty(containerName))
            {
                await arg.ModifyOriginalResponseAsync(edit => edit.Content = "No name has been specified");
                return;
            }

            var docker = dockerService.DockerStatus.FirstOrDefault(docker => docker.Names[0] == containerName);

            if (containerName == null)
            {
                await arg.ModifyOriginalResponseAsync(edit => edit.Content = $"{containerName} doesn't exist.");
                return;
            }

            var dockerId = docker.ID;

            bool serverStatus = dockerService.RunningDockers.Contains(containerName);

            await arg.ModifyOriginalResponseAsync(edit =>
            {
                edit.Content = "";
                edit.Embed = embedBuilder(serverStatus, containerName).Build();
                edit.Components = menuBuilder(serverStatus).Build();
            });
        }

        #endregion

        public static ComponentBuilder menuBuilder(bool serverStatus)
        {
            if (serverStatus)
            {
                ComponentBuilder builder = new ComponentBuilder().WithButton("Stop Server", "mc-server-button-power", ButtonStyle.Danger)
                    .WithButton("Restart", "mc-server-button-restart");
                return builder;
            }
            else
            {
                ComponentBuilder builder = new ComponentBuilder().WithButton("Start Server", "mc-server-button-power", ButtonStyle.Success);
                return builder;
            }
        }

        public static EmbedBuilder embedBuilder(bool serverStatus, string containerName)
        {
            EmbedBuilder embedBuilder = new EmbedBuilder
            {
                Title = "Minecraft Server - All The Mods 9",
                Description = "**Server status:** Offine",

            };
            embedBuilder.WithFooter(footer => footer.Text = containerName);
            if (serverStatus)
            {
                embedBuilder.AddField("Online Players", "```ml\n%value%```", true).WithColor(Color.Green);
                embedBuilder.Description = "**Server status:** Online";

            }

            return embedBuilder;
        }

        /// <summary>
        /// This is the most jank piece of crap i have ever written.
        /// </summary>
        /// <param name="dockerService">Takes a Docker Server to find out what containers are running</param>
        /// <param name="component">Takes a SocketMessageComponent</param>
        /// <returns></returns>
        public static async Task ServerPower(DockerService dockerService, SocketMessageComponent component)
        {
            await dockerService.DockerUpdate();
            var containerName = component.Message.Embeds.FirstOrDefault()?.Footer?.Text;

            var docker = dockerService.DockerStatus.FirstOrDefault(d => d.Names[0] == containerName);
            var dockerId = docker?.ID;

            if (dockerId != null)
            {
                bool serverRunning = dockerService.RunningDockers.Contains(containerName);

                if (serverRunning)
                {
                    dockerService.DockerCommandStop(dockerId);
                    serverRunning = false;
                }
                else
                {
                    dockerService.DockerCommandStart(dockerId);
                    serverRunning = true;
                }

                await Task.Delay(30); // Use Task.Delay instead of Thread.Sleep
                await dockerService.DockerUpdate();

                var embed = embedBuilder(serverRunning, containerName).Build();
                var components = menuBuilder(serverRunning).Build();

                // UpdateAsync may not return a Task, so you can call it directly without await
                component.UpdateAsync(edit =>
                {
                    edit.Embed = embed;
                    edit.Components = components;
                });
            }
        }
    }
}
