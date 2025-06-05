using BasicFaceitServer.GameStates;
using BasicFaceitServer.Utils;
using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Commands;
using CounterStrikeSharp.API.Modules.Utils;

namespace BasicFaceitServer.Commands;

public class MyCommands(BasicFaceitServer core)
{
    private readonly GameController _gameController = core.GameController;
    private readonly GameUtils _game = core.GameUtils;
    private readonly MyHelper _helper = core.Helper;
    
    public void Load()
    {
        core.AddCommand("t", "Switch team to T", OnTCommand);
        core.AddCommand("ct", "Switch team to CT", OnCTCommand);
        core.AddCommand("css_set_gp", "Set game state", OnSetGamePhaseCommand);
        core.AddCommand("css_get_gp", "Print game phase", OnGetGamePhaseCommand);
        core.AddCommand("css_get_gr", "Print game rules", OnPrintGameRulesCommand);
    }

    [CommandHelper(whoCanExecute: CommandUsage.SERVER_ONLY)]
    private void OnPrintGameRulesCommand(CCSPlayerController? player, CommandInfo commandInfo)
    {
        var gameRules = _helper.GetGameRules();
        MyLogger.Debug($"GamePhase: {gameRules!.GamePhase}");
        MyLogger.Debug($"Current game state: {_game.GetCurrentGameState()}");
        MyLogger.Debug($"WarmupPeriodStart: {gameRules.WarmupPeriodStart}");
        MyLogger.Debug($"WarmupPeriodEnd: {gameRules.WarmupPeriodEnd}");
        MyLogger.Debug($"Server Tick time: {Server.TickedTime}");
        MyLogger.Debug($"Server Engine time: {Server.EngineTime}");
        MyLogger.Debug($"Server Current time: {Server.CurrentTime}");
    }
    
    private void OnCTCommand(CCSPlayerController? player, CommandInfo command)
    {
        MyLogger.Info("On command execute: !ct - Start");

        if (_game.IsMatchLive()) return;

        if (player == null || !player.IsValid) return;

        var knifeWinnerTeam = _helper.GetKnifeWinnerTeam();
        
        MyLogger.Debug($"On command execute: !ct - Player team: {player.Team}");
        if (player.Team != knifeWinnerTeam || player.Team == CsTeam.Spectator) return;
        
        var gameRules = _helper.GetGameRules();
        
        gameRules!.SwapTeamsOnRestart = player.Team == CsTeam.Terrorist;
        gameRules.WarmupPeriod = false;
        
        _gameController.StartMatch();
        
        MyLogger.Info($"On command execute: !ct - End");
    }

    private void OnTCommand(CCSPlayerController? player, CommandInfo command)
    {
        MyLogger.Info($"On command execute: !t - Start");
        
        if (_game.IsMatchLive()) return;

        if (player == null || !player.IsValid) return;

        var knifeWinnerTeam = _helper.GetKnifeWinnerTeam();
        
        MyLogger.Debug($"On command execute: !t - Player team: {player.Team}");
        if (player.Team != knifeWinnerTeam || player.Team == CsTeam.Spectator) return;
        
        var gameRules = _helper.GetGameRules();
        
        gameRules!.SwapTeamsOnRestart = player.Team == CsTeam.CounterTerrorist;
        gameRules.WarmupPeriod = false;
        
        _gameController.StartMatch();
        
        MyLogger.Info($"On command execute: !t - End");
    }

    private void OnGetGamePhaseCommand(CCSPlayerController? player, CommandInfo command)
    {
        command.ReplyToCommand($"Current game state: {_game.GetCurrentGameState()}");
    }
    
    private void OnSetGamePhaseCommand(CCSPlayerController? player, CommandInfo command)
    {
        var cmdArg = command.GetArg(1);
        switch (cmdArg)
        {
            case "prewarmup":
                command.ReplyToCommand("Pre warmup state set");
                _gameController.StartPreKnifeWarmup();
                break;
            case "knife":
                command.ReplyToCommand("Knife state set");
                _gameController.StartKnife();
                Server.ExecuteCommand("mp_restartgame 1");
                break;
            case "postwarmup":
                command.ReplyToCommand("Post warmup state set");
                _gameController.StartPostKnifeWarmup();
                break;
            case "live":
                command.ReplyToCommand("Live state set");
                _gameController.StartMatch();
                Server.ExecuteCommand("mp_restartgame 1");
                break;
            default:
                command.ReplyToCommand("Incorrect command");
                Console.WriteLine("Incorrect state");
                return;
        }
    }
}