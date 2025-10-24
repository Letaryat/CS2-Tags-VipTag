using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Commands;
using CounterStrikeSharp.API.Core.Attributes.Registration;
using Microsoft.Extensions.Logging;
using CounterStrikeSharp.API.Modules.Admin;
using CS2MenuManager.API.Menu;

namespace CS2Tags_VipTag;

public partial class CS2Tags_VipTag
{

    [ConsoleCommand("css_settag", "Ability for VIP to change their Scoreboard and Chat tag")]
    [CommandHelper(minArgs: 1, usage: "TagName")]
    public void TagChange(CCSPlayerController? player, CommandInfo commandInfo)
    {
        if (player == null || player.IsBot || player.IsHLTV) return;
        if (!AdminManager.PlayerHasPermissions(player, Config.VipFlag))
        {
            player!.PrintToChat($"{Localizer["Prefix"]}{Localizer["NoPermissions"]}");
            return;
        }

        var arg = commandInfo.GetArg(1);
        var newtag = $"{arg} ";
        try
        {
            if (Players.ContainsKey(player.AuthorizedSteamID!.SteamId64))
            {
                Players[player.AuthorizedSteamID.SteamId64]!.tag = arg;
            }
            else
            {
                Players[player.AuthorizedSteamID.SteamId64] = new Player
                {
                    steamid = player.AuthorizedSteamID.SteamId64,
                    tag = arg,
                    tagcolor = null,
                    namecolor = null,
                    chatcolor = null,
                    visibility = true
                };
            }

            _tagApi?.SetAttribute(player, TagsApi.Tags.TagType.ScoreTag, newtag);
            if (Players[player.AuthorizedSteamID.SteamId64]!.tagcolor == null)
            {
                _tagApi?.SetAttribute(player!, TagsApi.Tags.TagType.ChatTag, $"{Players[player.AuthorizedSteamID.SteamId64]!.tag} ");
            }
            else
            {
                _tagApi?.SetAttribute(player, TagsApi.Tags.TagType.ChatTag, $"{{{Players[player.AuthorizedSteamID!.SteamId64]!.tagcolor}}}{arg} ");
            }
            player.PrintToChat($"{Localizer["Prefix"]}{Localizer["TagSet", arg]}");
            //await AddTag(player, arg);
        }
        catch (Exception ex)
        {
            Logger.LogInformation($"TagChange: {ex}");
        }

        //_tagApi?.SetPlayerColor(player, Tags.Tags_Colors.NameColor, "{Blue}");
        //_tagApi?.SetPlayerColor(player, Tags.Tags_Colors.ChatColor, "{DarkRed}");
    }

    [ConsoleCommand("css_tagmenu", "Ability for VIP to change their Scoreboard and Chat tag")]
    public void TagMenu(CCSPlayerController? player, CommandInfo commandInfo)
    {
        if (player == null || player.IsBot || player.IsHLTV) return;
        if (!AdminManager.PlayerHasPermissions(player, Config.VipFlag))
        {
            player!.PrintToChat($"{Localizer["Prefix"]}{Localizer["NoPermissions"]}");
            return;
        }
        if (!Players.ContainsKey(player.AuthorizedSteamID!.SteamId64))
        {
            player.PrintToChat($"{Localizer["Prefix"]}{Localizer["SetupTag"]}");
            return;
        }
        WasdMenu menu = new(Localizer["VipMenu"], this);
        menu?.AddItem($"{Localizer["ToggleTag"]} - {Players[player.AuthorizedSteamID!.SteamId64]!.visibility}", (player, option) =>
        {
            bool currentVisibility = Players[player.AuthorizedSteamID!.SteamId64]!.visibility ?? false;

            bool newVisibility = !currentVisibility;

            Players[player.AuthorizedSteamID!.SteamId64]!.visibility = newVisibility;

            _tagApi?.SetPlayerVisibility(player, newVisibility);

            if (newVisibility)
            {
                _tagApi?.SetAttribute(player, TagsApi.Tags.TagType.ScoreTag, Players[player.AuthorizedSteamID.SteamId64]!.tag);
                _tagApi?.SetAttribute(player, TagsApi.Tags.TagType.ChatColor, $"{{{Players[player.AuthorizedSteamID.SteamId64]!.chatcolor!}}}");
                _tagApi?.SetAttribute(player, TagsApi.Tags.TagType.NameColor, $"{{{Players[player.AuthorizedSteamID.SteamId64]!.namecolor!}}}");
                _tagApi?.SetAttribute(player, TagsApi.Tags.TagType.ChatTag, $"{{{Players[player.AuthorizedSteamID.SteamId64]!.tagcolor}}}{Players[player.AuthorizedSteamID!.SteamId64]!.tag} ");

                player.PrintToChat($"{Localizer["Prefix"]}{Localizer["Toggled"]}");
            }
            else
            {
                player.PrintToChat($"{Localizer["Prefix"]}{Localizer["UnToggled"]}");
            }
        });

        menu?.AddItem(Localizer["TagColorMenu"], (player, option) =>
        {
            CreateMenu(player, 1);
        });
        menu?.AddItem(Localizer["ChatColorMenu"], (player, option) =>
        {
            CreateMenu(player, 2);
        });
        menu?.AddItem(Localizer["NameColorMenu"], (player, option) =>
        {
            CreateMenu(player, 3);
        });
        menu?.Display(player, 0);
    }
}