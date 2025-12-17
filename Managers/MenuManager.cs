using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Translations;
using CounterStrikeSharp.API.Modules.Utils;
using CS2MenuManager.API.Enum;
using CS2MenuManager.API.Menu;
using CS2Tags_VipTag.Models;
using static TagsApi.Tags;

namespace CS2Tags_VipTag
{
    public class MenuManager(CS2Tags_VipTag plugin)
    {
        private readonly CS2Tags_VipTag _plugin = plugin;

        private bool TryGetModel(CCSPlayerController? player, out PlayerModel model)
        {
            model = null!;

            if (player?.AuthorizedSteamID == null)
                return false;

            return _plugin.Players.TryGetValue(
                player.AuthorizedSteamID.SteamId64,
                out model!
            );
        }


        public void CreateDisableMenu(CCSPlayerController player, WasdMenu? parentMenu)
        {
            if (!TryGetModel(player, out var model))
                return;

            WasdMenu menu = new("Disable menu", _plugin)
            {
                PrevMenu = parentMenu
            };
            menu.AddItem(
                $"{_plugin.Localizer["ToggleEverythingMenu"]} - [{model.visibility}]",
                (p, o) =>
                {
                    if (!TryGetModel(p, out var m))
                    {
                        o.PostSelectAction = PostSelectAction.Close;
                        return;
                    }

                    m.visibility = !(m.visibility ?? false);
                    _plugin._tagApi?.SetPlayerVisibility(p, m.visibility ?? true);

                    if (m.visibility == true)
                    {
                        _plugin.TagsManager!.SetEverythingTagRelated(p, 1);
                        p.PrintToChat($"{_plugin.Localizer["Prefix"]}{_plugin.Localizer["Toggled"]}");
                    }
                    else
                    {
                        p.PrintToChat($"{_plugin.Localizer["Prefix"]}{_plugin.Localizer["UnToggled"]}");
                    }

                    o.PostSelectAction = PostSelectAction.Close;
                    Server.NextWorldUpdate(() => CreateDisableMenu(p, parentMenu));
                }
            );

            menu.AddItem(
                $"{_plugin.Localizer["ToggleScoreTagMenu"]} - [{model.scorevisibility}]",
                (p, o) =>
                {
                    if (!TryGetModel(p, out var m))
                    {
                        o.PostSelectAction = PostSelectAction.Close;
                        return;
                    }

                    m.scorevisibility = !(m.scorevisibility ?? false);

                    if (m.scorevisibility == true)
                    {
                        _plugin._tagApi?.SetAttribute(p, TagType.ScoreTag, m.tag);
                        p.PrintToChat($"{_plugin.Localizer["Prefix"]}{_plugin.Localizer["ToggledScoreTag"]}");
                    }
                    else
                    {
                        _plugin._tagApi?.ResetAttribute(p, TagType.ScoreTag);
                        p.PrintToChat($"{_plugin.Localizer["Prefix"]}{_plugin.Localizer["UnToggledScoreTag"]}");
                    }

                    o.PostSelectAction = PostSelectAction.Close;
                    Server.NextWorldUpdate(() => CreateDisableMenu(p, parentMenu));
                }
            );

            menu.AddItem(
                $"{_plugin.Localizer["ToggleChatMenu"]} - [{model.chatvisibility}]",
                (p, o) =>
                {
                    if (!TryGetModel(p, out var m))
                    {
                        o.PostSelectAction = PostSelectAction.Close;
                        return;
                    }

                    m.chatvisibility = !(m.chatvisibility ?? false);

                    if (m.chatvisibility == true)
                    {
                        _plugin.TagsManager!.SetChatTag(p);
                        p.PrintToChat($"{_plugin.Localizer["Prefix"]}{_plugin.Localizer["ToggledChatTag"]}");
                    }
                    else
                    {
                        _plugin._tagApi?.ResetAttribute(p, TagType.ChatTag);
                        p.PrintToChat($"{_plugin.Localizer["Prefix"]}{_plugin.Localizer["UnToggledChatTag"]}");
                    }

                    o.PostSelectAction = PostSelectAction.Close;
                    Server.NextWorldUpdate(() => CreateDisableMenu(p, parentMenu));
                }
            );

            menu.Display(player, 0);
        }

        public void CreateMenuWithColors(CCSPlayerController? player, int type, WasdMenu? parentMenu)
        {
            if (player == null) return;
            if (!TryGetModel(player, out var model))
                return;

            WasdMenu menu = type switch
            {
                1 => new(_plugin.Localizer["TagColorMenu"], _plugin),
                2 => new(_plugin.Localizer["ChatColorMenu"], _plugin),
                3 => new(_plugin.Localizer["NameColorMenu"], _plugin),
                _ => new(_plugin.Localizer["TagsMenu"], _plugin)
            };

            menu.PrevMenu = parentMenu;

            foreach (var color in _plugin.Colors)
            {
                string hex = PluginUtilities.FromNameToHex(color) ?? "#FFFFFF";

                menu.AddItem(
                    $"<font color='{hex}'><b>{color}</b></font>",
                    (p, o) =>
                    {
                        if (!TryGetModel(p, out var m))
                        {
                            o.PostSelectAction = PostSelectAction.Close;
                            return;
                        }

                        switch (type)
                        {
                            case 1:
                                m.tagcolor = color;
                                player.PrintToChat($"{_plugin.Localizer["Prefix"]}{{{color}}}{_plugin.Localizer["NewTagColor", color]}".ReplaceColorTags().Replace("{TeamColor}", ChatColors.ForTeam(player.Team).ToString()));
                                _plugin.TagsManager!.SetChatTag(p);
                                break;

                            case 2:
                                m.chatcolor = color;
                                player.PrintToChat($"{_plugin.Localizer["Prefix"]}{{{color}}}{_plugin.Localizer["NewChatColor", color]}".ReplaceColorTags().Replace("{TeamColor}", ChatColors.ForTeam(player.Team).ToString()));
                                _plugin._tagApi?.SetAttribute(p, TagType.ChatColor, $"{{{color}}}");
                                break;

                            case 3:
                                m.namecolor = color;
                                player.PrintToChat($"{_plugin.Localizer["Prefix"]}{{{color}}}{_plugin.Localizer["NewNameColor", color]}".ReplaceColorTags().Replace("{TeamColor}", ChatColors.ForTeam(player.Team).ToString()));
                                _plugin._tagApi?.SetAttribute(p, TagType.NameColor, $"{{{color}}}");
                                break;
                        }

                        o.PostSelectAction = PostSelectAction.Nothing;
                    }
                );
            }

            menu.Display(player!, 0);
        }


    }
}