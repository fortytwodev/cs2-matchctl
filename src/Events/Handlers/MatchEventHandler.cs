using BasicFaceitServer.Core;
using BasicFaceitServer.Infrastructure;
using BasicFaceitServer.Services.Interfaces;
using BasicFaceitServer.States;
using CounterStrikeSharp.API.Core;
using MatchState = BasicFaceitServer.Core.MatchState;

namespace BasicFaceitServer.Events;

public static class MatchEventHandler
{
    private static IGameInterface _gameService = PluginContext.GameService;
    private static IChatInterface _chatService = PluginContext.ChatService;
    private static IState _matchState = PluginContext.MatchStateManager;

    public static void Register(BasicFaceitServer plugin)
    {
        plugin.RegisterEventHandler<EventCsWinPanelMatch>(OnEventCsWinPanelMatch);
        plugin.RegisterEventHandler<EventRoundAnnounceMatchStart>(OnRoundAnnounceMatchStart);
        plugin.RegisterEventHandler<EventTeamIntroStart>(OnEventTeamIntroStart, HookMode.Pre);
    }

    private static HookResult OnEventTeamIntroStart(EventTeamIntroStart @event, GameEventInfo info)
    {
        PluginLogger.Info("OnEventTeamIntroStart");
        info.DontBroadcast = true;
        return HookResult.Changed;
    }

    private static HookResult OnEventCsWinPanelMatch(EventCsWinPanelMatch @event, GameEventInfo info)
    {
        _matchState.SetMatchState(MatchState.Sleeping);
        _gameService.StopRecordingGameDemo();
        return HookResult.Continue;
    }
    
    private static HookResult OnRoundAnnounceMatchStart(EventRoundAnnounceMatchStart @event, GameEventInfo info)
    {
        PluginLogger.Info("Start");

        if (_matchState.IsKnifeRound)
        {
            PluginLogger.Info($"Print knife round start message to each player");
            _chatService.PrintToCenterAll("Pıshaq roundı baslandı");
            _chatService.PrintToChatAll("KNIFE!!!");
            _chatService.PrintToChatAll("KNIFE!!!");
            _chatService.PrintToChatAll("KNIFE!!!");
        }

        if (_matchState.IsLiveMatch)
        {
            PluginLogger.Info($"Print Good luck message");
            _chatService.PrintToChatAll("Hámmege áwmet!!!");
        }

        PluginLogger.Info("End");
        return HookResult.Continue;
    }
}
