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

    public static int ActionsTaken = 0;

    public static void ResetGame()
    {
        if (Mode == GameMode.Playing)
        {
            ActionsTaken = 0;
            //Board.GenerateBoard();
        }
    }
}
