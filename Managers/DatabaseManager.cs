using System.Text.Json.Serialization;
using Microsoft.Extensions.Logging;
using MySqlConnector;
using Dapper;
using CounterStrikeSharp.API.Core;
using CS2Tags_VipTag.Models;

namespace CS2Tags_VipTag
{
    public class DatabaseManager(CS2Tags_VipTag plugin)
    {
        private readonly CS2Tags_VipTag _plugin = plugin;
        public string DbConnection = string.Empty;
        public async void InitializeConnection()
        {
            var Config = _plugin.Config;
            if (Config.DBHost.Length < 1 || Config.DBName.Length < 1 || Config.DBPassword.Length < 1 || Config.DBUsername.Length < 1)
            {
                _plugin.Logger.LogInformation($"You need to setup a mysql database!");
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
                _plugin.Logger.LogInformation($"Succesfully connected to mysql database");
                var sqlcmd = connection.CreateCommand();
                string createTable = @"CREATE TABLE IF NOT EXISTS VipTags_Players(
                SteamID VARCHAR(255) PRIMARY KEY,
                Tag VARCHAR(50),
                TagColor VARCHAR(50),
                NameColor VARCHAR(50),
                ChatColor VARCHAR(50),
                Visibility TiNYINT(1),
                ChatVisibility TiNYINT(1),
                ScoreVisibility TiNYINT(1)
            );";
                await connection.QueryFirstOrDefaultAsync(createTable);
            }
            catch (Exception ex)
            {
                _plugin.Logger.LogInformation($"Error while trying to connect to database: {ex}");
                return;
            }
        }

        public async Task<bool> UserExist(ulong SteamID)
        {
            try
            {
                using var connection = new MySqlConnection(DbConnection);
                await connection.OpenAsync();
                string sqlExists = "SELECT COUNT(1) FROM `VipTags_Players` WHERE `SteamID` = @SteamID";
                var exists = await connection.ExecuteScalarAsync<bool>(sqlExists, new { SteamID });
                _plugin.Logger.LogInformation($"Player {SteamID} do exist");
                return exists;
            }
            catch (Exception ex)
            {
                _plugin.Logger.LogInformation($"{ex}");
            }
            _plugin.Logger.LogInformation($"Player {SteamID} does not exists");
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
                    _plugin.Logger.LogInformation($"User exists! Updating tag!");
                    await connection.OpenAsync();
                    string sqlUpdate = "UPDATE `VipTags_Players` SET `Tag` = @tag WHERE `SteamID` = @SteamID";
                    await connection.ExecuteAsync(sqlUpdate, new { SteamID, tag });
                    return;
                }
                await connection.OpenAsync();
                string sqlInsert = "INSERT INTO `VipTags_Players` (`SteamID`, `Tag`, `Visibility`, `ChatVisibility`, `ScoreVisibility`) VALUES (@SteamID, @tag, true, true, true)";
                await connection.ExecuteAsync(sqlInsert, new { SteamID, tag });
            }
            catch (Exception ex)
            {
                _plugin.Logger.LogInformation($"{ex}");
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
                    Tag = _plugin.Players[SteamID]!.tag,
                    TagColor = _plugin.Players[SteamID]!.tagcolor ?? null,
                    ChatColor = _plugin.Players[SteamID]!.chatcolor ?? null,
                    NameColor = _plugin.Players[SteamID]!.namecolor ?? null,
                    Visibility = _plugin.Players[SteamID]!.visibility ?? true,
                    ChatVisibility = _plugin.Players[SteamID]!.chatvisibility ?? true,
                    ScoreVisibility = _plugin.Players[SteamID]!.scorevisibility ?? true,
                };
                if (userExists)
                {
                    _plugin.Logger.LogInformation($"User exists! Updating tag!");
                    await connection.OpenAsync();
                    string sqlUpdate = "UPDATE `VipTags_Players` SET `Tag` = @Tag, `TagColor` = @TagColor, `NameColor` = @NameColor, `ChatColor` = @ChatColor, `Visibility` = @Visibility, `ChatVisibility` = @ChatVisibility, `ScoreVisibility` = @ScoreVisibility WHERE `SteamID` = @SteamID";
                    await connection.ExecuteAsync(sqlUpdate, parameters);
                    return;
                }
                await connection.OpenAsync();
                string sqlInsert = "INSERT INTO `VipTags_Players` (`SteamID`, `Tag`, `TagColor`, `NameColor`, `ChatColor`, `Visibility`, `ChatVisibility`, `ScoreVisibility`) VALUES (@SteamID, @Tag, @TagColor, @NameColor, @ChatColor, @Visibility, @ChatVisibility, @ScoreVisibility)";
                await connection.ExecuteAsync(sqlInsert, parameters);
            }
            catch (Exception err)
            {
                _plugin.Logger.LogInformation($"{err}");
            }
            return;
        }

        public async Task SaveAllTags()
        {
            try
            {
                await using var connection = new MySqlConnection(DbConnection);
                await connection.OpenAsync();
                foreach (var kvp in _plugin.Players)
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
                        Visibility = player.visibility ?? true,
                        ChatVis = player.chatvisibility ?? true,
                        ScoreVis = player.scorevisibility ?? true
                    };
                    if (userExists)
                    {
                        _plugin.Logger.LogInformation($"Updating tag {steamid}");
                        string sqlUpdate = @"
                    UPDATE `VipTags_Players`
                    SET `Tag` = @Tag, `TagColor` = @TagColor, `NameColor` = @NameColor, 
                        `ChatColor` = @ChatColor, `Visibility` = @Visibility, `ChatVisibility` = @ChatVis, `ScoreVisibility` = @ScoreVis
                    WHERE `SteamID` = @SteamID";
                        await connection.ExecuteAsync(sqlUpdate, parameters);
                    }
                    else
                    {
                        _plugin.Logger.LogInformation($"Inserting new tag {steamid}");
                        string sqlInsert = @"
                    INSERT INTO `VipTags_Players` 
                    (`SteamID`, `Tag`, `TagColor`, `NameColor`, `ChatColor`, `Visibility`, `ChatVisibility`, `ScoreVisibility`) 
                    VALUES (@SteamID, @Tag, @TagColor, @NameColor, @ChatColor, @Visibility, @ChatVis, @ScoreVis)";
                        await connection.ExecuteAsync(sqlInsert, parameters);
                    }
                }
                _plugin.Logger.LogInformation($"All players have been updated / inserted into DB");
            }
            catch (Exception err)
            {
                _plugin.Logger.LogInformation($"SaveAllTags - {err}");
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
                    _plugin.Logger.LogInformation($"User exists! Updating color - {type}!");
                    await connection.OpenAsync();
                    string sqlUpdate = $"UPDATE `VipTags_Players` SET {type1} = @color WHERE `SteamID` = @SteamID";
                    await connection.ExecuteAsync(sqlUpdate, new { color, SteamID });
                    return;
                }
                player.PrintToChat($"{_plugin.Localizer["Prefix"]}{_plugin.Localizer["SetupTag"]}");
            }
            catch (Exception ex)
            {
                _plugin.Logger.LogInformation($"ChangeColor Method: {ex}");
            }
        }

    public async Task<PlayerModel?> FetchPlayerInfo(ulong SteamID)
    {
        await using var connection = new MySqlConnection(DbConnection);
        var userExists = await UserExist(SteamID);
        try
        {
            await Task.Run(() => userExists);
            if (!userExists)
            {
                _plugin.Logger.LogInformation($"No player in database with steamid: {SteamID}");
                return null;
            }
            await connection.OpenAsync();
            string sqlSelect = $"SELECT * FROM `VipTags_Players` WHERE `SteamID` = {SteamID}";
            var user = await connection.QueryFirstOrDefaultAsync<PlayerModel>(sqlSelect);
            return user;
        }
        catch (Exception ex)
        {
            _plugin.Logger.LogInformation($"Fetchplayerinfo: {ex}");
        }
        return null;
    }

    }
}