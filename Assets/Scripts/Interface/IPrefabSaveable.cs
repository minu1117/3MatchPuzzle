using UnityEngine;

public interface IPrefabSaveable
{
    public GameObject SavePrefab(string folderPath, string name);
}
