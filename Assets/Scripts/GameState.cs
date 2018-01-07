public static class GameState
{
    public static GameMode Mode = GameMode.Playing;

    public enum GameMode
    {
        Playing,
        Disappearing,
        Falling,
    }
}
