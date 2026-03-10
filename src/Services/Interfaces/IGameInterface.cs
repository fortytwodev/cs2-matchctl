using BasicFaceitServer.Core;
using CounterStrikeSharp.API.Core;

namespace BasicFaceitServer.Services.Interfaces;

public interface IGameInterface
{
    public void PauseMatch();
    public void UnpauseMatch();
    public void StartRecordingGameDemo();
    public void StopRecordingGameDemo();
}
