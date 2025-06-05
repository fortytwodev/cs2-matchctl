using BasicFaceitServer.Commands;
using BasicFaceitServer.Config;
using BasicFaceitServer.Configs;
using BasicFaceitServer.Events;
using BasicFaceitServer.GameStates;
using BasicFaceitServer.Utils;
using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Cvars;

namespace BasicFaceitServer;

public class BasicFaceitServer : BasePlugin
{
    public override string ModuleName => "Faceit Server Plugin";
    public override string ModuleDescription => "";
    public override string ModuleAuthor => "Akbar Menglimuratov";
    public override string ModuleVersion => "0.0.1";

    public readonly PlayerEvent PlayerEvents;
    public readonly GameEvent GameEvents;
    public readonly MyHelper Helper;
    public readonly GameController GameController;
    public readonly MyCommands Commands;
    public readonly GameListener GameListeners;
    public readonly GameUtils GameUtils;

    public GamePhase GamePhase = GamePhase.Sleeping;
    public MatchState MatchState = MatchState.Live;

    public string TeamCabin1 = "";
    public string TeamCabin2 = "";
    public string[]? Maps = [];

    private readonly ConfigManager _configManager;
    public MyConfigs Config { get; private set; } = new();

    public bool MatchBeingPlayedIn = false;

    public FakeConVar<string> TeamCabinName1 = new("css_teamcabin_1", "First team cabin name", "Empty",
        ConVarFlags.FCVAR_SERVER_CAN_EXECUTE);

    public FakeConVar<string> TeamCabinName2 = new("css_teamcabin_2", "Second team cabin name", "Empty",
        ConVarFlags.FCVAR_SERVER_CAN_EXECUTE);

    public BasicFaceitServer()
    {
        _configManager = new ConfigManager(this);
        Helper = new MyHelper(this);
        GameUtils = new GameUtils(this);
        GameController = new GameController(this);
        PlayerEvents = new PlayerEvent(this);
        GameEvents = new GameEvent(this);
        Commands = new MyCommands(this);
        GameListeners = new GameListener(this);
    }

    public override void Load(bool hotReload)
    {
        MyLogger.Info("Start plugin load");

        if (hotReload)
        {
            MyLogger.Warn("The plugin is hotReloaded! This might cause instability to your server");
        }

        Config = _configManager.GetConfig(ModuleDirectory);
        _configManager.ValidateConfigs();

        PlayerEvents.Load();
        GameEvents.Load();
        GameController.Load();
        Commands.Load();
        GameListeners.Load();

        TeamCabinName1.ValueChanged += (sender, value) => { TeamCabin1 = value; };

        TeamCabinName2.ValueChanged += (sender, value) => { TeamCabin2 = value; };

        RegisterFakeConVars(typeof(ConVar));

        MyLogger.Info("End plugin load");
    }

    public override void Unload(bool hotReload)
    {
        PlayerEvents.Unload();
        Maps = null;
        GamePhase = GamePhase.Sleeping;
    }
}