using CounterStrikeSharp.API.Core;

namespace BasicFaceitServer.Services.Interfaces;

public interface IChatInterface
{
    public void PrintToChatPlayer(CCSPlayerController player, string message);
    public void PrintToCenterPlayer(CCSPlayerController player, string message, float delay);
    public void PrintToChatAll(string message);
    public void PrintToCenterAll(string message);
}