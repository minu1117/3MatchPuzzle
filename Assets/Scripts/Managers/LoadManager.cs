using UnityEngine;

public static class LoadManager
{
    public static SpriteObject[] Sprites { get; private set; }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Initalize()
    {
        Sprites = Resources.LoadAll<SpriteObject>("SpriteObjects/");
    }
}
