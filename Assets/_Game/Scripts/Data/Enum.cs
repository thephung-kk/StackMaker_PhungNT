public enum PlayerMoveDirection
{
    None = 0,
    Forward = 1,
    Right = 2,
    Backward = 3,
    Left = 4
}
public enum GameState
{
    WaitToStart,
    InGame,
    FinishLevel,
    Lost,
    Won
}