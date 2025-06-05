using BasicFaceitServer.Utils;
using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Timers;
using CounterStrikeSharp.API.Modules.Utils;
using Timer = CounterStrikeSharp.API.Modules.Timers.Timer;

namespace BasicFaceitServer.GameStates;

public class GameController(BasicFaceitServer core)
{
    public CsTeam KnifeWinnerTeam = CsTeam.None;
    private readonly MyHelper _helper = core.Helper;
    private readonly GameUtils _game = core.GameUtils;

    private Timer? _c4Countdown;

    public void Load()
    {
    }

    public void StartPreKnifeWarmup()
    {
        MyLogger.Info("Start pre knife warmup phase");
        string[] warmupCommands = [
            $"mp_respawn_immunitytime 2",
            $"mp_warmuptime {core.Config.PreWarmupTime}",
            $"mp_warmup_items_drop_policy 0",
            $"mp_warmup_items_nocost 1",
            $"mp_warmup_items_nocount_policy 1",
            $"mp_warmup_start"
        ];

        MyLogger.Debug($"Post knife warmup time: {core.Config.PreWarmupTime}");
        foreach (var cmd in warmupCommands)
            Server.ExecuteCommand(cmd);
        UpdateGamePhase(GamePhase.PreKnifeWarmup);
        
        var gameRules = _helper.GetGameRules();
        if (gameRules == null) return;
        core.GameListeners.WarmupStartTime = gameRules.WarmupPeriodStart;
        core.GameListeners.WarmupEndTime = gameRules.WarmupPeriodEnd;
    }

    public void StartKnife()
    {
        MyLogger.Info("Start knife round");
        if (_game.IsWarmup())
        {
            var gameRules = _helper.GetGameRules();
            gameRules!.WarmupPeriod = false;
        }

        Server.ExecuteCommand("mp_give_player_c4 0");

        UpdateGamePhase(GamePhase.Knife);
    }

    public void StartPostKnifeWarmup()
    {
        MyLogger.Info("Start post knife warmup phase");

        core.AddTimer(3.0f, () =>
        {
            MyLogger.Debug($"Post knife warmup time: {core.Config.PostWarmupTime}");

            Server.ExecuteCommand($"mp_warmuptime {core.Config.PostWarmupTime}");
            Server.ExecuteCommand($"mp_warmup_start");
        });
        UpdateGamePhase(GamePhase.PostKnifeWarmup);
    }

    public void StartMatch()
    {
        MyLogger.Info("Start live match");
        MyLogger.Info("Exec gamemode_competitive, restart game (1 sec)");

        if (_game.IsWarmup())
        {
            var gameRules = _helper.GetGameRules();
            gameRules!.WarmupPeriod = false;
        }

        Server.ExecuteCommand("exec gamemode_competitive; mp_restartgame 1;");
        UpdateGamePhase(GamePhase.MatchLive);
    }

    public void PauseMatch()
    {
        MyLogger.Info($"Pause match - {_game.GetCurrentGameState().ToString()}");
        Server.ExecuteCommand("mp_pause_match");
        UpdateMatchState(MatchState.Paused);
    }

    public void UnpauseMatch()
    {
        MyLogger.Info($"Unpause match - {_game.GetCurrentGameState().ToString()}");
        Server.ExecuteCommand("mp_unpause_match");
        UpdateMatchState(MatchState.Live);
    }

    public void SetKnifeWinnerTeam(CsTeam winner)
    {
        MyLogger.Info($"Define knife round winner: {winner.ToString()}");
        KnifeWinnerTeam = winner;
    }

    private void UpdateGamePhase(GamePhase value)
    {
        MyLogger.Info($"Updating game phase to - {value.ToString()}");
        if (!Enum.IsDefined(typeof(GamePhase), value)) return;

        core.GamePhase = value;
        MyLogger.Info("Game phase updated");
    }

    private void UpdateMatchState(MatchState value)
    {
        MyLogger.Info($"Updating match state to - {value.ToString()}");
        if (!Enum.IsDefined(typeof(MatchState), value)) return;

        core.MatchState = value;
        MyLogger.Info("Match state updated");
    }

    public void ShowOrganizerMessage()
    {
        core.MatchBeingPlayedIn = true;
        var showTime = 10.0f;

        _c4Countdown = core.AddTimer(1.0f, () =>
        {
            if (showTime <= 0)
            {
                core.MatchBeingPlayedIn = false;
                _c4Countdown?.Kill();
                return;
            }

            showTime -= 1.0f;
        }, TimerFlags.REPEAT);
    }
    
    public void PlayerJoinTeam(CCSPlayerController player, CsTeam playerTeam)
    {
        MyLogger.Info($"Player team - {playerTeam.ToString()}");

        core.AddTimer(0.1f, () =>
        {
            player.ChangeTeam(CsTeam.Spectator);

            if (playerTeam == CsTeam.Spectator) return;

            player.Respawn();
            core.AddTimer(0.1f, () => { player.ChangeTeam(playerTeam); });
        });
    }

    public void KickPlayer(CCSPlayerController player)
    {
        Server.ExecuteCommand($"kickid {player.UserId}");
        MyLogger.Info($"Player kicked - {player.ToString()}");
    }
}