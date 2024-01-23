using UnityEngine;

public class Grid : MonoBehaviour
{
    public Vector2 Position { get; set; }

    public Puzzle Puzzle { get; set; }

    public (int, int) GridNum { get; set; }

    public bool IsBlocked { get; set; }
}
