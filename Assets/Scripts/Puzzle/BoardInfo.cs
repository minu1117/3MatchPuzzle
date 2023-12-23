using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BoardInfo : MonoBehaviour
{
    public Row[] rows { get; private set; }
    public Grid[,] grids { get; private set; } // grids[y,x]

    public int width;
    public int height;

    private Dictionary<(int, int), bool> saveGridDict = new();
    [SerializeField] private List<bool> blockKeyList;

    public void SetBoardSize(int width, int height)
    {
        this.width = width;
        this.height = height;
    }

    public void CreateGrids(Grid backgroundTilePrefab, GridLayoutGroup backgroundParentsObject)
    {
        rows = new Row[height * 2];
        grids = new Grid[height * 2, width];

        for (int h = 0; h < height * 2; h++)
        {
            rows[h] = new Row();
            for (int w = 0; w < width; w++)
            {
                var grid = Instantiate(backgroundTilePrefab, backgroundParentsObject.transform);
                grids[h, w] = grid;

                if (h >= height)
                {
                    grid.gameObject.SetActive(false);
                }
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

    public void SaveGridBlocked(bool isBlocked)
    {
        if (blockKeyList == null)
            blockKeyList = new();

        blockKeyList.Add(isBlocked);
    }

    public bool GetBlockedGrid((int, int) gn)
    {
        if (saveGridDict.TryGetValue(gn, out bool blockGrid))
            return blockGrid;

        return false;
    }

    public void LoadGridsBlockData()
    {
        int index = 0;
        for (int h = 0; h < height; h++)
        {
            for (int w = 0; w < width; w++)
            {
                bool blocked = blockKeyList[index];
                saveGridDict.Add((h,w), blocked);

                index++;
            }
        }
        
        var gridKeys = saveGridDict.Keys;
        foreach ((int,int) grid in gridKeys)
        {
            grids[grid.Item1, grid.Item2].IsBlocked = saveGridDict[grid];
        }
    }

    public bool GetIsBlocked((int, int) gn)
    {
        return grids[gn.Item1, gn.Item2].IsBlocked;
    }
}
