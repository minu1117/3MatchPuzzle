using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BoardInfo : MonoBehaviour
{
    public Row[] rows { get; private set; }
    public Grid[,] grids { get; private set; } // grids[y,x]

    public int width;
    public int height;

    [SerializeField] private List<bool> blockKeyList;
    private Dictionary<(int, int), bool> saveGridDict = new();

    [SerializeField] private GridLayoutGroup.Constraint constraint;
    [SerializeField] private int constraintCount;

    public void SetBoardSize(int width, int height)
    {
        this.width = width;
        this.height = height;
    }

    public void SetGridLayoutData(GridLayoutGroup gridlayout)
    {
        constraint = gridlayout.constraint;
        constraintCount = gridlayout.constraintCount;
    }

    public GridLayoutGroup.Constraint GetConstraintType()
    {
        return constraint;
    }

    public int GetConstaintCount()
    {
        return constraintCount;
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

    public Puzzle GetPuzzle(int x, int y)
    {
        if (y >= height * 2 ||
            x >= width ||
            y < 0 ||
            x < 0)
            return null;

        return grids[y, x].Puzzle;
    }

    public void SetPuzzle(Puzzle p, int x, int y)
    {
        grids[y, x].Puzzle = p;
    }

    public void SetGridNum(int x, int y)
    {
        grids[y, x].GridNum = (y, x);
    }

    public void SetGridPosition(Vector2 pos, int x, int y)
    {
        grids[y, x].Position = pos;
    }

    public Vector2 GetGridPosition(int x, int y)
    {
        return grids[y, x].Position;
    }

    public (int, int) GetGridNum(int x, int y)
    {
        return grids[y, x].GridNum;
    }

    public void SaveGridBlocked(bool isBlocked)
    {
        if (blockKeyList == null)
            blockKeyList = new();

        blockKeyList.Add(isBlocked);
    }

    public bool GetBlockedGrid(int x, int y)
    {
        if (saveGridDict.TryGetValue((y, x), out bool blockGrid))
            return blockGrid;

        return false;
    }

    public void LoadGridsBlockData()
    {
        if (saveGridDict.Count == 0)
        {
            int index = 0;
            for (int h = 0; h < height; h++)
            {
                for (int w = 0; w < width; w++)
                {
                    bool blocked = blockKeyList[index];
                    saveGridDict.Add((h, w), blocked);

                    index++;
                }
            }
        }      

        if (grids != null)
        {
            var gridKeys = saveGridDict.Keys;
            foreach ((int, int) grid in gridKeys)
            {
                grids[grid.Item1, grid.Item2].IsBlocked = saveGridDict[grid];
            }
        }
    }

    public void ClearSaveDict()
    {
        saveGridDict.Clear();
    }

    public bool GetBlocked(int checkX, int yIndex)
    {
        for (int y = yIndex; y < height; y++)
        {
            if (IsBlocked(checkX, y))
            {
                return true;
            }
        }

        return false;
    }

    private bool IsBlocked(int x, int y)
    {
        return grids[y, x].IsBlocked;
    }
}
