using UnityEngine;

public class Grid
{
    public Vector2 Position
    {
        get;
        set;
    }

    public Puzzle Puzzle
    {
        get;
        set;
    }

    public (int, int) GridNum
    {
        get;
        set;
    }
}
