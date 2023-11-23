using UnityEngine;

public class BackgroundTile : MonoBehaviour
{
    public int X { get; set; }
    public int Y { get; set; }

    public RectTransform RectTransform { get; set; }

    public void Awake()
    {
        RectTransform = GetComponent<RectTransform>();
    }
}
