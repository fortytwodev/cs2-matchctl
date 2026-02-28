namespace BasicFaceitServer.Core;

public enum MatchState
{
    PreKnifeWarmup,
    PostKnifeWarmup,
    Knife,
    MatchLive,
    Sleeping
}

public enum GameState
{
    Paused,
    Live
}
