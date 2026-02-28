using BasicFaceitServer.Configs;
using BasicFaceitServer.Core;
using BasicFaceitServer.Services.Interfaces;
using BasicFaceitServer.Infrastructure;
using BasicFaceitServer.States;
using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Events;
using CounterStrikeSharp.API.Modules.Utils;

namespace BasicFaceitServer.Events;

public static class PlayerEventHandler
{
    private static IPlayerInterface _playerService = PluginContext.PlayerService;
    private static PluginConfig _config = PluginContext.Config;
    private static IGameInterface _gameService = PluginContext.GameService;
    private static IMatchInterface _matchService = PluginContext.MatchService;
    private static IChatInterface _chatService = PluginContext.ChatService;
    private static IState _matchState = PluginContext.MatchStateManager;

    public static void Register(BasicFaceitServer plugin)
    {
        plugin.RegisterEventHandler<EventPlayerConnectFull>(OnPlayerConnectFull);
        plugin.RegisterEventHandler<EventPlayerDisconnect>(OnPlayerDisconnect);
        plugin.RegisterEventHandler<EventPlayerTeam>(OnEventPlayerTeam);
        plugin.RegisterEventHandler<EventPlayerChat>(OnEventPlayerChat);

        PluginLogger.Info("Player events are loaded");
    }

    private static HookResult OnEventPlayerChat(EventPlayerChat @event, GameEventInfo info)
    {
        var text = @event.Text;
        var isAll = @event.Teamonly;
        PluginLogger.Info($"{isAll}: {text}");
        return HookResult.Continue;
    }

    private static HookResult OnPlayerConnectFull(EventPlayerConnectFull @event, GameEventInfo info)
    {
        PluginLogger.Info($"Start");

        var player = @event.Userid;
        if (player is null || !player.IsValid || player.IsBot || player.IpAddress is null)
        {
            PluginLogger.Debug($"Player is null or bot - {player?.IpAddress}");
            return HookResult.Continue;
        }

        if (_config.AutoJoinTeam)
        {
            var team = _playerService.GetPlayerTeam(player);
            if (player.Team is CsTeam.Spectator or CsTeam.None)
            {
                PluginLogger.Debug($"Player connecting first time. Assign team");
                _playerService.PlayerJoinTeam(player, team);
            }
        }

        var allPlayers = Utilities
            .GetPlayers()
            .Where(p => !p.IsHLTV && p.Team != CsTeam.Spectator && p.Team != CsTeam.None)
            .ToList();
        if (_matchState.IsPaused)
        {
            if (allPlayers.Count >= _config.MinPlayerToStart)
                _gameService.UnpauseMatch();
        }

        if (_matchState.IsLiveMatch || _matchState.IsPostWarmup)
            return HookResult.Continue;

        if (_matchState.IsKnifeRound)
        {
            _playerService.PreparePlayerForKnifeRound(player);
            return HookResult.Handled;
        }

        PluginLogger.Info($"Finish");
        return HookResult.Continue;
    }

    private static HookResult OnPlayerDisconnect(EventPlayerDisconnect @event, GameEventInfo info)
    {
        PluginLogger.Info($"Start");

        var player = @event.Userid;
        if (player == null || !player.IsValid || player.IsBot)
        {
            PluginLogger.Debug($"Player is null or bot - {player?.IpAddress}");
            return HookResult.Continue;
        }

        if (_matchState.IsKnifeRound || _matchState.IsLiveMatch)
        {
            if (!new[] { CsTeam.Spectator, CsTeam.None }.Contains(player.Team))
            {
                PluginLogger.Debug($"Player disconnected. Match will be paused - {player.IpAddress}");
                _gameService.PauseMatch();
            }
        }

        PluginLogger.Info($"Player disconnected- {player.IpAddress}");
        PluginLogger.Info($"Finish");
        return HookResult.Continue;
    }

    private static HookResult OnEventPlayerTeam(EventPlayerTeam @event, GameEventInfo info)
    {
        if (@event.Userid is null) return HookResult.Continue;
        
        PluginLogger.Info($"Player - {@event.Userid.PlayerName}:{@event.Team}");
        
        var allPlayers = Utilities
            .GetPlayers()
            .Where(p => p is {IsHLTV: false, Team: not (CsTeam.None or CsTeam.Spectator)})
            .ToList();
        PluginLogger.Debug($"Players count - {allPlayers.Count}");

        PluginLogger.Debug($"Is pre warmup: {_matchState.IsPreWarmup}");
        if (_matchState.IsPreWarmup || allPlayers.Count > 0) return HookResult.Continue;

        PluginLogger.Debug($"First player connected - {@event.Userid.PlayerName}:{@event.Team}");
        _matchService.StartPreKnifeWarmup();

        if (!_matchState.IsPreWarmup) return HookResult.Continue;

        _chatService.PrintToChatPlayer(@event.Userid, "Oyın aldınan razminka!!!");
        _chatService.PrintToChatPlayer(@event.Userid, "RAZMINKA!!!");
        _chatService.PrintToChatPlayer(@event.Userid, "RAZMINKA!!!");
        _chatService.PrintToChatPlayer(@event.Userid, "RAZMINKA!!!");
        _chatService.PrintToCenterPlayer(@event.Userid, "Oyın aldınan razminka", 5.0f);

        return HookResult.Continue;
    }
}
