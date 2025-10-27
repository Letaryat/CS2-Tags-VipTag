
using CounterStrikeSharp.API.Core;
using TagsApi;
using Microsoft.Extensions.Logging;
using CounterStrikeSharp.API.Modules.Admin;
using CS2Tags_VipTag.Models;
using static TagsApi.Tags;
using CounterStrikeSharp.API;

namespace CS2Tags_VipTag
{
    public class EventManager(CS2Tags_VipTag plugin)
    {
        private readonly CS2Tags_VipTag _plugin = plugin;

        public void InitializeEvents()
        {
            // _plugin.RegisterEventHandler<EventPlayerSpawn>(OnPlayerSpawn);
            _plugin.RegisterEventHandler<EventPlayerConnectFull>(OnPlayerConnect);
            _plugin.RegisterEventHandler<EventPlayerDisconnect>(OnPlayerDisconnect);
        }

        /*
        public HookResult OnPlayerSpawn(EventPlayerSpawn @event, GameEventInfo info)
        {
            try
            {
                var player = @event.Userid;
                if (player == null || player.IsBot || player.IsHLTV) return HookResult.Continue;
                var steamid64 = player!.AuthorizedSteamID!.SteamId64;
                if (!_plugin.Players.ContainsKey(steamid64)) return HookResult.Continue;
                if (!AdminManager.PlayerHasPermissions(player, _plugin.Config.VipFlag)) return HookResult.Continue;
                //var VipTag = $" {_plugin.Players[steamid64]!.tag}";
                if (_plugin.Players[steamid64]!.visibility == false) { return HookResult.Continue; }
                //_plugin._tagApi?.SetAttribute(player!, Tags.TagType.ScoreTag, VipTag);

                _plugin.TagsManager!.SetEverythingTagRelated(player, 0);

            }
            catch (Exception ex)
            {
                _plugin.Logger.LogInformation($"On_plugin.Playerspawn - {ex}");
            }

            return HookResult.Continue;
        }

        */
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

                        Server.NextFrame(() =>
                        {
                            //var VipTag = $" {_plugin.Players[steamid64]!.tag}";
                            if (_plugin.Players[steamid64]!.visibility == false) { return; }
                            //_plugin._tagApi?.SetAttribute(player!, Tags.TagType.ScoreTag, VipTag);

                            _plugin.TagsManager!.SetEverythingTagRelated(player, 0);
                        });

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
                visibility = user.visibility ?? false,
                chatvisibility = user.chatvisibility ?? false,
                scorevisibility = user.scorevisibility ?? false,
            };
        }

    }
}