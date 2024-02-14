using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[SerializeField]
public struct BoardInfoData
{
    public int width;
    public int height;
    public List<bool> blockKeyList;
    public GridLayoutGroup.Constraint constraint;
    public int constraintCount;
}

public class BoardInfo : IBoardSaveable
{
    private Dictionary<(int, int), bool> saveGridDict = new();
    public BoardInfoData data;

    public BoardInfo(BoardInfoData data)
    {
        this.data = data;
    }

    public void Save(string name, BoardType type)
    {
        var json = MyJsonUtility.ToJson(data);
        bool changeCreationTime = type == BoardType.Custom;
        MyJsonUtility.SaveJson(json, name, InfoType.Board, type, changeCreationTime);
    }

    public void LoadData(string name, BoardType type)
    {
        var jsonLoad = MyJsonUtility.LoadJson<BoardInfoData>(name, InfoType.Board, type);
        if (!jsonLoad.Item2)
            return;

        data = jsonLoad.Item1;
    }

    public void SetBoardSize(int width, int height)
    {
        data.width = width;
        data.height = height;
    }

    public void SetGridLayoutData(GridLayoutGroup gridlayout)
    {
        data.constraint = gridlayout.constraint;
        data.constraintCount = gridlayout.constraintCount;
    }

    public GridLayoutGroup.Constraint GetConstraintType()
    {
        return data.constraint;
    }

    public int GetConstaintCount()
    {
        return data.constraintCount;
    }

    public void SaveGridBlocked(bool isBlocked)
    {
        if (data.blockKeyList == null)
            data.blockKeyList = new();

        data.blockKeyList.Add(isBlocked);
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
            for (int y = 0; y < data.height; y++)
            {
                for (int x = 0; x < data.width; x++)
                {
                    bool blocked = data.blockKeyList[index];
                    saveGridDict.Add((y, x), blocked);

                    index++;
                }
            }
        }

        //if (grids != null)
        //{
        //    var gridKeys = saveGridDict.Keys;
        //    foreach ((int, int) grid in gridKeys)
        //    {
        //        grids[grid.Item1, grid.Item2].IsBlocked = saveGridDict[grid];
        //    }
        //}
    }

    public void ClearSaveDict()
    {
        saveGridDict.Clear();
    }
}
