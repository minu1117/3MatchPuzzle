using UnityEngine;

public class PuzzleManager : MonoBehaviour
{
    // Tile : Object Pooling
    [SerializeField] private GameObject backgroundTilePrefab;
    [SerializeField] private GameObject tilePrefab;
    private GameObject[,] board;
    private GameObject[,] tiles;
    public int width;
    public int height;

    void Start()
    {
        if (width == 0 || height == 0)
            return;

        board = new GameObject[width, height];

        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++) 
            {
                Vector2 position = new Vector2 (i, j);
                GameObject backgroundtile = Instantiate(backgroundTilePrefab, position, Quaternion.identity);
                board[i, j] = backgroundtile;
            }
        }
    }
}
