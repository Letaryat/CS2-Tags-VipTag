using CounterStrikeSharp.API.Core;
using static TagsApi.Tags;

namespace CS2Tags_VipTag
{
    public class TagsManager(CS2Tags_VipTag plugin)
    {
        private readonly CS2Tags_VipTag _plugin = plugin;

        public void SetEverythingTagRelated(CCSPlayerController player, int mode)
        {
            if (player == null) return;
            if (mode == 0)
            {
                if (_plugin.Players[player.AuthorizedSteamID!.SteamId64]!.chatvisibility == true)
                {
                    _plugin.TagsManager!.SetChatTag(player);
                }
                if (_plugin.Players[player.AuthorizedSteamID!.SteamId64]!.scorevisibility == true)
                {
                    _plugin._tagApi.SetAttribute(player, TagType.ScoreTag, _plugin.Players[player.AuthorizedSteamID!.SteamId64]!.tag);
                }
            }
            else
            {
                _plugin._tagApi?.SetAttribute(player, TagType.ScoreTag, _plugin.Players[player.AuthorizedSteamID!.SteamId64]!.tag);
                SetChatTag(player);

            }
            SetNameColor(player);
            SetChatColor(player);

        }
        public void SetChatTag(CCSPlayerController player)
        {
            if (player == null) return;
            var steamid64 = player!.AuthorizedSteamID!.SteamId64;
            if (_plugin.Players[steamid64]!.tagcolor == null)
            {
                _plugin._tagApi?.SetAttribute(player!, TagType.ChatTag, $"{_plugin.Players[steamid64]!.tag} ");
            }
            else
            {
                _plugin._tagApi?.SetAttribute(player!, TagType.ChatTag, $"{{{_plugin.Players[steamid64]!.tagcolor}}}{_plugin.Players[steamid64]!.tag} ");
            }
        }

        public void SetNameColor(CCSPlayerController player)
        {
            if (player == null) return;
            var steamid64 = player!.AuthorizedSteamID!.SteamId64;
            if (_plugin.Players[steamid64]!.namecolor == null)
            {
                _plugin._tagApi?.ResetAttribute(player, TagType.NameColor);
            }
            else
            {
                _plugin._tagApi?.SetAttribute(player!, TagType.NameColor, $"{{{_plugin.Players[steamid64]!.namecolor}}}");
            }
        }
        public void SetChatColor(CCSPlayerController player)
        {
            if (player == null) return;
            var steamid64 = player!.AuthorizedSteamID!.SteamId64;
            if (_plugin.Players[steamid64]!.chatcolor == null)
            {
                _plugin._tagApi?.ResetAttribute(player, TagType.ChatColor);
            }
            else
            {
                _plugin._tagApi?.SetAttribute(player!, TagType.ChatColor, $"{{{_plugin.Players[steamid64]!.chatcolor}}}");
            }
        }

    }
}