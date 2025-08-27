using BasicFaceitServer.GameStates;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Cvars;
using CounterStrikeSharp.API.Modules.Utils;

namespace BasicFaceitServer.Utils;

public class GameUtils(BasicFaceitServer core)
{
    private readonly MyHelper _helper = core.Helper;
    
    public bool IsPreWarmup()
    {
        return core.GamePhase == GamePhase.PreKnifeWarmup;
    }
    
    public bool IsPostWarmup()
    {
        return core.GamePhase == GamePhase.PostKnifeWarmup;
    }
    
    public bool IsKnife()
    {
        return core.GamePhase == GamePhase.Knife;
    }
    
    public bool IsMatchLive()
    {
        return core.GamePhase == GamePhase.MatchLive;
    }
    
    public bool IsSleeping()
    {
        return core.GamePhase == GamePhase.Sleeping;
    }

    public GamePhase GetCurrentGameState()
    {
        return core.GamePhase;
    }

    public bool IsWarmup()
    {
        var gameRules = _helper.GetGameRules();
        return gameRules!.WarmupPeriod;
    }

    public bool IsPaused()
    {
        return core.MatchState == MatchState.Paused;
    }

    public bool IsFreezePeriod()
    {
        var gameRules = _helper.GetGameRules();
        return gameRules!.FreezePeriod;
    }

    public bool CheckValueInCabinsList(string code)
    {
        return core.Config.Cabins.Any(c => c.Name == code);
    }

    public bool IsFriendlyFireOn()
    {
        return core.Config.IsFriendlyFireShotOn;
    }
    
    public CsTeam GetPlayerTeam(CCSPlayerController player)
    {
        MyLogger.Info("Get client team (CT, T or Spectator)");
        var configs = core.Config;
        var playerIp = player.IpAddress?.Split(":")[0];

        if (string.IsNullOrEmpty(playerIp))
            return CsTeam.Spectator;

        var cabin = configs.Cabins.FirstOrDefault(c => c.IpAddresses.Contains(playerIp));
        if (cabin == null)
            return CsTeam.None;
        
        MyLogger.Debug("Player cabin is found");

        var cabin1 = core.TeamCabin1;
        var cabin2 = core.TeamCabin2;
        MyLogger.Debug($"1.{cabin1}, 2.{cabin2} - Player cabin - {cabin.Name}");
            
        var defaultTeam = CsTeam.None;
        if (cabin.Name == cabin1)
            defaultTeam = CsTeam.CounterTerrorist;
        else if (cabin.Name == cabin2)
            defaultTeam = CsTeam.Terrorist;
        else if (cabin.Name == "spec")
            defaultTeam = CsTeam.Spectator;

        return defaultTeam;
    }
}