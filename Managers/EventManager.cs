
using CounterStrikeSharp.API.Core;
using TagsApi;
using Microsoft.Extensions.Logging;
using CounterStrikeSharp.API.Modules.Admin;
using CS2Tags_VipTag.Models;

namespace CS2Tags_VipTag
{
    public class EventManager(CS2Tags_VipTag plugin)
    {
        private readonly CS2Tags_VipTag _plugin = plugin;

        public void InitializeEvents()
        {
            _plugin.RegisterEventHandler<EventPlayerSpawn>(OnPlayerSpawn);
            _plugin.RegisterEventHandler<EventPlayerConnectFull>(OnPlayerConnect);
            _plugin.RegisterEventHandler<EventPlayerDisconnect>(OnPlayerDisconnect);
        }

        public HookResult OnPlayerSpawn(EventPlayerSpawn @event, GameEventInfo info)
        {
            try
            {
                var player = @event.Userid;
                if (player == null || player.IsBot || player.IsHLTV) return HookResult.Continue;
                var steamid64 = player!.AuthorizedSteamID!.SteamId64;
                if (!_plugin.Players.ContainsKey(steamid64)) return HookResult.Continue;
                if (!AdminManager.PlayerHasPermissions(player, _plugin.Config.VipFlag)) return HookResult.Continue;
                var VipTag = $" {_plugin.Players[steamid64]!.tag}";
                if (_plugin.Players[steamid64]!.visibility == false) { return HookResult.Continue; }
                _plugin._tagApi?.SetAttribute(player!, Tags.TagType.ScoreTag | Tags.TagType.ChatTag, VipTag);
                if (_plugin.Players[steamid64]!.chatcolor == null)
                {
                    _plugin._tagApi?.ResetAttribute(player, Tags.TagType.ChatColor);
                }
                else
                {
                    _plugin._tagApi?.SetAttribute(player!, Tags.TagType.ChatColor, $"{{{_plugin.Players[steamid64]!.chatcolor}}}");
                }
                if (_plugin.Players[steamid64]!.namecolor == null)
                {
                    _plugin._tagApi?.ResetAttribute(player, Tags.TagType.NameColor);
                }
                else
                {
                    _plugin._tagApi?.SetAttribute(player!, Tags.TagType.NameColor, $"{{{_plugin.Players[steamid64]!.namecolor}}}");
                }
                if (_plugin.Players[steamid64]!.tagcolor == null)
                {
                    _plugin._tagApi?.SetAttribute(player!, Tags.TagType.ChatTag, $"{_plugin.Players[steamid64]!.tag} ");
                }
                else
                {
                    _plugin._tagApi?.SetAttribute(player!, Tags.TagType.ChatTag, $"{{{_plugin.Players[steamid64]!.tagcolor}}}{_plugin.Players[steamid64]!.tag} ");
                }
            }
            catch (Exception ex)
            {
                _plugin.Logger.LogInformation($"On_plugin.Playerspawn - {ex}");
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
                if (!AdminManager.PlayerHasPermissions(player, _plugin.Config.VipFlag)) return HookResult.Continue;
                Task.Run(async () =>
                {
                    try
                    {
                        await OnClientAuthorizedAsync(steamid64);
                    }
                    catch (Exception ex)
                    {
                        _plugin.Logger.LogInformation($"{ex}");
                    }
                });
            }
            catch (Exception ex)
            {
                _plugin.Logger.LogInformation($"OnPlayerConnectFull - {ex}");
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
                if (!_plugin.Players.ContainsKey(steamid64)) return HookResult.Continue;
                if (!AdminManager.PlayerHasPermissions(player, _plugin.Config.VipFlag)) return HookResult.Continue;
                Task.Run(async () =>
                {
                    try
                    {
                        _plugin.Logger.LogInformation("Saving player into db");
                        await _plugin.DatabaseManager!.SaveTags(steamid64);
                    }
                    catch (Exception ex)
                    {
                        _plugin.Logger.LogInformation($"{ex}");
                    }
                    finally
                    {
                        _plugin.Players.Remove(steamid64, out var _);
                    }
                });
            }
            catch (Exception ex)
            {
                _plugin.Logger.LogInformation($"OnPlayerDisconnect - {ex}");
            }
            return HookResult.Continue;
        }

        public async Task OnClientAuthorizedAsync(ulong steamid)
        {
            var user = await _plugin.DatabaseManager!.FetchPlayerInfo(steamid);
            if (user == null) return;

            _plugin.Players[steamid] = new PlayerModel
            {
                steamid = user!.steamid,
                tag = user.tag,
                tagcolor = user.tagcolor,
                namecolor = user.namecolor,
                chatcolor = user.chatcolor,
                visibility = user.visibility ?? false
            };
        }

    }
}