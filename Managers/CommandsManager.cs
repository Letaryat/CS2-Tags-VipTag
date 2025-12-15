
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Admin;
using CounterStrikeSharp.API.Modules.Commands;
using CS2MenuManager.API.Menu;
using CS2Tags_VipTag.Models;
using Microsoft.Extensions.Logging;

namespace CS2Tags_VipTag
{
    public class CommandManager(CS2Tags_VipTag plugin)
    {
        private readonly CS2Tags_VipTag _plugin = plugin;

        public void InitializeCommands()
        {
            _plugin.AddCommand("css_settag", "Ability for VIP to change their Scoreboard and Chat tag", TagChange);
            _plugin.AddCommand("css_tagmenu", "Ability for VIP to change their Scoreboard and Chat tag", TagMenu);
        }

        [CommandHelper(minArgs: 1, usage: "TagName")]
        public void TagChange(CCSPlayerController? player, CommandInfo commandInfo)
        {
            if (player == null || player.IsBot || player.IsHLTV) return;
            if (!AdminManager.PlayerHasPermissions(player, _plugin.Config.VipFlag))
            {
                player!.PrintToChat($"{_plugin.Localizer["Prefix"]}{_plugin.Localizer["NoPermissions"]}");
                return;
            }

            //var arg = commandInfo.GetArg(1);

            string arg = "";

            for (int i = 1; i < commandInfo.ArgCount; i++)
            {
                arg += commandInfo.GetArg(i) + " ";
            }

            arg = arg.TrimEnd();

            if (arg.Length > 50)
            {
                player.PrintToChat($"{_plugin.Localizer["Prefix"]}{_plugin.Localizer["TooLong"]}");
                return;
            }

    

            var newtag = $"{arg} ";
            try
            {
                if (_plugin.Players.ContainsKey(player.AuthorizedSteamID!.SteamId64))
                {
                    _plugin.Players[player.AuthorizedSteamID.SteamId64]!.tag = arg;
                }
                else
                {
                    _plugin.Players[player.AuthorizedSteamID.SteamId64] = new PlayerModel
                    {
                        steamid = player.AuthorizedSteamID.SteamId64,
                        tag = arg,
                        tagcolor = null,
                        namecolor = null,
                        chatcolor = null,
                        visibility = true,
                        chatvisibility = true,
                        scorevisibility = true,
                    };
                }

                _plugin._tagApi?.SetAttribute(player, TagsApi.Tags.TagType.ScoreTag, newtag);
                if (_plugin.Players[player.AuthorizedSteamID.SteamId64]!.tagcolor == null)
                {
                    _plugin._tagApi?.SetAttribute(player!, TagsApi.Tags.TagType.ChatTag, $"{_plugin.Players[player.AuthorizedSteamID.SteamId64]!.tag} ");
                }
                else
                {
                    _plugin._tagApi?.SetAttribute(player, TagsApi.Tags.TagType.ChatTag, $"{{{_plugin.Players[player.AuthorizedSteamID!.SteamId64]!.tagcolor}}}{arg} ");
                }
                player.PrintToChat($"{_plugin.Localizer["Prefix"]}{_plugin.Localizer["TagSet", arg]}");
            }
            catch (Exception ex)
            {
                _plugin.Logger.LogInformation($"TagChange: {ex}");
            }

        }
        public void TagMenu(CCSPlayerController? player, CommandInfo commandInfo)
        {
            if (player == null || player.IsBot || player.IsHLTV) return;
            if (!AdminManager.PlayerHasPermissions(player, _plugin.Config.VipFlag))
            {
                player!.PrintToChat($"{_plugin.Localizer["Prefix"]}{_plugin.Localizer["NoPermissions"]}");
                return;
            }
            if (!_plugin.Players.ContainsKey(player.AuthorizedSteamID!.SteamId64))
            {
                player.PrintToChat($"{_plugin.Localizer["Prefix"]}{_plugin.Localizer["SetupTag"]}");
                return;
            }
            WasdMenu menu = new(_plugin.Localizer["VipMenu"], _plugin);

            menu?.AddItem($"{_plugin.Localizer["ToggleTagMenu"]}", (player, option) =>
            {
                _plugin.MenuManager!.CreateDisableMenu(player, menu);
            });

            menu?.AddItem(_plugin.Localizer["TagColorMenu"], (player, option) =>
            {
                _plugin.MenuManager!.CreateMenuWithColors(player, 1, menu);
            });
            menu?.AddItem(_plugin.Localizer["ChatColorMenu"], (player, option) =>
            {
                _plugin.MenuManager!.CreateMenuWithColors(player, 2, menu);
            });
            menu?.AddItem(_plugin.Localizer["NameColorMenu"], (player, option) =>
            {
                _plugin.MenuManager!.CreateMenuWithColors(player, 3, menu);
            });
            menu?.Display(player, 0);
        }

    }
}