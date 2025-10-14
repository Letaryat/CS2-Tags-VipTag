using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Capabilities;
using CounterStrikeSharp.API.Core.Translations;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Logging;
using MySqlConnector;
using Dapper;
using System.Collections.Concurrent;
using System.Drawing;
using CS2MenuManager.API.Menu;
using TagsApi;
using static TagsApi.Tags;

namespace CS2Tags_VipTag;

public class TagConfig : BasePluginConfig
{
    [JsonPropertyName("VipFlag")] public string VipFlag { get; set; } = "@vip/plugin";
    [JsonPropertyName("DBHost")] public string DBHost { get; set; } = "localhost";
    [JsonPropertyName("DBPort")] public uint DBPort { get; set; } = 3306;
    [JsonPropertyName("DBUsername")] public string DBUsername { get; set; } = "root";
    [JsonPropertyName("DBName")] public string DBName { get; set; } = "db_";
    [JsonPropertyName("DBPassword")] public string DBPassword { get; set; } = "123";

}

public partial class CS2Tags_VipTag : BasePlugin, IPluginConfig<TagConfig>
{
    public override string ModuleName => "CS2Tags_VipTag";
    public override string ModuleVersion => "0.0.2";
    public override string ModuleAuthor => "Letaryat";
    public override string ModuleDescription => "Tag change for vip players";
    private ITagApi _tagApi = null!;
    public required TagConfig Config { get; set; }

    public readonly ConcurrentDictionary<ulong, Player?> Players = new();
    public string DbConnection = string.Empty;

    List<string> Colors =
        [
        "Default", "White", "DarkRed", "Green", "LightYellow", "LightBlue", "Olive", "Lime", "Red", "LightPurple", "Purple", "Grey", "Yellow", "Gold", "Silver", "Blue","DarkBlue", "BlueGrey", "Magenta", "LightRed", "Orange"
        ];
    public override void Load(bool hotReload)
    {
        Logger.LogInformation("CS2Tags_VipTag - Loaded");
        RegisterEventHandler<EventPlayerSpawn>(OnPlayerSpawn);
        RegisterEventHandler<EventPlayerConnectFull>(OnPlayerConnect);
        RegisterEventHandler<EventPlayerDisconnect>(OnPlayerDisconnect);
    }

    public override void OnAllPluginsLoaded(bool hotReload)
    {
        _tagApi = ITagApi.Capability.Get() ?? throw new Exception("Tags API not found!");
    }

    public override void Unload(bool hotReload)
    {
        Logger.LogInformation("CS2Tags_VipTag - Unloaded");
        _ = SaveAllTags();
    }

    public async void OnConfigParsed(TagConfig config)
    {
        Config = config;
        if (Config.DBHost.Length < 1 || Config.DBName.Length < 1 || Config.DBPassword.Length < 1 || Config.DBUsername.Length < 1)
        {
            Logger.LogInformation($"You need to setup a mysql database!");
        }

        MySqlConnectionStringBuilder builder = new()
        {
            Server = Config.DBHost,
            UserID = Config.DBUsername,
            Port = Config.DBPort,
            Password = Config.DBPassword,
            Database = Config.DBName,
        };

        DbConnection = builder.ConnectionString;

        try
        {
            var connection = new MySqlConnection(builder.ConnectionString);
            await connection.OpenAsync();
            Logger.LogInformation($"Succesfully connected to mysql database");
            var sqlcmd = connection.CreateCommand();
            string createTable = @"CREATE TABLE IF NOT EXISTS VipTags_Players(
                SteamID VARCHAR(255) PRIMARY KEY,
                Tag VARCHAR(50),
                TagColor VARCHAR(50),
                NameColor VARCHAR(50),
                ChatColor VARCHAR(50),
                Visibility TiNYINT(1)
            );";
            await connection.QueryFirstOrDefaultAsync(createTable);
        }
        catch (Exception ex)
        {
            Logger.LogInformation($"Error while trying to connect to database: {ex}");
            return;
        }
    }


    public string? GetITag(CCSPlayerController controller)
    {
        if (controller == null) { return null; }
        return _tagApi?.GetAttribute(controller, Tags.TagType.ChatTag);
    }

    public async Task<bool> UserExist(ulong SteamID)
    {
        try
        {
            using var connection = new MySqlConnection(DbConnection);
            await connection.OpenAsync();
            string sqlExists = "SELECT COUNT(1) FROM `VipTags_Players` WHERE `SteamID` = @SteamID";
            var exists = await connection.ExecuteScalarAsync<bool>(sqlExists, new { SteamID });
            Logger.LogInformation($"Player {SteamID} do exist");
            return exists;
        }
        catch (Exception ex)
        {
            Logger.LogInformation($"{ex}");
        }
        Logger.LogInformation($"Player {SteamID} does not exists");
        return false;
    }
    public async Task AddTag(CCSPlayerController player, string tag)
    {
        try
        {
            ulong SteamID = 0;
            await Task.Run(() =>
            {
                SteamID = player.AuthorizedSteamID!.SteamId64;
            });
            var userExists = await UserExist(SteamID);
            await using var connection = new MySqlConnection(DbConnection);
            await Task.Run(() => userExists);
            if (userExists)
            {
                Logger.LogInformation($"User exists! Updating tag!");
                await connection.OpenAsync();
                string sqlUpdate = "UPDATE `VipTags_Players` SET `Tag` = @tag WHERE `SteamID` = @SteamID";
                await connection.ExecuteAsync(sqlUpdate, new { SteamID, tag });
                return;
            }
            await connection.OpenAsync();
            string sqlInsert = "INSERT INTO `VipTags_Players` (`SteamID`, `Tag`, `Visibility`) VALUES (@SteamID, @tag, true)";
            await connection.ExecuteAsync(sqlInsert, new { SteamID, tag });
        }
        catch (Exception ex)
        {
            Logger.LogInformation($"{ex}");
        }
        return;
    }

    public async Task SaveTags(ulong SteamID)
    {
        try
        {
            var userExists = await UserExist(SteamID);
            await using var connection = new MySqlConnection(DbConnection);
            await Task.Run(() => userExists);
            var parameters = new
            {
                SteamID,
                Tag = Players[SteamID]!.tag,
                TagColor = Players[SteamID]!.tagcolor ?? null,
                ChatColor = Players[SteamID]!.chatcolor ?? null,
                NameColor = Players[SteamID]!.namecolor ?? null,
                Visibility = Players[SteamID]!.visibility ?? true
            };
            if (userExists)
            {
                Logger.LogInformation($"User exists! Updating tag!");
                await connection.OpenAsync();
                string sqlUpdate = "UPDATE `VipTags_Players` SET `Tag` = @Tag, `TagColor` = @TagColor, `NameColor` = @NameColor, `ChatColor` = @ChatColor, `Visibility` = @Visibility WHERE `SteamID` = @SteamID";
                await connection.ExecuteAsync(sqlUpdate, parameters);
                return;
            }
            await connection.OpenAsync();
            string sqlInsert = "INSERT INTO `VipTags_Players` (`SteamID`, `Tag`, `TagColor`, `NameColor`, `ChatColor`, `Visibility`) VALUES (@SteamID, @Tag, @TagColor, @NameColor, @ChatColor, @Visibility)";
            await connection.ExecuteAsync(sqlInsert, parameters);
        }
        catch (Exception err)
        {
            Logger.LogInformation($"{err}");
        }
        return;
    }

    public async Task SaveAllTags()
    {
        try
        {
            await using var connection = new MySqlConnection(DbConnection);
            await connection.OpenAsync();
            foreach (var kvp in Players)
            {
                var steamid = kvp.Key;
                var player = kvp.Value;
                if (player == null) continue;
                var userExists = await UserExist(steamid);
                var parameters = new
                {
                    SteamID = steamid,
                    Tag = player.tag,
                    TagColor = player.tagcolor ?? null,
                    ChatColor = player.chatcolor ?? null,
                    NameColor = player.namecolor ?? null,
                    Visibility = player.visibility ?? true
                };
                if (userExists)
                {
                    Logger.LogInformation($"Updating tag {steamid}");
                    string sqlUpdate = @"
                    UPDATE `VipTags_Players`
                    SET `Tag` = @Tag, `TagColor` = @TagColor, `NameColor` = @NameColor, 
                        `ChatColor` = @ChatColor, `Visibility` = @Visibility
                    WHERE `SteamID` = @SteamID";
                    await connection.ExecuteAsync(sqlUpdate, parameters);
                }
                else
                {
                    Logger.LogInformation($"Inserting new tag {steamid}");
                    string sqlInsert = @"
                    INSERT INTO `VipTags_Players` 
                    (`SteamID`, `Tag`, `TagColor`, `NameColor`, `ChatColor`, `Visibility`) 
                    VALUES (@SteamID, @Tag, @TagColor, @NameColor, @ChatColor, @Visibility)";
                    await connection.ExecuteAsync(sqlInsert, parameters);
                }
            }
            Logger.LogInformation($"All players have been updated / inserted into DB");
        }
        catch (Exception err)
        {
            Logger.LogInformation($"SaveAllTags - {err}");
        }
        return;
    }
    public async Task ChangeColor(CCSPlayerController player, string color, int type)
    {
        var SteamID = player!.AuthorizedSteamID!.SteamId64;
        var userExists = await UserExist(SteamID);
        string? type1 = null;
        switch (type)
        {
            case 1:
                type1 = "TagColor";
                break;
            case 2:
                type1 = "ChatColor";
                break;
            case 3:
                type1 = "NameColor";
                break;
        }
        try
        {
            await using var connection = new MySqlConnection(DbConnection);
            await Task.Run(() => userExists);
            if (userExists)
            {
                Logger.LogInformation($"User exists! Updating color - {type}!");
                await connection.OpenAsync();
                string sqlUpdate = $"UPDATE `VipTags_Players` SET {type1} = @color WHERE `SteamID` = @SteamID";
                await connection.ExecuteAsync(sqlUpdate, new { color, SteamID });
                return;
            }
            player.PrintToChat($"{Localizer["Prefix"]}{Localizer["SetupTag"]}");
        }
        catch (Exception ex)
        {
            Logger.LogInformation($"ChangeColor Method: {ex}");
        }
    }

    public void CreateMenu(CCSPlayerController? player, int type)
    {
        if (player == null) { return; }
        WasdMenu menu = new("Tags menu", this);
        switch (type)
        {
            case 1:
                menu = new(Localizer["TagColorMenu"], this);
                break;
            case 2:
                menu = new(Localizer["ChatColorMenu"], this);
                break;
            case 3:
                menu = new(Localizer["NameColorMenu"], this);
                break;
        }

        foreach (var chatcolors in Colors)
        {
            string hex = FromNameToHex(chatcolors)!;
            string? menuOption;
            menuOption = $"<font color='{hex}'><b>{chatcolors}</b></font>";

            menu?.AddItem(menuOption, async (player, option) =>
            {
                switch (type)
                {
                    case 1:
                        string? playertag = _tagApi?.GetAttribute(player, TagType.ScoreTag);
                        _tagApi?.SetAttribute(player, TagType.ChatTag, $"{{{chatcolors}}}{Players[player.AuthorizedSteamID!.SteamId64]!.tag} ");

                        player.PrintToChat($"{Localizer["Prefix"]}{{{chatcolors}}}{Localizer["NewTagColor", chatcolors]}".ReplaceColorTags());
                        Players[player.AuthorizedSteamID!.SteamId64]!.tagcolor = chatcolors;
                        break;
                    case 2:
                        _tagApi?.SetAttribute(player, TagType.ChatColor, $"{{{chatcolors}}}");
                        player.PrintToChat($"{Localizer["Prefix"]}{{{chatcolors}}}{Localizer["NewChatColor", chatcolors]}".ReplaceColorTags());
                        Players[player.AuthorizedSteamID!.SteamId64]!.chatcolor = chatcolors;
                        break;
                    case 3:
                        _tagApi?.SetAttribute(player, TagType.NameColor, $"{{{chatcolors}}}");
                        player.PrintToChat($"{Localizer["Prefix"]}{{{chatcolors}}}{Localizer["NewNameColor", chatcolors]}".ReplaceColorTags());
                        Players[player.AuthorizedSteamID!.SteamId64]!.namecolor = chatcolors;
                        break;
                }
            });
        }
        menu!.Display(player, 0);
    }


    public static string? FromNameToHex(string name)
    {
        try
        {
            Dictionary<string, string> predefinedColors = new Dictionary<string, string>
            {
                {"Default", "#FFFFFF"},
                {"BlueGrey", "#B1C4D9"},
                {"Grey", "#C6CBD0"},
                {"LightPurple", "#BB82F0"},
                {"LightRed", "#EB4C4C"},
            };
            Color color = Color.FromName(name);
            if (!color.IsKnownColor && !color.IsNamedColor) return null;
            if (predefinedColors.ContainsKey(name))
            {
                return predefinedColors[name];
            }
            return $"#{color.R:X2}{color.G:X2}{color.B:X2}";
        }
        catch
        {
            return null;
        }
    }
    public async Task<Player?> FetchPlayerInfo(ulong SteamID)
    {
        await using var connection = new MySqlConnection(DbConnection);
        var userExists = await UserExist(SteamID);
        try
        {
            await Task.Run(() => userExists);
            if (!userExists)
            {
                Logger.LogInformation($"No player in database with steamid: {SteamID}");
                return null;
            }
            await connection.OpenAsync();
            string sqlSelect = $"SELECT * FROM `VipTags_Players` WHERE `SteamID` = {SteamID}";
            var user = await connection.QueryFirstOrDefaultAsync<Player>(sqlSelect);
            return user;
        }
        catch (Exception ex)
        {
            Logger.LogInformation($"Fetchplayerinfo: {ex}");
        }
        return null;
    }

    public async Task OnClientAuthorizedAsync(ulong steamid)
    {
        var user = await FetchPlayerInfo(steamid);
        if (user == null) return;

        Players[steamid] = new Player
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
