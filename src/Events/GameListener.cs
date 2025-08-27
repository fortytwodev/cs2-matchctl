using BasicFaceitServer.GameStates;
using BasicFaceitServer.Utils;
using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;

namespace BasicFaceitServer.Events;

public class GameListener(BasicFaceitServer core)
{
    public DateTime LastMessageTime { get; set; }
    public DateTime LastWarmupEndValueCheckTime { get; set; }
    public int WarmupTime { get;  set; }
    public float WarmupStartTime { get; set; }
    public float WarmupEndTime { get; set; }
    
    public void Load()
    {
        core.RegisterListener<Listeners.OnServerHibernationUpdate>(OnServerHibernationUpdate);
        core.RegisterListener<Listeners.OnTick>(OnTick);
    }

    private void OnTick()
    {
        if (core.GameUtils.IsPreWarmup())
        {
            if ((DateTime.Now - LastWarmupEndValueCheckTime).TotalSeconds >= 1)
            {
                var gameRules = core.Helper.GetGameRules();
                if (gameRules != null)
                {
                    if ((int) WarmupEndTime != (int) gameRules.WarmupPeriodEnd)
                    {
                        WarmupEndTime = gameRules.WarmupPeriodEnd;
                        LastMessageTime = DateTime.Now.AddSeconds(-(60 - (((int) WarmupTime - (int) Server.CurrentTime) % 60)*60));
                    }
                }
                LastWarmupEndValueCheckTime = DateTime.Now;
            }
            
            var elapsedTime = DateTime.Now - LastMessageTime;
            if (elapsedTime.TotalSeconds >= 60)
            {
                var leftTime = (int)WarmupEndTime - (int)Server.CurrentTime;
                if (leftTime % 60 == 0 && leftTime / 60 > 0)
                {
                    var minutes = leftTime / 60;
                    core.Helper.PrintToChatAll($"{{green}}Oyın baslanıwına {minutes} minut qaldı");
                    LastMessageTime = DateTime.Now;
                }
            }
        }
        
        // if (core.MatchBeingPlayedIn)
        //     core.Helper.GetPlayers()
        //         .ToList()
        //         .ForEach(OnMatchBeingPlayedIn);
    }

    private void OnMatchBeingPlayedIn(CCSPlayerController player)
    {
        var imgPath = Path.Combine(core.ModuleDirectory, "F9jJeIw3percent.png");
        string organizer =
            $"<font class='fontSize-m' color='red'>Организатор турнира</font><br><img src='{imgPath}' width='64' height='64'/>";
        player.PrintToCenterHtml($"{organizer}");
    }

    private void OnServerHibernationUpdate(bool isHibernating)
    {
        if (!isHibernating) return;

        MyLogger.Info($"[OnServerHibernationUpdate]: Hibernating. Reset game state");
        core.GamePhase = GamePhase.Sleeping;
    }
}