using CounterStrikeSharp.API.Core;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;
using TagsApi;
using CS2Tags_VipTag.Models;

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
    public override string ModuleVersion => "0.3.1";
    public override string ModuleAuthor => "Letaryat";
    public override string ModuleDescription => "Tag change for vip players";
    public ITagApi _tagApi = null!;
    public required TagConfig Config { get; set; }
    public readonly ConcurrentDictionary<ulong, PlayerModel?> Players = new();

    public DatabaseManager? DatabaseManager { get; private set; }
    public EventManager? EventManager { get; private set; }
    public MenuManager? MenuManager { get; private set; }
    public CommandManager? CmdManager { get; private set; }
    public TagsManager? TagsManager { get; private set; }
    public List<string> Colors =
        [
        "TeamColor", "White", "DarkRed", "Green", "LightYellow", "LightBlue", "Olive", "Lime", "Red", "LightPurple", "Purple", "Grey", "Yellow", "Gold", "Silver", "Blue","DarkBlue", "BlueGrey", "Magenta", "LightRed", "Orange"
        ];
    public override void Load(bool hotReload)
    {

        DatabaseManager = new DatabaseManager(this);
        EventManager = new EventManager(this);
        MenuManager = new MenuManager(this);
        CmdManager = new CommandManager(this);
        TagsManager = new TagsManager(this);

        DatabaseManager.InitializeConnection();
        EventManager.InitializeEvents();
        CmdManager.InitializeCommands();

        Logger.LogInformation("CS2Tags_VipTag - Loaded");

    }

    public override void OnAllPluginsLoaded(bool hotReload)
    {
        _tagApi = ITagApi.Capability.Get() ?? throw new Exception("Tags API not found!");
    }

    public override void Unload(bool hotReload)
    {
        Logger.LogInformation("CS2Tags_VipTag - Unloaded");
        if(DatabaseManager != null)
        {
            _ = DatabaseManager!.SaveAllTags();
        }
        
    }
    public void OnConfigParsed(TagConfig config)
    {
        Config = config;
    }

}
