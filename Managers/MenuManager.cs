using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Translations;
using CS2MenuManager.API.Menu;
using static TagsApi.Tags;

namespace CS2Tags_VipTag
{
    public class MenuManager(CS2Tags_VipTag plugin)
    {
        private readonly CS2Tags_VipTag _plugin = plugin;
        public void CreateMenu(CCSPlayerController? player, int type)
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
                menuOption = $"<font color='{hex}'><b>{chatcolors}</b></font>";

                menu?.AddItem(menuOption, (player, option) =>
                {
                    switch (type)
                    {
                        case 1:
                            string? playertag = _plugin._tagApi?.GetAttribute(player, TagType.ScoreTag);
                            _plugin._tagApi?.SetAttribute(player, TagType.ChatTag, $"{{{chatcolors}}}{_plugin.Players[player.AuthorizedSteamID!.SteamId64]!.tag} ");

                            player.PrintToChat($"{_plugin.Localizer["Prefix"]}{{{chatcolors}}}{_plugin.Localizer["NewTagColor", chatcolors]}".ReplaceColorTags());
                            _plugin.Players[player.AuthorizedSteamID!.SteamId64]!.tagcolor = chatcolors;
                            break;
                        case 2:
                            _plugin._tagApi?.SetAttribute(player, TagType.ChatColor, $"{{{chatcolors}}}");
                            player.PrintToChat($"{_plugin.Localizer["Prefix"]}{{{chatcolors}}}{_plugin.Localizer["NewChatColor", chatcolors]}".ReplaceColorTags());
                            _plugin.Players[player.AuthorizedSteamID!.SteamId64]!.chatcolor = chatcolors;
                            break;
                        case 3:
                            _plugin._tagApi?.SetAttribute(player, TagType.NameColor, $"{{{chatcolors}}}");
                            player.PrintToChat($"{_plugin.Localizer["Prefix"]}{{{chatcolors}}}{_plugin.Localizer["NewNameColor", chatcolors]}".ReplaceColorTags());
                            _plugin.Players[player.AuthorizedSteamID!.SteamId64]!.namecolor = chatcolors;
                            break;
                    }
                });
            }
            menu!.Display(player, 0);
        }

    }
}