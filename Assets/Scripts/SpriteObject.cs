using UnityEngine;

[CreateAssetMenu(fileName = "sprite", menuName = "Scriptable Object/SpriteObject")]
public class SpriteObject : ScriptableObject
{
    public Sprite sprite;
    public string tag;
}
