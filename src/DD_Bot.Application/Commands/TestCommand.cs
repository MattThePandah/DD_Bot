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

using DD_Bot.Application.Services;
using DD_Bot.Domain;
using Discord;
using Discord.Commands;
using Discord.Interactions;
using Discord.WebSocket;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;

namespace DD_Bot.Application.Commands
{
    public class TestCommand
    {

        private DiscordSocketClient _discord;

        public TestCommand(DiscordSocketClient discord)
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
            return builder.Build();
        }

        //public static ApplicationCommandProperties Create()
        //{
        //    var builder = new SlashCommandBuilder()
        //    {
        //        Name = "ping",
        //        Description = "Ping"
        //    };
        //    return builder.Build();
        //}

        #endregion

        #region ExecuteCommand

        //[ComponentInteraction("button")]
        //public async Task HandleButtonInput()
        //{
        //    ulong id = 1204513253212823603;
        //    var channel = _discord.GetChannel(id) as IMessageChannel;
        //    await channel.SendMessageAsync("I pressed a button.");
        //}

        public static async void Execute(SocketSlashCommand arg, DockerService dockerService, DiscordSettings settings)
        {
            await arg.RespondAsync("Contacting Docker Service...");
            //await arg.RespondAsync("woof.");
            //await dockerService.DockerUpdate();

            bool serverStatus = true;

            EmbedBuilder embedBuilder = new EmbedBuilder
            {
                Title = "Minecraft Server - All The Mods 9",
                Description = "**Server status:** %value%"

            };
            embedBuilder.AddField("Online Players", "```ml\n%value%```", true)
            .WithColor(Color.Green);
            ComponentBuilder builder;

            if (serverStatus)
            {
                builder = menuBuilder(true);
            } 
            else
            {
                builder = menuBuilder(false);
            }

            await arg.ModifyOriginalResponseAsync(edit =>
            {
                edit.Content = "";
                edit.Embed = embedBuilder.Build();
                edit.Components = builder.Build();
            });
            //await arg.ModifyOriginalResponseAsync(edit => edit.Content = " ");
            //await arg.ModifyOriginalResponseAsync(edit =>
            //{
            //    edit.Embed = embedBuilder.Build();
            //    edit.Components = builder.Build();
            //});
            //await arg.ModifyOriginalResponseAsync(edit => edit.Content = "Test", Component: builder.Build())
            //await arg.RespondAsync("Minecraft server", components: builder.Build());

            //await arg.ModifyOriginalResponseAsync(edit => "test", components: builder.Build());
        }

        #endregion

        public static ComponentBuilder menuBuilder(bool test)
        {
            if (test)
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

        public static void ServerPower()
        {
            bool serverRunning = true;

            if (serverRunning)
            {
                
            }
        }

        //public static void GoFuckYourself()
        //{
        //    if (serverStatus)
        //    {
        //        test = "Stop Server";
        //        var builder = new ComponentBuilder().WithButton(test, "mc-server-button-power").WithButton("Restart", "mc-server-button-restart");
        //        await arg.ModifyOriginalResponseAsync(edit => edit.Content = "Minecraft server: %status%");
        //        await arg.ModifyOriginalResponseAsync(edit => edit.Components = builder.Build());
        //    }
        //    else
        //    {
        //        test = "Start Server";
        //        var builder = new ComponentBuilder().WithButton(test, "mc-server-button-power");
        //        await arg.ModifyOriginalResponseAsync(edit => edit.Content = "Minecraft server: %status%");
        //        await arg.ModifyOriginalResponseAsync(edit => edit.Components = builder.Build());
        //    }
        //}
    }

}
