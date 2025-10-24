using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Translations;
using CounterStrikeSharp.API.Modules.Utils;
using CS2MenuManager.API.Menu;
using static TagsApi.Tags;

namespace CS2Tags_VipTag
{
    public class MenuManager(CS2Tags_VipTag plugin)
    {
        private readonly CS2Tags_VipTag _plugin = plugin;

        public void CreateDisableMenu(CCSPlayerController player)
        {
            if (player == null) return;
            WasdMenu menu = new("Disable menu", _plugin);
            menu.AddItem($"Toggle Everything  - [{_plugin.Players[player.AuthorizedSteamID!.SteamId64]!.visibility}]", (p, o) =>
            {
                bool currentVisibility = _plugin.Players[player.AuthorizedSteamID!.SteamId64]!.visibility ?? false;

                bool newVisibility = !currentVisibility;

                _plugin.Players[player.AuthorizedSteamID!.SteamId64]!.visibility = newVisibility;

                _plugin._tagApi?.SetPlayerVisibility(player, newVisibility);

                if (newVisibility)
                {
                    /*
                    _plugin._tagApi?.SetAttribute(player, TagsApi.Tags.TagType.ScoreTag, _plugin.Players[player.AuthorizedSteamID.SteamId64]!.tag);
                    _plugin._tagApi?.SetAttribute(player, TagsApi.Tags.TagType.ChatColor, $"{{{_plugin.Players[player.AuthorizedSteamID.SteamId64]!.chatcolor!}}}");
                    _plugin._tagApi?.SetAttribute(player, TagsApi.Tags.TagType.NameColor, $"{{{_plugin.Players[player.AuthorizedSteamID.SteamId64]!.namecolor!}}}");
                    _plugin._tagApi?.SetAttribute(player, TagsApi.Tags.TagType.ChatTag, $"{{{_plugin.Players[player.AuthorizedSteamID.SteamId64]!.tagcolor}}}{_plugin.Players[player.AuthorizedSteamID!.SteamId64]!.tag} ");
                    */

                    _plugin.TagsManager!.SetEverythingTagRelated(player);
                    player.PrintToChat($"{_plugin.Localizer["Prefix"]}{_plugin.Localizer["Toggled"]}");
                }
                else
                {
                    player.PrintToChat($"{_plugin.Localizer["Prefix"]}{_plugin.Localizer["UnToggled"]}");
                }
            });
            menu.AddItem($"Toggle ScoreTag  - [{_plugin.Players[player.AuthorizedSteamID!.SteamId64]!.scorevisibility}]", (p, o) =>
            {
                bool currentVisibility = _plugin.Players[player.AuthorizedSteamID!.SteamId64]!.scorevisibility ?? false;

                bool newVisibility = !currentVisibility;

                _plugin.Players[player.AuthorizedSteamID!.SteamId64]!.scorevisibility = newVisibility;

                if (newVisibility)
                {
                    _plugin._tagApi.SetAttribute(player, TagType.ScoreTag, _plugin.Players[player.AuthorizedSteamID!.SteamId64]!.tag);
                    Server.PrintToChatAll($"Test 1 {newVisibility}");
                }
                else
                {
                    _plugin._tagApi.ResetAttribute(player, TagType.ScoreTag);
                    Server.PrintToChatAll($"Test 2 {newVisibility}");
                }

            });
            menu.AddItem($"Toggle ChatTag  - [{_plugin.Players[player.AuthorizedSteamID!.SteamId64]!.chatvisibility}]", (p, o) =>
            {
                bool currentVisibility = _plugin.Players[player.AuthorizedSteamID!.SteamId64]!.chatvisibility ?? false;

                bool newVisibility = !currentVisibility;

                _plugin.Players[player.AuthorizedSteamID!.SteamId64]!.chatvisibility = newVisibility;

                if (newVisibility)
                {
                    //_plugin._tagApi?.SetAttribute(player!, TagType.ChatTag, $"{{{_plugin.Players[player.AuthorizedSteamID.SteamId64]!.tagcolor}}}{_plugin.Players[player.AuthorizedSteamID.SteamId64]!.tag} ");
                    _plugin.TagsManager!.SetChatTag(player);
                    Server.PrintToChatAll($"Test 1 {newVisibility}");
                }
                else
                {
                    _plugin._tagApi.ResetAttribute(player, TagType.ChatTag);
                    Server.PrintToChatAll($"Test 2 {newVisibility}");
                }
            });
            
            menu.Display(player, 0);
        }
        public void CreateMenuWithColors(CCSPlayerController? player, int type)
        {
            if (player == null) { return; }
            WasdMenu menu = new("Tags menu", _plugin);
            switch (type)
            {
                case 1:
                    menu = new(_plugin.Localizer["TagColorMenu"], _plugin);
                    break;
                case 2:
                    menu = new(_plugin.Localizer["ChatColorMenu"], _plugin);
                    break;
                case 3:
                    menu = new(_plugin.Localizer["NameColorMenu"], _plugin);
                    break;
            }

            foreach (var chatcolors in _plugin.Colors)
            {
                string hex = PluginUtilities.FromNameToHex(chatcolors)!;
                string? menuOption;
                if(chatcolors == "TeamColor")
                {
                    if (player.Team == CsTeam.CounterTerrorist)
                    {
                        hex = PluginUtilities.FromNameToHex("CTBlue")!;
                    }
                    else if (player.Team == CsTeam.Terrorist)
                    {
                        hex = PluginUtilities.FromNameToHex("Orange")!;
                    }
                    else
                    {
                        hex = "#FFFFFF";
                    }
                }
                menuOption = $"<font color='{hex}'><b>{chatcolors}</b></font>";

                menu?.AddItem(menuOption, (player, option) =>
                {
                    switch (type)
                    {
                        case 1:
                            string? playertag = _plugin._tagApi?.GetAttribute(player, TagType.ScoreTag);
                            _plugin._tagApi?.SetAttribute(player, TagType.ChatTag, $"{{{chatcolors}}}{_plugin.Players[player.AuthorizedSteamID!.SteamId64]!.tag} ");

                            player.PrintToChat($"{_plugin.Localizer["Prefix"]}{{{chatcolors}}}{_plugin.Localizer["NewTagColor", chatcolors]}".ReplaceColorTags().Replace("{TeamColor}", ChatColors.ForTeam(player.Team).ToString()));
                            _plugin.Players[player.AuthorizedSteamID!.SteamId64]!.tagcolor = chatcolors;
                            break;
                        case 2:
                            _plugin._tagApi?.SetAttribute(player, TagType.ChatColor, $"{{{chatcolors}}}");
                            player.PrintToChat($"{_plugin.Localizer["Prefix"]}{{{chatcolors}}}{_plugin.Localizer["NewChatColor", chatcolors]}".ReplaceColorTags().Replace("{TeamColor}", ChatColors.ForTeam(player.Team).ToString()));
                            _plugin.Players[player.AuthorizedSteamID!.SteamId64]!.chatcolor = chatcolors;
                            break;
                        case 3:
                            _plugin._tagApi?.SetAttribute(player, TagType.NameColor, $"{{{chatcolors}}}");
                            player.PrintToChat($"{_plugin.Localizer["Prefix"]}{{{chatcolors}}}{_plugin.Localizer["NewNameColor", chatcolors]}".ReplaceColorTags().Replace("{TeamColor}", ChatColors.ForTeam(player.Team).ToString()));
                            _plugin.Players[player.AuthorizedSteamID!.SteamId64]!.namecolor = chatcolors;
                            break;
                    }
                });
            }
            menu!.Display(player, 0);
        }

    }
}