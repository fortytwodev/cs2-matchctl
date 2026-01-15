using BasicFaceitServer.GameStates;
using BasicFaceitServer.Utils;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Cvars;
using CounterStrikeSharp.API.Modules.Utils;

namespace BasicFaceitServer.Events;

public class GameEvent(BasicFaceitServer core)
{
    private readonly GameController _gameController = core.GameController;
    private readonly GameUtils _gameUtils = core.GameUtils;
    private readonly MyHelper _helper = core.Helper;

    public void Load()
    {
        core.RegisterEventHandler<EventRoundStart>(OnRoundStart);
        core.RegisterEventHandler<EventRoundEnd>(OnRoundEnd);
        core.RegisterEventHandler<EventRoundAnnounceWarmup>(OnRoundAnnounceWarmup);
        core.RegisterEventHandler<EventWarmupEnd>(OnWarmupEnd);
        core.RegisterEventHandler<EventRoundAnnounceMatchStart>(OnRoundAnnounceMatchStart);
        core.RegisterEventHandler<EventCsWinPanelMatch>(OnEventCsWinPanelMatch);
        core.RegisterEventHandler<EventTeamIntroStart>(OnEventTeamIntroStart, HookMode.Pre);
    }

    private HookResult OnEventTeamIntroStart(EventTeamIntroStart @event, GameEventInfo info)
    {
        MyLogger.Info("OnEventTeamIntroStart");
        info.DontBroadcast = true;
        return HookResult.Changed;
    }

    private HookResult OnEventCsWinPanelMatch(EventCsWinPanelMatch @event, GameEventInfo info)
    {
        core.GamePhase = GamePhase.Sleeping;
        core.AddTimer(5.0f, () =>
        {
            _gameController.StopRecordingGameDemo();
        });
        return HookResult.Continue;
    }

    private HookResult OnRoundStart(EventRoundStart @event, GameEventInfo info)
    {
        MyLogger.Info("Start");

        if (_gameUtils.IsMatchLive()) return HookResult.Continue;

        var players = _helper.GetPlayers();

        if (_gameUtils.IsKnife())
        {
            MyLogger.Debug($"Knife round started. Skip team intro");

            // var gameRules = _helper.GetGameRules();
            // gameRules!.TeamIntroPeriod = false;

            foreach (var player in players)
                _helper.PreparePlayerForKnifeRound(player);

            _helper.PrintToChatAll("Pıshaq roundı!");
            _helper.PrintToChatAll("{red}DĺQQAT!!! {green}Friendly fire qosılǵan");
            _helper.PrintToChatAll("{green}Eger oyınshı  bilep-bilmey, pıshaq roundı yamasa janlı oyın (game) waqtında komandalasına zálel jetkerse, oyın qayta baslanbaydı (restart berilmeydi).");
        }
        else if (_gameUtils.IsMatchLive() && players.Count >= core.Config.MinPlayerToStart)
        {
            MyLogger.Debug($"Players ({players.Count}) count is below 10. Pause the match");
            _gameController.PauseMatch();
        }

        MyLogger.Info("Finish");
        return HookResult.Continue;
    }

    private HookResult OnRoundEnd(EventRoundEnd @event, GameEventInfo info)
    {
        MyLogger.Info($"Start");

        if (_gameUtils.IsMatchLive()) return HookResult.Continue;

        if (_gameUtils.IsKnife())
        {
            var knifeWinner = @event.Winner == (byte)CsTeam.CounterTerrorist
                ? CsTeam.CounterTerrorist
                : CsTeam.Terrorist;
            _gameController.SetKnifeWinnerTeam(knifeWinner);
            _gameController.StartPostKnifeWarmup();
        }

        MyLogger.Info($"Finish");
        return HookResult.Continue;
    }

    private HookResult OnRoundAnnounceWarmup(EventRoundAnnounceWarmup @event, GameEventInfo info)
    {
        var gameRules = _helper.GetGameRules();
        if (gameRules != null)
        {
            core.GameListeners.WarmupStartTime = gameRules.WarmupPeriodStart;
            core.GameListeners.WarmupEndTime = gameRules.WarmupPeriodEnd;
            core.GameListeners.WarmupTime = core.Config.PreWarmupTime;
            core.GameListeners.LastMessageTime = DateTime.Now.AddSeconds(-60);
            core.GameListeners.LastWarmupEndValueCheckTime = DateTime.Now;
        }
        
        if (_gameUtils.IsPreWarmup() || !_gameUtils.IsPostWarmup()) return HookResult.Continue;

        MyLogger.Debug($"Post knife warmup period started");

        var teamName1 = ConVar.Find("mp_teamname_1")?.StringValue ?? "Counter-Terrorist";
        var teamName2 = ConVar.Find("mp_teamname_2")?.StringValue ?? "Terrorist";
        MyLogger.Info($"Team name 1 - {teamName1}");
        MyLogger.Info($"Team name 2 - {teamName2}");

        var knifeWinner = _helper.GetKnifeWinnerTeam();
        if (knifeWinner == CsTeam.None)
            return HookResult.Continue;

        var winnerTeamName = knifeWinner == CsTeam.CounterTerrorist
            ? teamName1
            : teamName2;
        MyLogger.Debug($"Winner team name - {winnerTeamName}");

        _helper.PrintToChatAll($"{{white}}Utqan komanda tárepti tańlań");
        _helper.PrintToChatAll("{green}!ct {white}yamasa {green}!t {white}komandasın jazıń");

        MyLogger.Info($"Finish");

        return HookResult.Continue;
    }

    private HookResult OnWarmupEnd(EventWarmupEnd @event, GameEventInfo info)
    {
        MyLogger.Info("Start");

        if (_gameUtils.IsPreWarmup())
        {
            MyLogger.Info("Pre-knife warmup period ended");

            var players = _helper.GetPlayers();
            if (players.Count < core.Config.MinPlayerToStart)
            {
                MyLogger.Debug($"Players ({players.Count}) count is below {core.Config.MinPlayerToStart}");
                _gameController.PauseMatch();
            }

            if (core.Config.KnifeRoundEnabled)
                _gameController.StartKnife();
            else
                _gameController.StartMatch();
            
            return HookResult.Continue;
        }

        if (_gameUtils.IsPostWarmup())
        {
            MyLogger.Info("Post knife warmup period ended");
            _gameController.StartMatch();
        }

        MyLogger.Info("End");
        return HookResult.Continue;
    }

    private HookResult OnRoundAnnounceMatchStart(EventRoundAnnounceMatchStart @event, GameEventInfo info)
    {
        MyLogger.Info("Start");

        if (_gameUtils.IsKnife())
        {
            MyLogger.Info($"Print knife round start message to each player");
            _helper.PrintToCenterAll("Pıshaq roundı baslandı");
            _helper.PrintToChatAll("KNIFE!!!");
            _helper.PrintToChatAll("KNIFE!!!");
            _helper.PrintToChatAll("KNIFE!!!");
        }

        if (_gameUtils.IsMatchLive())
        {
            MyLogger.Info($"Print Good luck message");
            _helper.PrintToChatAll("Hámmege áwmet!!!");
        }

        MyLogger.Info("End");
        return HookResult.Continue;
    }
}