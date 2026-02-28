using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Utils;

namespace BasicFaceitServer.Services.Interfaces;

public interface IPlayerInterface
{
    CsTeam GetPlayerTeam(CCSPlayerController player);
    public void PlayerJoinTeam(CCSPlayerController player, CsTeam playerTeam);
    public void PreparePlayerForKnifeRound(CCSPlayerController player);
    public List<CCSPlayerController> GetPlayersList(CsTeam? includeTeam = null, bool includeSpec = false);
}