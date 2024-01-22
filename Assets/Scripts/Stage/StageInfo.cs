using System;
using System.IO;
using UnityEditor;
using UnityEngine;

public class StageInfo : MonoBehaviour, IPrefabSaveable
{
    public BoardInfo boardInfo;
    public int clearScore;
    public float maxPlayTime;
    public bool isInfinityMode;
    public bool isStageMode;
    public string stageName;
    public int clearStarCount;

    public GameObject SavePrefab(string folderPath, string name)
    {
        stageName = name;
        string prefabPath = $"{folderPath}/{stageName}_StageInfo.prefab";
        GameObject prefab = PrefabUtility.SaveAsPrefabAsset(gameObject, prefabPath);

        DirectoryInfo directoryInfo = new(folderPath);
        directoryInfo.CreationTime = DateTime.Now;
        return prefab;
    }

    public void OverWritePrefab()
    {
        string stageFileName = $"{stageName}_StageInfo.prefab";

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
                    PrefabUtility.SaveAsPrefabAsset(gameObject, prefabPath);
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
        if (clearStarCount >= count)
            return;

        clearStarCount = count;
    }
}
