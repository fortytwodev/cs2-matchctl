using BasicFaceitServer.Configs;
using BasicFaceitServer.Core;
using BasicFaceitServer.Services.Interfaces;
using BasicFaceitServer.Infrastructure;
using BasicFaceitServer.States;
using CounterStrikeSharp.API;
using MatchState = BasicFaceitServer.Core.MatchState;

namespace BasicFaceitServer.Services;

public class MatchService : IMatchInterface
{
    private readonly BasicFaceitServer _plugin;
    private readonly PluginConfig _config;

    private static IGameInterface _gameService = PluginContext.GameService;
    private static IState _matchState = PluginContext.MatchStateManager;

    public MatchService(BasicFaceitServer plugin, PluginConfig config)
    {
        _plugin = plugin;
        _config = config;
    }

    public void StartPreKnifeWarmup()
    {
        PluginLogger.Info("Start pre knife warmup phase");
        string[] warmupCommands =
        [
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
        _matchState.SetMatchState(MatchState.PreKnifeWarmup);

        var gameRules = _matchState.GetGameRules();
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
        _matchState.SetMatchState(MatchState.PostKnifeWarmup);
    }

    public void StartKnife()
    {
        PluginLogger.Info("Start knife round");
        if (_matchState.IsWarmup())
        {
            var gameRules = _matchState.GetGameRules();
            gameRules!.WarmupPeriod = false;
        }

        Server.ExecuteCommand("mp_give_player_c4 0; sv_disable_teamselect_menu 1;");

        _matchState.SetMatchState(MatchState.KnifeRound);
    }

    public void StartMatch()
    {
        PluginLogger.Info("Start live match");
        PluginLogger.Info("Exec matchctl_live, restart game (1 sec)");

        if (_matchState.IsWarmup())
        {
            var gameRules = _matchState.GetGameRules();
            gameRules!.WarmupPeriod = false;
        }

        Server.ExecuteCommand("exec matchctl_live;");
        _plugin.AddTimer(1.0f, () =>
        {
            Server.ExecuteCommand("sv_disable_teamselect_menu 1; mp_restartgame 1;");
        });
        _gameService.StartRecordingGameDemo();
        _matchState.SetMatchState(MatchState.LiveMatch);
    }
}