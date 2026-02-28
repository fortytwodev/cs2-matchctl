namespace BasicFaceitServer.Core;

public enum MatchState
{
    PreKnifeWarmup,
    PostKnifeWarmup,
    KnifeRound,
    LiveMatch,
    Sleeping
}

public enum GameState
{
    Paused,
    Unpaused
}
