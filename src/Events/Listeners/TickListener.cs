using BasicFaceitServer.Configs;
using BasicFaceitServer.Core;
using BasicFaceitServer.Services.Interfaces;
using CounterStrikeSharp.API;
using CSSListeners = CounterStrikeSharp.API.Core.Listeners;

namespace BasicFaceitServer.Events.Listeners;

public static class TickListener
{
    
    private static PluginConfig _config = PluginContext.Config;
    private static IMatchInterface _matchService = PluginContext.MatchService;
    private static IGameInterface _gameService = PluginContext.GameService;
    private static IChatInterface _chatService = PluginContext.ChatService;
    
    public static void Register(BasicFaceitServer plugin)
    {
        plugin.RegisterListener<CSSListeners.OnTick>(OnTick);
    }

    private static void OnTick()
    {
        var messageInterval = _config.WarmupMessageIntervalSeconds;
        if (!_matchService.IsPreWarmup()) return;

        if ((DateTime.Now - PluginContext.LastWarmupEndValueCheckTime).TotalSeconds >= 1)
        {
            var gameRules = _gameService.GetGameRules();
            if (gameRules != null)
            {
                if ((int) PluginContext.WarmupEndTime != (int) gameRules.WarmupPeriodEnd)
                {
                    PluginContext.WarmupEndTime = gameRules.WarmupPeriodEnd;
                    PluginContext.LastMessageTime = DateTime.Now.AddSeconds(-(messageInterval - ((PluginContext.WarmupTime - (int) Server.CurrentTime) % messageInterval) * messageInterval));
                }
            }
            PluginContext.LastWarmupEndValueCheckTime = DateTime.Now;
        }
            
        var elapsedTime = DateTime.Now - PluginContext.LastMessageTime;
        if (!(elapsedTime.TotalSeconds >= messageInterval)) return;
        
        var leftTime = (int)PluginContext.WarmupEndTime - (int)Server.CurrentTime;
        if (leftTime % messageInterval != 0 || leftTime / messageInterval <= 0) return;
        
        var minutes = leftTime / _config.WarmupMessageIntervalSeconds;
        
        // TODO: add localization
        _chatService.PrintToChatAll($"{{green}}Oyın baslanıwına {minutes} minut qaldı");
        PluginContext.LastMessageTime = DateTime.Now;
    }
}