using System.Reflection;
using BasicFaceitServer.Configs;
using BasicFaceitServer.Services;
using BasicFaceitServer.Services.Interfaces;
using BasicFaceitServer.States;
using CounterStrikeSharp.API;

namespace BasicFaceitServer.Core;

public static class PluginContext
{
    private static BasicFaceitServer? _plugin;
    private static PluginConfig? _config;
    private static MatchStateManager? _matchState;
    private static PlayerService? _playerService;
    private static GameService? _gameService;
    private static MatchService? _matchService;
    private static ChatService? _chatService;

    public static BasicFaceitServer Plugin => _plugin ?? throw new InvalidOperationException("Plugin not initialized");
    public static PluginConfig Config => _config ?? throw new InvalidOperationException("Config not initialized");
    public static IPlayerInterface PlayerService => _playerService ?? throw new InvalidOperationException("Player service not initialized");
    public static IGameInterface GameService => _gameService ?? throw new InvalidOperationException("Game service not initialized");
    public static IMatchInterface MatchService => _matchService ?? throw new InvalidOperationException("Match service not initialized");
    public static IChatInterface ChatService => _chatService ?? throw new InvalidOperationException("Chat service not initialized");
    public static IState MatchStateManager => _matchState ?? throw new InvalidOperationException("Match state not initialized");

    public static float WarmupStartTime { get; set; }
    public static float WarmupEndTime { get; set; }
    public static int WarmupTime { get; set; }
    public static DateTime LastMessageTime { get; set; }
    public static DateTime LastWarmupEndValueCheckTime { get; set; }
    
    public static void Initialize(BasicFaceitServer plugin, string moduleDirectory)
    {
        _plugin = plugin;
        _config = ConfigLoader.Load(moduleDirectory);
        _matchState = new MatchStateManager();
        _playerService = new PlayerService(_plugin, _config);
        _gameService = new GameService(_plugin);
        _matchService = new MatchService(_plugin, _config);
        _chatService = new ChatService(_plugin, _config, _playerService);
    }
    
    public static void SetWarmupTimes(float startTime, float endTime, int warmupTime)
    {
        WarmupStartTime = startTime;
        WarmupEndTime = endTime;
        WarmupTime = warmupTime;
        LastMessageTime = DateTime.Now.AddSeconds(-Config.WarmupMessageIntervalSeconds);
        LastWarmupEndValueCheckTime = DateTime.Now;
    }

    public static void ExecBaseCfgFile()
    {
        var assembly = Assembly.GetExecutingAssembly();
        var resourceName = "BasicFaceitServer.src.Core.GameCfg.matchctl_live.cfg";
        var cfgPath = Path.Combine(Server.GameDirectory, "csgo", "cfg", "matchctl_live.cfg");

        if (!File.Exists(cfgPath))
        {
            using var stream = assembly.GetManifestResourceStream(resourceName);
            using var fileStream = File.Create(cfgPath);
            stream?.CopyTo(fileStream);
        }

        Server.ExecuteCommand("exec gamemode_competitive");
        Server.NextFrame(() =>
        {
            Server.ExecuteCommand("exec matchctl_live");
        });
    }
}
