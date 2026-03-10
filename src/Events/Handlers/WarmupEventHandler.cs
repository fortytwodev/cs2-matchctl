using BasicFaceitServer.Configs;
using BasicFaceitServer.Core;
using BasicFaceitServer.Services.Interfaces;
using BasicFaceitServer.Infrastructure;
using BasicFaceitServer.States;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Cvars;
using CounterStrikeSharp.API.Modules.Utils;

namespace BasicFaceitServer.Events;

public static class WarmupEventHandler
{
    private static PluginConfig _config = PluginContext.Config;
    private static IGameInterface _gameService = PluginContext.GameService;
    private static IMatchInterface _matchService = PluginContext.MatchService;
    private static IPlayerInterface _playerService = PluginContext.PlayerService;
    private static IChatInterface _chatService = PluginContext.ChatService;
    private static IState _matchState = PluginContext.MatchStateManager;

    public static void Register(BasicFaceitServer plugin)
    {
        plugin.RegisterEventHandler<EventRoundAnnounceWarmup>(OnRoundAnnounceWarmup);
        plugin.RegisterEventHandler<EventWarmupEnd>(OnWarmupEnd);
    }

    private static HookResult OnRoundAnnounceWarmup(EventRoundAnnounceWarmup @event, GameEventInfo info)
    {
        var gameRules = _matchState.GetGameRules();
        if (gameRules != null)
        {
            PluginContext.SetWarmupTimes(
                gameRules.WarmupPeriodStart,
                gameRules.WarmupPeriodEnd,
                _config.WarmupMessageIntervalSeconds);
        }
        
        if (_matchState.IsPreWarmup || !_matchState.IsPostWarmup) return HookResult.Continue;

        PluginLogger.Debug($"Post knife warmup period started");

        var teamName1 = ConVar.Find("mp_teamname_1")?.StringValue ?? "Counter-Terrorist";
        var teamName2 = ConVar.Find("mp_teamname_2")?.StringValue ?? "Terrorist";
        PluginLogger.Info($"Team name 1 - {teamName1}");
        PluginLogger.Info($"Team name 2 - {teamName2}");

        var knifeWinner = _matchState.GetKnifeRoundWinnerTeam;
        if (knifeWinner == CsTeam.None)
            return HookResult.Continue;

        var winnerTeamName = knifeWinner == CsTeam.CounterTerrorist
            ? teamName1
            : teamName2;
        PluginLogger.Debug($"Winner team name - {winnerTeamName}");

        _chatService.PrintToChatAll($"{{white}}Utqan komanda tárepti tańlań");
        _chatService.PrintToChatAll("{green}!ct {white}yamasa {green}!t {white}komandasın jazıń");

        PluginLogger.Info($"Finish");

        return HookResult.Continue;
    }

    private static HookResult OnWarmupEnd(EventWarmupEnd @event, GameEventInfo info)
    {
        PluginLogger.Info("Start");

        if (_matchState.IsPreWarmup)
        {
            PluginLogger.Info("Pre-knife warmup period ended");

            var players = _playerService.GetPlayersList();
            if (players.Count < _config.MinPlayerToStart)
            {
                PluginLogger.Debug($"Players ({players.Count}) count is below {_config.MinPlayerToStart}");
                _gameService.PauseMatch();
            }

            if (_config.KnifeRoundEnabled)
                _matchService.StartKnife();
            else
                _matchService.StartMatch();
            
            return HookResult.Continue;
        }

        if (_matchState.IsPostWarmup)
        {
            PluginLogger.Info("Post knife warmup period ended");
            _matchService.StartMatch();
        }

        PluginLogger.Info("End");
        return HookResult.Continue;
    }
}
