using BasicFaceitServer.GameStates;
using BasicFaceitServer.Utils;
using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Events;
using CounterStrikeSharp.API.Modules.Utils;

namespace BasicFaceitServer.Events;

public class PlayerEvent(BasicFaceitServer core)
{
    private readonly MyHelper _helper = core.Helper;
    private readonly GameController _gameController = core.GameController;
    private readonly GameUtils _gameUtils = core.GameUtils;

    public void Load()
    {
        core.RegisterEventHandler<EventPlayerConnectFull>(OnPlayerConnectFull);
        core.RegisterEventHandler<EventPlayerDisconnect>(OnPlayerDisconnect);
        core.RegisterEventHandler<EventPlayerTeam>(OnEventPlayerTeam);
        core.RegisterEventHandler<EventPlayerChat>(OnEventPlayerChat);

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

        var team = _gameUtils.GetPlayerTeam(player);
        if (player.Team is CsTeam.Spectator or CsTeam.None)
        {
            MyLogger.Debug($"Player connecting first time. Assign team");
            _gameController.PlayerJoinTeam(player, team);
        }

        var allPlayers = Utilities
            .GetPlayers()
            .Where(p => !p.IsHLTV && p.Team != CsTeam.Spectator && p.Team != CsTeam.None)
            .ToList();
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
}