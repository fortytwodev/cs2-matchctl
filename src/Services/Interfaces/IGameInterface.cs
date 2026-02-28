using BasicFaceitServer.Core;
using CounterStrikeSharp.API.Core;

namespace BasicFaceitServer.Services.Interfaces;

public interface IGameInterface
{
    GameState State { get; }

    public void SetState(GameState state);
    public bool IsWarmup();
    public bool IsPaused();
    public bool IsFriendlyFireOn();
    public bool IsFreezePeriod();
    public void PauseMatch();
    public void UnpauseMatch();
    public CCSGameRules? GetGameRules();
    public void StartRecordingGameDemo();
    public void StopRecordingGameDemo();
}
