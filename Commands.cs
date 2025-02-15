using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Commands;
using CounterStrikeSharp.API.Core.Attributes.Registration;
using TagsApi;
using Microsoft.Extensions.Logging;
using CounterStrikeSharp.API.Modules.Admin;
namespace CS2Tags_VipTag;

public partial class CS2Tags_VipTag
{

    [ConsoleCommand("css_settag", "Ability for VIP to change their Scoreboard and Chat tag")]
    [CommandHelper(minArgs: 1, usage: "TagName")]
    public async void TagChange(CCSPlayerController? player, CommandInfo commandInfo)
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

            SharedApi_Tag?.SetPlayerTag(player, Tags.Tags_Tags.ScoreTag, newtag);
            if (Players[player.AuthorizedSteamID.SteamId64]!.tagcolor == null)
            {
                SharedApi_Tag?.SetPlayerTag(player!, Tags.Tags_Tags.ChatTag, $"{Players[player.AuthorizedSteamID.SteamId64]!.tag} ");
            }
            else
            {
                SharedApi_Tag?.SetPlayerTag(player, Tags.Tags_Tags.ChatTag, $"{{{Players[player.AuthorizedSteamID!.SteamId64]!.tagcolor}}}{arg} ");
            }
            player.PrintToChat($"{Localizer["Prefix"]}{Localizer["TagSet", arg]}");
            //await AddTag(player, arg);
        }
        catch (Exception ex)
        {
            Logger.LogInformation($"TagChange: {ex}");
        }

        //SharedApi_Tag?.SetPlayerColor(player, Tags.Tags_Colors.NameColor, "{Blue}");
        //SharedApi_Tag?.SetPlayerColor(player, Tags.Tags_Colors.ChatColor, "{DarkRed}");
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
        var menu = _api?.NewMenu(Localizer["VipMenu"]);
        menu?.AddMenuOption($"{Localizer["ToggleTag"]} - {Players[player.AuthorizedSteamID!.SteamId64]!.visibility}", (player, option) =>
        {
            SharedApi_Tag?.SetPlayerToggleTags(player, !SharedApi_Tag.GetPlayerToggleTags(player));

            if (Players[player.AuthorizedSteamID!.SteamId64]!.visibility == false)
            {
                Players[player.AuthorizedSteamID!.SteamId64]!.visibility = true;
                player.PrintToChat($"{Localizer["Prefix"]}{Localizer["Toggled"]}");
            }
            else
            {
                Players[player.AuthorizedSteamID!.SteamId64]!.visibility = false;
                player.PrintToChat($"{Localizer["Prefix"]}{Localizer["UnToggled"]}");
            }
        });
        menu?.AddMenuOption(Localizer["TagColorMenu"], (player, option) =>
        {
            CreateMenu(player, 1);
        });
        menu?.AddMenuOption(Localizer["ChatColorMenu"], (player, option) =>
        {
            CreateMenu(player, 2);
        });
        menu?.AddMenuOption(Localizer["NameColorMenu"], (player, option) =>
        {
            CreateMenu(player, 3);
        });
        menu?.Open(player);
    }
}