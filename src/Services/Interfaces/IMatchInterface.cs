using BasicFaceitServer.Core;
using CounterStrikeSharp.API.Modules.Utils;

namespace BasicFaceitServer.Services.Interfaces;

public interface IMatchInterface
{   
    // TODO: Make another class to control the states
    MatchState State { get; }

    public void SetState(MatchState state);
    public bool IsPreWarmup();
    public bool IsPostWarmup();
    public bool IsKnife();
    public bool IsMatchLive();
    public bool IsSleeping();
    public MatchState GetCurrentGameState();
    public void StartPreKnifeWarmup();
    public void StartPostKnifeWarmup();
    public void StartKnife();
    public void StartMatch();

    // TODO: Make another class for it
    public void SetKnifeWinnerTeam(CsTeam team);
    public CsTeam GetKnifeWinnerTeam();
}