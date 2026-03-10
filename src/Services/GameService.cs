using BasicFaceitServer.Configs;
using BasicFaceitServer.Core;
using BasicFaceitServer.Services.Interfaces;
using BasicFaceitServer.Infrastructure;
using BasicFaceitServer.States;
using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Modules.Cvars;

namespace BasicFaceitServer.Services;

public class GameService : IGameInterface
{
    private readonly BasicFaceitServer _plugin;
    private static PluginConfig _config = PluginContext.Config;
    private static IState _matchState = PluginContext.MatchStateManager;

    public GameService(BasicFaceitServer plugin)
    {
        _plugin = plugin;
    }

    public void PauseMatch()
    {
        PluginLogger.Info($"Pause match");
        Server.ExecuteCommand("mp_pause_match");
        _matchState.SetGameState(GameState.Paused);
    }

    public void UnpauseMatch()
    {
        PluginLogger.Info($"Unpause match");
        Server.ExecuteCommand("mp_unpause_match");
        _matchState.SetGameState(GameState.Unpaused);
    }

    public void StartRecordingGameDemo()
    {
        if (!_config.RecordGameDemo) return;

        PluginLogger.Info("Start recording game");

        var team1 = ConVar.Find("mp_teamname_1")?.StringValue ?? "ct";
        var team2 = ConVar.Find("mp_teamname_2")?.StringValue ?? "t";
        var mapName = Server.MapName;
        var todayDate = DateTime.Now.ToString("dd-MM-yyyy-HH-mm");
        var demoFilename = $"{team1}_vs_{team2}_{mapName}_{todayDate}";

        PluginLogger.Info($"Demo filename: {demoFilename}");
        Server.ExecuteCommand($"tv_record {demoFilename}");
    }

    public void StopRecordingGameDemo()
    {
        _plugin.AddTimer(5.0f, () =>
        {
            PluginLogger.Info("Stop recording game");
            Server.ExecuteCommand("tv_stoprecord");
        });
    }
}