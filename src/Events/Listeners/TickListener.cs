using BasicFaceitServer.Configs;
using BasicFaceitServer.Core;
using BasicFaceitServer.Services.Interfaces;
using BasicFaceitServer.States;
using CounterStrikeSharp.API;
using CSSListeners = CounterStrikeSharp.API.Core.Listeners;

namespace BasicFaceitServer.Events.Listeners;

public static class TickListener
{
    
    private static PluginConfig _config = PluginContext.Config;
    private static IChatInterface _chatService = PluginContext.ChatService;
    private static IState _matchState = PluginContext.MatchStateManager;
    
    public static void Register(BasicFaceitServer plugin)
    {
        plugin.RegisterListener<CSSListeners.OnTick>(OnTick);
    }

    private static void OnTick()
    {
        var messageInterval = _config.WarmupMessageIntervalSeconds;
        if (!_matchState.IsPreWarmup) return;

        if ((DateTime.Now - PluginContext.LastWarmupEndValueCheckTime).TotalSeconds >= 1)
        {
            var gameRules = _matchState.GetGameRules();
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