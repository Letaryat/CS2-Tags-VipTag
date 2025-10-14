using CounterStrikeSharp.API.Core;
using TagsApi;
using Microsoft.Extensions.Logging;
using CounterStrikeSharp.API.Modules.Admin;

namespace CS2Tags_VipTag;

public partial class CS2Tags_VipTag
{
    public HookResult OnPlayerSpawn(EventPlayerSpawn @event, GameEventInfo info)
    {
        try
        {
            var player = @event.Userid;
            if (player == null || player.IsBot || player.IsHLTV) return HookResult.Continue;
            var steamid64 = player!.AuthorizedSteamID!.SteamId64;
            if (!Players.ContainsKey(steamid64)) return HookResult.Continue;
            if (!AdminManager.PlayerHasPermissions(player, Config.VipFlag)) return HookResult.Continue;
            var VipTag = $" {Players[steamid64]!.tag}";
            if (Players[steamid64]!.visibility == false) { return HookResult.Continue; }
            _tagApi?.SetAttribute(player!, Tags.TagType.ScoreTag | Tags.TagType.ChatTag, VipTag);
            if (Players[steamid64]!.chatcolor == null)
            {
                _tagApi?.ResetAttribute(player, Tags.TagType.ChatColor);
            }
            else
            {
                _tagApi?.SetAttribute(player!, Tags.TagType.ChatColor, $"{{{Players[steamid64]!.chatcolor}}}");
            }
            if (Players[steamid64]!.namecolor == null)
            {
                _tagApi?.ResetAttribute(player, Tags.TagType.NameColor);
            }
            else
            {
                _tagApi?.SetAttribute(player!, Tags.TagType.NameColor, $"{{{Players[steamid64]!.namecolor}}}");
            }
            if (Players[steamid64]!.tagcolor == null)
            {
                _tagApi?.SetAttribute(player!, Tags.TagType.ChatTag, $"{Players[steamid64]!.tag} ");
            }
            else
            {
                _tagApi?.SetAttribute(player!, Tags.TagType.ChatTag, $"{{{Players[steamid64]!.tagcolor}}}{Players[steamid64]!.tag} ");
            }
        }
        catch (Exception ex)
        {
            Logger.LogInformation($"OnPlayerSpawn - {ex}");
        }

        return HookResult.Continue;
    }

    public HookResult OnPlayerConnect(EventPlayerConnectFull @event, GameEventInfo info)
    {
        try
        {
            var player = @event.Userid;
            if (player == null || player.IsBot || player.IsHLTV) return HookResult.Continue;
            var steamid64 = player!.AuthorizedSteamID!.SteamId64;
            if (!AdminManager.PlayerHasPermissions(player, Config.VipFlag)) return HookResult.Continue;
            Task.Run(async () =>
            {
                try
                {
                    await OnClientAuthorizedAsync(steamid64);
                }
                catch (Exception ex)
                {
                    Logger.LogInformation($"{ex}");
                }
            });
        }
        catch (Exception ex)
        {
            Logger.LogInformation($"OnPlayerConnectFull - {ex}");
        }
        return HookResult.Continue;
    }


    public HookResult OnPlayerDisconnect(EventPlayerDisconnect @event, GameEventInfo info)
    {
        try
        {
            var player = @event.Userid;
            if (player == null || player.IsBot || player.IsHLTV) return HookResult.Continue;
            var steamid64 = player!.AuthorizedSteamID!.SteamId64;
            if (!Players.ContainsKey(steamid64)) return HookResult.Continue;
            if (!AdminManager.PlayerHasPermissions(player, Config.VipFlag)) return HookResult.Continue;
            Task.Run(async () =>
            {
                try
                {
                    Logger.LogInformation("Saving player into db");
                    await SaveTags(steamid64);
                }
                catch (Exception ex)
                {
                    Logger.LogInformation($"{ex}");
                }
                finally
                {
                    Players.Remove(steamid64, out var _);
                }
            });
        }
        catch (Exception ex)
        {
            Logger.LogInformation($"OnPlayerDisconnect - {ex}");
        }
        return HookResult.Continue;
    }
}