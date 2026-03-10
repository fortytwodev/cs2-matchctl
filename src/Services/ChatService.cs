using System.Text.RegularExpressions;
using BasicFaceitServer.Configs;
using BasicFaceitServer.Services.Interfaces;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Utils;

namespace BasicFaceitServer.Services;

public class ChatService : IChatInterface
{
    private readonly BasicFaceitServer _plugin;
    private readonly PluginConfig _config;
    private readonly IPlayerInterface _playerService;

    public ChatService(BasicFaceitServer plugin, PluginConfig config, IPlayerInterface playerService)
    {
        _plugin = plugin;
        _config = config;
        _playerService = playerService;
    }

    public void PrintToChatPlayer(CCSPlayerController player, string message)
    {
        if (player.Team is CsTeam.Spectator or CsTeam.None)
            return;

        var coloredText = $"{{green}}[{_config.Host}]{{white}}: {message}";
        player.PrintToChat(GetColoredText(coloredText));
    }

    public void PrintToCenterPlayer(CCSPlayerController player, string message, float delay)
    {
        if (player.Team is CsTeam.Spectator or CsTeam.None)
            return;

        if (delay > 0.0f)
            _plugin.AddTimer(delay, () => player.PrintToCenter(message));
        else
            player.PrintToCenter(message);
    }
    
    public void PrintToChatAll(string message)
    {
        var coloredText = $"{{green}}[{_config.Host}]{{white}}: {message}";
        var players = _playerService.GetPlayersList();
        foreach (var player in players)
            player.PrintToChat(GetColoredText(coloredText));
    }

    public void PrintToCenterAll(string message)
    {
        var players = _playerService.GetPlayersList();
        foreach (var player in players)
            player.PrintToCenter(message);
    }
    
    // TODO: Make or move to other class
    private static readonly Dictionary<string, char> ColorMap = new()
    {
        { "default", (char)1 },
        { "white", (char)1 },
        { "darkred", (char)2 },
        { "purple", (char)3 },
        { "green", (char)3 },
        { "lightgreen", (char)5 },
        { "slimegreen", (char)6 },
        { "red", (char)7 },
        { "grey", (char)8 },
        { "yellow", (char)9 },
        { "invisible", (char)10 },
        { "lightblue", (char)11 },
        { "blue", (char)12 },
        { "lightpurple", (char)13 },
        { "pink", (char)14 },
        { "fadedred", (char)15 },
        { "gold", (char)16 }
    };

    private static string GetColoredText(string message)
    {
        const string pattern = "{(\\w+)}";
        var replaced = Regex.Replace(message, pattern, match =>
        {
            var colorCode = match.Groups[1].Value;
            return ColorMap.TryGetValue("{" + colorCode + "}", out var replacement)
                ? Convert.ToChar(replacement).ToString()
                : match.Value;
        });

        return $"\u200B{replaced}";
    }
}
