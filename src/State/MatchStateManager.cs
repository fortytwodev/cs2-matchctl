using BasicFaceitServer.Core;
using BasicFaceitServer.Infrastructure;
using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Utils;

namespace BasicFaceitServer.States;

public class MatchStateManager : IState
{
    public MatchState CurrentMatchState { get; private set; } = MatchState.Sleeping;
    public GameState CurrentGameState { get; private set; } = GameState.Unpaused;
    public CsTeam KnifeRoundWinnerTeam { get; private set; } = CsTeam.None;

    public bool IsSleeping => CurrentMatchState == MatchState.Sleeping;
    public bool IsPreWarmup => CurrentMatchState == MatchState.PreKnifeWarmup;
    public bool IsPostWarmup => CurrentMatchState == MatchState.PostKnifeWarmup;
    public bool IsKnifeRound => CurrentMatchState == MatchState.KnifeRound;
    public bool IsLiveMatch => CurrentMatchState == MatchState.LiveMatch;
    public bool IsPaused => CurrentGameState == GameState.Paused;

    public MatchState GetCurrentMatchState => CurrentMatchState;
    public GameState GetCurrentGameState => CurrentGameState;

    public CsTeam GetKnifeRoundWinnerTeam => KnifeRoundWinnerTeam;

    public void SetMatchState(MatchState state)
    {
        PluginLogger.Info($"Updating match state to - {state.ToString()}");
        if (!Enum.IsDefined(typeof(MatchState), state)) return;

        PluginLogger.Info("Match state updated");
        CurrentMatchState = state;
    }

    public void SetGameState(GameState state)
    {
        PluginLogger.Info($"Updating game state to - {state.ToString()}");
        if (!Enum.IsDefined(typeof(GameState), state)) return;

        PluginLogger.Info($"Game state updated to - {state.ToString()}");
        CurrentGameState = state;
    }

    public void SetKnifeRoundWinnerTeam(CsTeam team)
    {
        PluginLogger.Info($"Set knife round winner: {KnifeRoundWinnerTeam.ToString()}");
        KnifeRoundWinnerTeam = team;
    }

    public bool IsWarmup()
    {
        var gameRules = GetGameRules();
        return gameRules!.WarmupPeriod;
    }

    public bool IsFreezePeriod()
    {
        var gameRules = GetGameRules();
        return gameRules!.FreezePeriod;
    }

    public bool IsFriendlyFireOn()
    {
        // TODO: Check friendly fire from cvar and only by nades
        throw new NotImplementedException();
    }

    public CCSGameRules? GetGameRules()
    {
        return Utilities.FindAllEntitiesByDesignerName<CCSGameRulesProxy>("cs_gamerules")
            .First()
            .GameRules;
    }

    public void Reset()
    {
        SetGameState(GameState.Unpaused);
        SetMatchState(MatchState.Sleeping);
        SetKnifeRoundWinnerTeam(CsTeam.None);
    }
}
