using BasicFaceitServer.Core;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Utils;

namespace BasicFaceitServer.States;

public interface IState
{
    MatchState CurrentMatchState { get; }
    GameState CurrentGameState { get; }
    CsTeam KnifeRoundWinnerTeam { get; }

    bool IsSleeping { get; }
    bool IsPreWarmup { get; }
    bool IsPostWarmup { get; }
    bool IsKnifeRound { get; }
    bool IsLiveMatch { get; }
    bool IsPaused { get; }
    GameState GetCurrentGameState { get; }
    MatchState GetCurrentMatchState { get; }

    void SetMatchState(MatchState state);
    void SetGameState(GameState state);
    void SetKnifeRoundWinnerTeam(CsTeam team);
    CsTeam GetKnifeRoundWinnerTeam { get; }
    bool IsWarmup();
    bool IsFreezePeriod();
    bool IsFriendlyFireOn();
    CCSGameRules? GetGameRules();
    void Reset();
}
