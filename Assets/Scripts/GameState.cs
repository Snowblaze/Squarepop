using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

public static class GameState
{
    public enum GameMode
    {
        Playing,
        Disappearing,
        Falling,
    }

    public static GameMode Mode = GameMode.Playing;
}
