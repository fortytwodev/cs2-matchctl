using BasicFaceitServer.Events.DamageManagement;
using BasicFaceitServer.GameStates;
using BasicFaceitServer.Utils;
using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Capabilities;
using CounterStrikeSharp.API.Modules.Memory;
using CounterStrikeSharp.API.Modules.Memory.DynamicFunctions;
using CounterStrikeSharp.API.Modules.Utils;

namespace BasicFaceitServer.Events;

public class PlayerEvent(BasicFaceitServer core)
{
    private readonly MyHelper _helper = core.Helper;
    private readonly GameController _gameController = core.GameController;
    private readonly GameUtils _gameUtils = core.GameUtils;
    
    //Shared API
    private DamageManagementApi managementAPI { get; set; }
    private static PluginCapability<IDamageManagementApi> DamageManagementCapability { get; } = new("damagemanagement:api");   

    public void Load()
    {
        core.RegisterEventHandler<EventPlayerConnectFull>(OnPlayerConnectFull);
        core.RegisterEventHandler<EventPlayerDisconnect>(OnPlayerDisconnect);
        core.RegisterEventHandler<EventPlayerTeam>(OnEventPlayerTeam, HookMode.Pre);

        managementAPI = new DamageManagementApi();
        Capabilities.RegisterPluginCapability(DamageManagementCapability, () => managementAPI);
        VirtualFunctions.CBaseEntity_TakeDamageOldFunc.Hook(OnTakeDamage, HookMode.Pre);

        MyLogger.Info("Player events loaded");
    }
    
    public void Unload()
    {
        VirtualFunctions.CBaseEntity_TakeDamageOldFunc.Unhook(OnTakeDamage, HookMode.Pre);
    }

    private HookResult OnPlayerConnectFull(EventPlayerConnectFull @event, GameEventInfo info)
    {
        MyLogger.Info($"Start");

        var player = @event.Userid;
        if (player is null || !player.IsValid || player.IsBot || player.IpAddress is null)
        {
            MyLogger.Debug($"Player is null or bot - {player?.IpAddress}");
            return HookResult.Continue;
        }

        var isParticipant = _helper.CheckIpInParticipantsList(player.IpAddress);
        MyLogger.Debug($"Is player participant {isParticipant} - {player.IpAddress}.");
        if (!isParticipant)
        {
            MyLogger.Debug($"Player is not participant - {player.IpAddress}. Kicking");
            _gameController.KickPlayer(player);
            return HookResult.Continue;
        }

        var team = _gameUtils.GetPlayerTeam(player);
        if (team is CsTeam.None)
        {
            MyLogger.Debug($"Player is not assigned to team - {player.IpAddress}. Kick player");
            _gameController.KickPlayer(player);
            return HookResult.Continue;
        }
        
        if (player.Team is CsTeam.Spectator or CsTeam.None)
        {
            MyLogger.Debug($"Player connecting first time. Assign team");
            _gameController.PlayerJoinTeam(player, team);
        }

        var allPlayers = Utilities.GetPlayers();
        if (_gameUtils.IsPaused())
        {
            if (allPlayers.Count >= core.Config.MinPlayerToStart)
                _gameController.UnpauseMatch();
        }

        if (_gameUtils.IsMatchLive() || _gameUtils.IsPostWarmup())
            return HookResult.Continue;

        if (_gameUtils.IsKnife())
        {
            _helper.PreparePlayerForKnifeRound(player);
            return HookResult.Handled;
        }

        if (!_gameUtils.IsPreWarmup() && allPlayers.Count == 1)
        {
            MyLogger.Debug($"First player connected - {player.IpAddress}");
            _gameController.StartPreKnifeWarmup();
        }

        if (_gameUtils.IsPreWarmup())
        {
            _helper.PrintToChat(player, "Oyın aldınan razminka!!!");
            _helper.PrintToChat(player, "RAZMINKA!!!");
            _helper.PrintToChat(player, "RAZMINKA!!!");
            _helper.PrintToChat(player, "RAZMINKA!!!");
            _helper.PrintToCenter(player, "Oyın aldınan razminka", 5.0f);

            return HookResult.Continue;
        }

        MyLogger.Info($"Finish");
        return HookResult.Continue;
    }

    private HookResult OnPlayerDisconnect(EventPlayerDisconnect @event, GameEventInfo info)
    {
        MyLogger.Info($"Start");

        var player = @event.Userid;
        if (player == null || !player.IsValid || player.IsBot)
        {
            MyLogger.Debug($"Player is null or bot - {player?.IpAddress}");
            return HookResult.Continue;
        }

        if (_gameUtils.IsKnife() || _gameUtils.IsMatchLive())
        {
            if (!new[] { CsTeam.Spectator, CsTeam.None }.Contains(player.Team))
            {
                MyLogger.Debug($"Player disconnected. Match will be paused - {player.IpAddress}");
                _gameController.PauseMatch();
            }
        }

        MyLogger.Info($"Player disconnected- {player.IpAddress}");
        MyLogger.Info($"Finish");
        return HookResult.Continue;
    }

    private HookResult OnEventPlayerTeam(EventPlayerTeam @event, GameEventInfo info)
    {
        if (@event.Team == (byte)CsTeam.Spectator)
        {
            info.DontBroadcast = true;
            MyLogger.Debug($"Player team is spectator");
            return HookResult.Changed;
        }

        MyLogger.Info($"Player team is not spectator");
        return HookResult.Changed;
    }

    private HookResult OnTakeDamage(DynamicHook hook)
    {
        //Check handle for API first
        var executeOriginalMethod = managementAPI.IsNeedCallOriginalMethod();
        //it will allow consumer determine execute original method or not
        if (executeOriginalMethod)
        {
            return HookResult.Continue;
        }
        
        if (_gameUtils.IsFriendlyFireShotOn()) return HookResult.Continue;
        
        try
        {
            var victim = hook.GetParam<CEntityInstance>(0);
            var damageInfo = hook.GetParam<CTakeDamageInfo>(1);
    
            if (damageInfo.Attacker.Value == null) return HookResult.Continue;
    
            var inflicter = damageInfo.Inflictor.Value?.DesignerName ?? "";
            var attackPlayer = new CCSPlayerPawn(damageInfo.Attacker.Value.Handle);
            var playerTakenDmg = new CCSPlayerController(victim.Handle);
    
            if (attackPlayer.TeamNum != playerTakenDmg.TeamNum || !"player".Equals(victim.DesignerName))
                return HookResult.Continue; 
            
            string[] enableDmgInflicter =
            [
                "inferno", "hegrenade_projectile", "flashbang_projectile", "smokegrenade_projectile",
                "decoy_projectile", "planted_c4"
            ];
            return enableDmgInflicter.Contains(inflicter) ? HookResult.Continue : HookResult.Handled;
        }
        catch (Exception ex)
        {
            MyLogger.Error($"Error while shooting to player (OnTakeDamage) - {ex}");
        }
    
        return HookResult.Continue;
    }
}