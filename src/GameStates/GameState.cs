namespace BasicFaceitServer.GameStates;

public enum GamePhase
{
    PreKnifeWarmup,
    PostKnifeWarmup,
    Knife,
    MatchLive,
    Sleeping
}

public enum MatchState
{
    Paused,
    Live
}