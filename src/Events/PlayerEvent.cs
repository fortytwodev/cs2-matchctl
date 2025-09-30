using BasicFaceitServer.Events.DamageManagement;
using BasicFaceitServer.GameStates;
using BasicFaceitServer.Utils;
using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Capabilities;
using CounterStrikeSharp.API.Modules.Events;
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
    private DamageManagementApi ManagementApi { get; set; }
    private static PluginCapability<IDamageManagementApi> DamageManagementCapability { get; } = new("damagemanagement:api");   

    public void Load()
    {
        core.RegisterEventHandler<EventPlayerConnectFull>(OnPlayerConnectFull);
        core.RegisterEventHandler<EventPlayerDisconnect>(OnPlayerDisconnect);
        core.RegisterEventHandler<EventPlayerTeam>(OnEventPlayerTeam);
        core.RegisterEventHandler<EventPlayerChat>(OnEventPlayerChat);

        ManagementApi = new DamageManagementApi();
        Capabilities.RegisterPluginCapability(DamageManagementCapability, () => ManagementApi);
        VirtualFunctions.CBaseEntity_TakeDamageOldFunc.Hook(OnTakeDamage, HookMode.Pre);

        MyLogger.Info("Player events loaded");
    }

    private HookResult OnEventPlayerChat(EventPlayerChat @event, GameEventInfo info)
    {
        var text = @event.Text;
        var isAll = @event.Teamonly;
        MyLogger.Info($"{isAll}: {text}");
        return HookResult.Continue;
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

        if (_gameUtils.IsMatchLive() || _gameUtils.IsPostWarmup())
            return HookResult.Continue;

        if (_gameUtils.IsKnife())
        {
            _helper.PreparePlayerForKnifeRound(player);
            return HookResult.Handled;
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
        if (@event.Userid is null) return HookResult.Continue;
        
        MyLogger.Info($"Player - {@event.Userid.PlayerName}:{@event.Team}");
        
        var allPlayers = Utilities
            .GetPlayers()
            .Where(p => p is {IsHLTV: false, Team: not (CsTeam.None or CsTeam.Spectator)})
            .ToList();
        MyLogger.Debug($"Players count - {allPlayers.Count}");

        MyLogger.Debug($"Is pre warmup: {_gameUtils.IsPreWarmup()}");
        if (_gameUtils.IsPreWarmup() || allPlayers.Count > 0) return HookResult.Continue;

        MyLogger.Debug($"First player connected - {@event.Userid.PlayerName}:{@event.Team}");
        _gameController.StartPreKnifeWarmup();

        if (!_gameUtils.IsPreWarmup()) return HookResult.Continue;

        _helper.PrintToChatPlayer(@event.Userid, "Oyın aldınan razminka!!!");
        _helper.PrintToChatPlayer(@event.Userid, "RAZMINKA!!!");
        _helper.PrintToChatPlayer(@event.Userid, "RAZMINKA!!!");
        _helper.PrintToChatPlayer(@event.Userid, "RAZMINKA!!!");
        _helper.PrintToCenterPlayer(@event.Userid, "Oyın aldınan razminka", 5.0f);

        return HookResult.Continue;
    }

    private HookResult OnTakeDamage(DynamicHook hook)
    {
        //Check handle for API first
        var executeOriginalMethod = ManagementApi.IsNeedCallOriginalMethod();
        //it will allow consumer determine execute original method or not
        if (executeOriginalMethod)
        {
            return HookResult.Continue;
        }
        
        if (_gameUtils.IsFriendlyFireOn()) return HookResult.Continue;
        
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