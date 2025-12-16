using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Admin;
using static TagsApi.Tags;

namespace CS2Tags_VipTag
{
    public class TagsManager(CS2Tags_VipTag plugin)
    {
        private readonly CS2Tags_VipTag _plugin = plugin;

        public void SetEverythingTagRelated(CCSPlayerController player, int mode)
        {
            if (player?.AuthorizedSteamID == null)
                return;

            ulong steamid64 = player.AuthorizedSteamID.SteamId64;

            if (!_plugin.Players.TryGetValue(steamid64, out var model))
                return;

            if (model == null) return;

            if (AdminManager.PlayerHasPermissions(player, _plugin.Config.VipScoreboardFlag))
            {
                if (mode == 1 || model.scorevisibility == true)
                    _plugin._tagApi?.SetAttribute(player, TagType.ScoreTag, model.tag);
                else
                    _plugin._tagApi?.ResetAttribute(player, TagType.ScoreTag);
            }
            else
            {
                _plugin._tagApi?.ResetAttribute(player, TagType.ScoreTag);
            }

            // CHAT TAG
            if (AdminManager.PlayerHasPermissions(player, _plugin.Config.VipChatFlag))
            {
                if (mode == 1 || model.chatvisibility == true)
                    SetChatTag(player);
                else
                    _plugin._tagApi?.ResetAttribute(player, TagType.ChatTag);
            }
            else
            {
                _plugin._tagApi?.ResetAttribute(player, TagType.ChatTag);
            }

            SetNameColor(player);
            SetChatColor(player);
        }



        public void SetChatTag(CCSPlayerController player)
        {
            if (player?.AuthorizedSteamID == null)
                return;

            ulong steamid64 = player.AuthorizedSteamID.SteamId64;

            if (!_plugin.Players.TryGetValue(steamid64, out var model))
                return;

            if (model == null) return;

            if (!AdminManager.PlayerHasPermissions(player, _plugin.Config.VipTagColorFlag) || model.tagcolor == null)
                _plugin._tagApi?.SetAttribute(player, TagType.ChatTag, $"{model.tag} ");
            else
                _plugin._tagApi?.SetAttribute(player, TagType.ChatTag, $"{{{model.tagcolor}}}{model.tag} ");
        }


        public void SetNameColor(CCSPlayerController player)
        {
            if (player?.AuthorizedSteamID == null)
                return;

            ulong steamid64 = player.AuthorizedSteamID.SteamId64;

            if (!_plugin.Players.TryGetValue(steamid64, out var model))
                return;

            if (model == null) return;

            if (!AdminManager.PlayerHasPermissions(player, _plugin.Config.VipNameColorFlag) || model.namecolor == null)
            {
                _plugin._tagApi?.ResetAttribute(player, TagType.NameColor);
                return;
            }

            _plugin._tagApi?.SetAttribute(player, TagType.NameColor, $"{{{model.namecolor}}}");
        }

        public void SetChatColor(CCSPlayerController player)
        {
            if (player?.AuthorizedSteamID == null)
                return;

            ulong steamid64 = player.AuthorizedSteamID.SteamId64;


            if (!_plugin.Players.TryGetValue(steamid64, out var model))
                return;

            if (model == null) return;

            if (!AdminManager.PlayerHasPermissions(player, _plugin.Config.VipChatColorFlag) || model.chatcolor == null)
            {
                _plugin._tagApi?.ResetAttribute(player, TagType.ChatColor);
                return;
            }

            _plugin._tagApi?.SetAttribute(player, TagType.ChatColor, $"{{{model.chatcolor}}}");
        }
    }
}