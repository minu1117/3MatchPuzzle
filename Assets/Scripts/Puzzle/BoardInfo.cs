using UnityEngine;

public class BoardInfo : MonoBehaviour
{
    [Header("Background Tile")]
    [SerializeField] private GameObject backgroundTilePrefab;

    private Row[] rows;
    private Grid[,] grids;

    public int width { get; private set; }
    public int height { get; private set; }

    public void CreateGrids()
    {
        rows = new Row[height * 2];
        grids = new Grid[height * 2, width];
        for (int h = 0; h < height * 2; h++)
        {
            rows[h] = new Row();
            for (int w = 0; w < width; w++)
            {
                grids[h, w] = new Grid();
            }
        }
    }

    public Puzzle GetPuzzle((int, int) gridNum)
    {
        if (gridNum.Item1 >= height * 2 ||
            gridNum.Item2 >= width ||
            gridNum.Item1 < 0 ||
            gridNum.Item2 < 0)
            return null;

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

    public void SetGridPosition(Vector2 pos, (int, int) gridNum)
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
