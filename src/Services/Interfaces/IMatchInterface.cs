using BasicFaceitServer.Core;
using CounterStrikeSharp.API.Modules.Utils;

namespace BasicFaceitServer.Services.Interfaces;

public interface IMatchInterface
{
    public void StartPreKnifeWarmup();
    public void StartPostKnifeWarmup();
    public void StartKnife();
    public void StartMatch();
}