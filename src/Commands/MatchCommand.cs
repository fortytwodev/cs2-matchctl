using BasicFaceitServer.Core;
using BasicFaceitServer.Services.Interfaces;
using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Commands;

namespace BasicFaceitServer.Commands;

public static class MatchCommand
{
    private static IMatchInterface _matchService = PluginContext.MatchService;

    public static void Register(BasicFaceitServer plugin)
    {
        plugin.AddCommand("css_get_match_state", "Print game phase", OnGetMatchStateCommand);
        plugin.AddCommand("css_set_match_state", "Set game state", OnSetMatchStateCommand);
    }

    private static void OnGetMatchStateCommand(CCSPlayerController? player, CommandInfo commandInfo)
    {
        commandInfo.ReplyToCommand($"Current game state: {_matchService.GetCurrentGameState()}");
    }

    private static void OnSetMatchStateCommand(CCSPlayerController? player, CommandInfo commandInfo)
    {
        var cmdArg = commandInfo.GetArg(1);
        switch (cmdArg)
        {
            case "warmup":
                commandInfo.ReplyToCommand("Pre warmup state set");
                _matchService.StartPreKnifeWarmup();
                break;
            case "knife":
                commandInfo.ReplyToCommand("Knife state set");
                _matchService.StartKnife();
                Server.ExecuteCommand("mp_restartgame 1");
                break;
            case "postwarmup":
                commandInfo.ReplyToCommand("Post warmup state set");
                _matchService.StartPostKnifeWarmup();
                break;
            case "live":
                commandInfo.ReplyToCommand("Live state set");
                _matchService.StartMatch();
                Server.ExecuteCommand("mp_restartgame 1");
                break;
            default:
                commandInfo.ReplyToCommand("Incorrect command");
                return;
        }
    }
}