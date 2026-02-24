using BasicFaceitServer.GameStates;
using CounterStrikeSharp.API.Core;
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

        var cabin = configs.IpByTeams?.FirstOrDefault(c => c.IpAddresses.Contains(playerIp));
        if (cabin == null)
            return CsTeam.None;

        var defaultTeam = cabin.Side switch
        {
            "CT" => CsTeam.CounterTerrorist,
            "T" => CsTeam.Terrorist,
            "spectator" => CsTeam.Spectator,
            _ => CsTeam.Spectator
        };

        return defaultTeam;
    }
}