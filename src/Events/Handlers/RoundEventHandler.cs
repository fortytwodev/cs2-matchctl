using BasicFaceitServer.Configs;
using BasicFaceitServer.Core;
using BasicFaceitServer.Services.Interfaces;
using BasicFaceitServer.Infrastructure;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Utils;

namespace BasicFaceitServer.Events;

public static class RoundEventHandler
{
    private static PluginConfig _config = PluginContext.Config;
    private static IGameInterface _gameService = PluginContext.GameService;
    private static IMatchInterface _matchService = PluginContext.MatchService;
    private static IPlayerInterface _playerService = PluginContext.PlayerService;
    private static IChatInterface _chatService = PluginContext.ChatService;
    
    public static void Register(BasicFaceitServer plugin)
    {
        plugin.RegisterEventHandler<EventRoundStart>(OnRoundStart);
        plugin.RegisterEventHandler<EventRoundEnd>(OnRoundEnd);
    }
    
     private static HookResult OnRoundStart(EventRoundStart @event, GameEventInfo info)
    {
        PluginLogger.Info("Start");

        if (_matchService.IsMatchLive()) return HookResult.Continue;

        var players = _playerService.GetPlayersList();

        if (_matchService.IsKnife())
        {
            PluginLogger.Debug($"Knife round started. Skip team intro");

            // var gameRules = _helper.GetGameRules();
            // gameRules!.TeamIntroPeriod = false;

            foreach (var player in players)
                _playerService.PreparePlayerForKnifeRound(player);

            // TODO: Localization
            _chatService.PrintToChatAll("Pıshaq roundı!");
            _chatService.PrintToChatAll("{red}DĺQQAT!!! {green}Friendly fire qosılǵan");
            _chatService.PrintToChatAll("{green}Eger oyınshı  bilep-bilmey, pıshaq roundı yamasa janlı oyın (game) waqtında komandalasına zálel jetkerse, oyın qayta baslanbaydı (restart berilmeydi).");
        }
        else if (_matchService.IsMatchLive() && players.Count >= _config.MinPlayerToStart)
        {
            PluginLogger.Debug($"Players ({players.Count}) count is below 10. Pause the match");
            _gameService.PauseMatch();
        }

        PluginLogger.Info("Finish");
        return HookResult.Continue;
    }

    private static HookResult OnRoundEnd(EventRoundEnd @event, GameEventInfo info)
    {
        PluginLogger.Info($"Start");

        if (_matchService.IsMatchLive()) return HookResult.Continue;

        if (_matchService.IsKnife())
        {
            var knifeWinner = @event.Winner == (byte) CsTeam.CounterTerrorist
                ? CsTeam.CounterTerrorist
                : CsTeam.Terrorist;
            _matchService.SetKnifeWinnerTeam(knifeWinner);
            _matchService.StartPostKnifeWarmup();
        }

        PluginLogger.Info($"Finish");
        return HookResult.Continue;
    }

    
}