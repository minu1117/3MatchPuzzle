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
}

public class StageInfo : IPrefabSaveable
{
    public StageInfoData data;

    public StageInfo(StageInfoData data)
    {
        this.data = data;
    }

    public void Save(string name, BoardType type)
    {
        data.stageName = name;
        var json = MyJsonUtility.ToJson(this);
        MyJsonUtility.SaveJson(json, name, InfoType.Stage, type);
    }

    public void LoadData(string name, BoardType type)
    {
        data = MyJsonUtility.LoadJson<StageInfoData>(name, InfoType.Board, type);
    }

    public void OverWritePrefab()
    {
        string stageFileName = $"{data.stageName}_StageInfo.prefab";

        string[] stageFolders = Directory.GetDirectories(Application.dataPath, $"{GameManager.Instance.stageSaveFolderName}/");
        foreach (string stageFolder in stageFolders)
        {
            bool isSaved = false;
            string[] prefabs = Directory.GetFiles(stageFolder, "*StageInfo.prefab");
            foreach (string prefabPath in prefabs)
            {
                string fileName = Path.GetFileName(prefabPath);
                if (fileName == stageFileName)
                {
                    //PrefabUtility.SaveAsPrefabAsset(gameObject, prefabPath);
                    isSaved = true;
                    break;
                }
            }

            if (isSaved)
                break;
        }
    }

    public void SetStarCount(int count)
    {
        if (data.clearStarCount >= count)
            return;

        data.clearStarCount = count;
    }
}
