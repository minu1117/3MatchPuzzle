using UnityEngine;

public class Board
{
    public Grid[,] grids;
    public int boardSize;

    public Board(int width, int height)
    {
        grids = new Grid[height*2, width];
        for (int h = 0; h < height*2; h++)
        {
            for (int w = 0; w < width; w++)
            {
                grids[h, w] = new Grid();
            }
        }

        boardSize = width * height;
    }

    public Puzzle GetPuzzle((int, int) gridNum)
    {
        return grids[gridNum.Item1, gridNum.Item2].Puzzle;
    }

    public void SetPuzzle(Puzzle p, (int, int) gridNum)
    {
        grids[gridNum.Item1, gridNum.Item2].Puzzle = p;
    }

    public void SetGridNum((int, int) gridNum)
    {
        grids[gridNum.Item1, gridNum.Item2].GridNum = gridNum;
    }

    public void SetGridPosition(Vector3 pos, (int, int) gridNum)
    {
        grids[gridNum.Item1, gridNum.Item2].Position = pos;
    }

    public Vector2 GetGridPosition((int, int) gridNum)
    {
        return grids[gridNum.Item1, gridNum.Item2].Position;
    }

    public (int, int) GetGridNum((int, int) gn)
    {
        return grids[gn.Item1, gn.Item2].GridNum;
    }
}
