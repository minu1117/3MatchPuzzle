using System.IO;
using UnityEngine;

[SerializeField]
public struct StageInfoData
{
    public int clearScore;
    public float maxPlayTime;
    public bool isInfinityMode;
    public bool isStageMode;
    public string stageName;
    public int clearStarCount;
    public BoardType boardType;
}

public class StageInfo : IBoardSaveable
{
    public StageInfoData data;

    public StageInfo(StageInfoData data)
    {
        this.data = data;
    }

    public void Save(string name, BoardType type)
    {
        data.stageName = name;
        data.boardType = type;
        var json = MyJsonUtility.ToJson(data);
        bool changeCreationTime = type == BoardType.Custom;
        MyJsonUtility.SaveJson(json, name, InfoType.Stage, type, changeCreationTime);
    }

    public void LoadData(string name, BoardType type)
    {
        var jsonLoad = MyJsonUtility.LoadJson<StageInfoData>(name, InfoType.Board, type);
        if (!jsonLoad.Item2)
            return;

        data = jsonLoad.Item1;
    }

    public void OverWriteData()
    {
        Save(data.stageName, data.boardType);
    }

    public void SetStarCount(int count)
    {
        if (data.clearStarCount >= count)
            return;

        data.clearStarCount = count;
    }
}
