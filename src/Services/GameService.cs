using BasicFaceitServer.Configs;
using BasicFaceitServer.Core;
using BasicFaceitServer.Services.Interfaces;
using BasicFaceitServer.Infrastructure;
using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Cvars;

namespace BasicFaceitServer.Services;

public class GameService : IGameInterface
{
    private readonly BasicFaceitServer _plugin;
    private static PluginConfig _config = PluginContext.Config;

    public GameState State { get; private set; }

    public GameService(BasicFaceitServer plugin)
    {
        _plugin = plugin;
    }

    public void SetState(GameState state)
    {
        if (!Enum.IsDefined(typeof(GameState), state))
        {
            PluginLogger.Warn($"Invalid game phase: {state}");
            return;
        }

        State = state;
    }

    public bool IsWarmup()
    {
        var gameRules = GetGameRules();
        return gameRules!.WarmupPeriod;
    }

    public bool IsPaused()
    {
        return State == GameState.Paused;
    }

    public bool IsFriendlyFireOn()
    {
        // TODO: Check friendly fire from cvar and only by nades
        throw new NotImplementedException();
    }

    public bool IsFreezePeriod()
    {
        var gameRules = GetGameRules();
        return gameRules!.FreezePeriod;
    }

    public void PauseMatch()
    {
        PluginLogger.Info($"Pause match");
        Server.ExecuteCommand("mp_pause_match");
        SetState(GameState.Paused);
    }

    public void UnpauseMatch()
    {
        PluginLogger.Info($"Unpause match");
        Server.ExecuteCommand("mp_unpause_match");
        SetState(GameState.Live);
    }
    
    public CCSGameRules? GetGameRules()
    {
        return Utilities.FindAllEntitiesByDesignerName<CCSGameRulesProxy>("cs_gamerules")
            .First()
            .GameRules;
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
