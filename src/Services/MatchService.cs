using BasicFaceitServer.Configs;
using BasicFaceitServer.Core;
using BasicFaceitServer.Services.Interfaces;
using BasicFaceitServer.Infrastructure;
using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Modules.Utils;
using MatchState = BasicFaceitServer.Core.MatchState;

namespace BasicFaceitServer.Services;

public class MatchService : IMatchInterface
{
    private readonly BasicFaceitServer _plugin;
    private readonly PluginConfig _config;

    private static IGameInterface _gameService = PluginContext.GameService;

    public MatchState State { get; private set; } = MatchState.Sleeping;
    public CsTeam KnifeRoundWinnerTeam { get; set; }

    public MatchService(BasicFaceitServer plugin, PluginConfig config)
    {
        _plugin = plugin;
        _config = config;
    }

    public void SetState(MatchState state)
    {
        PluginLogger.Info($"Updating match state to - {state.ToString()}");
        if (!Enum.IsDefined(typeof(MatchState), state)) return;

        PluginLogger.Info("Match state updated");
        State = state;
    }

    public bool IsPreWarmup()
    {
        return State == MatchState.PreKnifeWarmup;
    }

    public bool IsPostWarmup()
    {
        return State == MatchState.PostKnifeWarmup;
    }

    public bool IsKnife()
    {
        return State == MatchState.Knife;
    }

    public bool IsMatchLive()
    {
        return State == MatchState.MatchLive;
    }

    public bool IsSleeping()
    {
        return State == MatchState.Sleeping;
    }

    public MatchState GetCurrentGameState()
    {
        return State;
    }

    public void StartPreKnifeWarmup()
    {
        PluginLogger.Info("Start pre knife warmup phase");
        string[] warmupCommands = [
            $"mp_respawn_immunitytime 2",
            $"mp_warmuptime {_config.PreWarmupTime}",
            $"mp_warmup_items_drop_policy 0",
            $"mp_warmup_items_nocost 1",
            $"mp_warmup_items_nocount_policy 1",
            $"mp_warmup_start",
            $"sv_disable_teamselect_menu 1"
        ];

        PluginLogger.Debug($"Pre knife warmup time: {_config.PreWarmupTime}");
        foreach (var cmd in warmupCommands)
            Server.ExecuteCommand(cmd);
        SetState(MatchState.PreKnifeWarmup);
        
        var gameRules = _gameService.GetGameRules();
        if (gameRules == null) return;

        PluginContext.SetWarmupTimes(
            gameRules.WarmupPeriodStart,
            gameRules.WarmupPeriodEnd,
            _config.PreWarmupTime);
    }

    public void StartPostKnifeWarmup()
    {
        PluginLogger.Info("Start post knife warmup phase");

        _plugin.AddTimer(3.0f, () =>
        {
            PluginLogger.Debug($"Post knife warmup time: {_config.PostWarmupTime}");

            Server.ExecuteCommand($"mp_warmuptime {_config.PostWarmupTime}");
            Server.ExecuteCommand($"mp_warmup_start");
        });
        SetState(MatchState.PostKnifeWarmup);
    }

    public void StartKnife()
    {
        PluginLogger.Info("Start knife round");
        if (_gameService.IsWarmup())
        {
            var gameRules = _gameService.GetGameRules();
            gameRules!.WarmupPeriod = false;
        }

        Server.ExecuteCommand("mp_give_player_c4 0; sv_disable_teamselect_menu 1;");

        SetState(MatchState.Knife);
    }

    public void StartMatch()
    {
        PluginLogger.Info("Start live match");
        PluginLogger.Info("Exec gamemode_competitive, restart game (1 sec)");

        if (_gameService.IsWarmup())
        {
            var gameRules = _gameService.GetGameRules();
            gameRules!.WarmupPeriod = false;
        }

        Server.ExecuteCommand("exec gamemode_competitive;");
        _plugin.AddTimer(1.0f, () =>
        {
            Server.ExecuteCommand("sv_disable_teamselect_menu 1; mp_restartgame 1;");
        });
        _gameService.StartRecordingGameDemo();
        SetState(MatchState.MatchLive);
    }

    public void SetKnifeWinnerTeam(CsTeam team)
    {
        PluginLogger.Info($"Define knife round winner: {KnifeRoundWinnerTeam.ToString()}");
        KnifeRoundWinnerTeam = team;
    }

    public CsTeam GetKnifeWinnerTeam()
    {
        return KnifeRoundWinnerTeam;
    }
}