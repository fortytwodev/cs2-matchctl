using BasicFaceitServer.Core;
using BasicFaceitServer.Services.Interfaces;
using BasicFaceitServer.Infrastructure;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Commands;
using CounterStrikeSharp.API.Modules.Utils;

namespace BasicFaceitServer.Commands;

public class PlayerCommand
{
    private static IMatchInterface _matchService = PluginContext.MatchService;
    private static IGameInterface _gameService = PluginContext.GameService;

    public static void Register(BasicFaceitServer plugin)
    {
        plugin.AddCommand("t", "Switch team to T", OnTCommand);
        plugin.AddCommand("ct", "Switch team to CT", OnCTCommand);
    }

    private static void OnCTCommand(CCSPlayerController? player, CommandInfo commandInfo)
    {
        PluginLogger.Info("On command execute: !ct - Start");

        if (_matchService.IsMatchLive()) return;

        if (player == null || !player.IsValid) return;

        var knifeWinnerTeam = _matchService.GetKnifeWinnerTeam();

        PluginLogger.Debug($"On command execute: !ct - Player team: {player.Team}");
        if (player.Team != knifeWinnerTeam || player.Team == CsTeam.Spectator) return;

        var gameRules = _gameService.GetGameRules();

        gameRules!.SwapTeamsOnRestart = player.Team == CsTeam.Terrorist;
        gameRules.WarmupPeriod = false;

        _matchService.StartMatch();

        PluginLogger.Info($"On command execute: !ct - End");
    }

    private static void OnTCommand(CCSPlayerController? player, CommandInfo commandInfo)
    {
        PluginLogger.Info($"On command execute: !t - Start");

        if (_matchService.IsMatchLive()) return;

        if (player == null || !player.IsValid) return;

        var knifeWinnerTeam = _matchService.GetKnifeWinnerTeam();

        PluginLogger.Debug($"On command execute: !t - Player team: {player.Team}");
        if (player.Team != knifeWinnerTeam || player.Team == CsTeam.Spectator) return;

        var gameRules = _gameService.GetGameRules();

        gameRules!.SwapTeamsOnRestart = player.Team == CsTeam.CounterTerrorist;
        gameRules.WarmupPeriod = false;

        _matchService.StartMatch();

        PluginLogger.Info($"On command execute: !t - End");
    }
}
