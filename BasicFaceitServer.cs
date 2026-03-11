using BasicFaceitServer.Commands;
using BasicFaceitServer.Events;
using CounterStrikeSharp.API.Core;
using BasicFaceitServer.Core;
using BasicFaceitServer.Events.Listeners;
using BasicFaceitServer.Infrastructure;

namespace BasicFaceitServer;

public class BasicFaceitServer : BasePlugin
{
    public override string ModuleName => "Faceit Server Plugin";
    public override string ModuleDescription => "";
    public override string ModuleAuthor => "Akbar Menglimuratov";
    public override string ModuleVersion => "0.0.1";

    public override void Load(bool hotReload)
    {
        PluginLogger.Info("Start plugin load");

        if (hotReload)
        {
            PluginLogger.Warn("The plugin is hotReloaded! This might cause instability to your server");
        }

        PluginContext.Initialize(this, ModuleDirectory);
        
        MatchEventHandler.Register(this);
        RoundEventHandler.Register(this);
        WarmupEventHandler.Register(this);
        PlayerEventHandler.Register(this);
        
        TickListener.Register(this);
        
        MatchCommand.Register(this);
        PlayerCommand.Register(this);

        PluginContext.ExecBaseCfgFile();

        PluginLogger.Info("End plugin load");
    }

    public override void Unload(bool hotReload)
    {
        PluginContext.MatchStateManager.SetMatchState(MatchState.Sleeping);
    }
}
