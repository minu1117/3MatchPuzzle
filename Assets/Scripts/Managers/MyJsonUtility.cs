using System;
using System.IO;
using System.Xml.Linq;
using UnityEngine;

public enum InfoType
{
    Stage,
    Board,
}

public enum BoardType
{
    Stage,
    Custom,
}

public static class MyJsonUtility
{
    public static string path = Application.persistentDataPath;

    public static string ToJson(object obj)
    {
        return JsonUtility.ToJson(obj);
    }

    public static void SaveJson(string json, string name, InfoType infoType, BoardType boardType, bool changeCreationTime)
    {
        string boardTypeStr = boardType == BoardType.Stage ? "Stage" : "CustomBoard";
        string saveFolder = $"{path}/{boardTypeStr}/{name}";
        if (!Directory.Exists(saveFolder))
        {
            Directory.CreateDirectory(saveFolder);
        }

        string typeStr = infoType == InfoType.Stage ? "StageInfo" : "BoardInfo";
        string folderPath = $"{saveFolder}/{name}_{typeStr}.json";
        File.WriteAllText(folderPath, json);

        if (changeCreationTime)
            Directory.SetCreationTime(saveFolder, DateTime.Now);
    }

    public static T LoadJson<T>(string name, InfoType infoType, BoardType boardType)
    {
        string json = GetSaveFilePath(name, infoType, boardType);
        string jsonParse = File.ReadAllText(json);
        return JsonUtility.FromJson<T>(jsonParse);
    }

    public static bool Exists(BoardType boardType)
    {
        string boardTypeStr = boardType == BoardType.Stage ? "Stage" : "CustomBoard";
        string saveFolder = $"{path}/{boardTypeStr}";
        if (!Directory.Exists($"{saveFolder}"))
        {
            return false;
        }

        return true;
    }

    public static string GetSaveFolderPath(BoardType boardType)
    {
        string boardTypeStr = boardType == BoardType.Stage ? "Stage" : "CustomBoard";
        return $"{path}/{boardTypeStr}";
    }

    public static string GetSaveFilePath(string name, InfoType infoType, BoardType boardType)
    {
        string boardTypeStr = boardType == BoardType.Stage ? "Stage" : "CustomBoard";
        string typeStr = infoType == InfoType.Stage ? "StageInfo" : "BoardInfo";
        string saveFolder = $"{path}/{boardTypeStr}/{name}";
        return $"{saveFolder}/{name}_{typeStr}.json";
    }
}
