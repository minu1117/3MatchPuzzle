using System.Collections;
using UnityEngine;

public partial class PuzzleManager : MonoBehaviour
{
    // puzzle : Object Pooling
    [SerializeField] private GameObject backgroundTilePrefab;
    [SerializeField] private Puzzle puzzlePrefab;
    public SpriteRenderer[] puzzleSpritePrefabs;
    private GameObject[,] board;
    private Puzzle[,] puzzles;
    public int width;
    public int height;

    private void Start()
    {
        CreateGrid(width, height);
        CreateBackgroundTiles(width, height);
        StartCoroutine(CoCreateAllPuzzles(width, height));
    }

    private void CreateGrid(int width, int height)
    {
        board = new GameObject[height, width];
        puzzles = new Puzzle[height, width];
    }

    private Vector2 GetSpriteBounds(SpriteRenderer sprite)
    {
        float width = sprite.bounds.size.x;
        float height = sprite.bounds.size.y;

        return new Vector2(width, height);
    }

    private void CreateBackgroundTiles(int width, int height)
    {
        if (width == 0 || height == 0)
            return;

        SpriteRenderer sprite = backgroundTilePrefab.GetComponent<SpriteRenderer>();
        Vector2 tileSize = GetSpriteBounds(sprite);

        /*
         Create order
         width : i
         height : j
         7 8 9 
         4 5 6 
         1 2 3
        */
        for (int i = 0; i < height; i++)
        {
            for (int j = 0; j < width; j++)
            {
                float posX = j * tileSize.x;
                float posY = i * tileSize.y;
                Vector2 tilePosition = new Vector2(posX, posY);

                GameObject backgroundtile = Instantiate(backgroundTilePrefab, tilePosition, Quaternion.identity);
                board[i, j] = backgroundtile;
            }
        }
    }

    // Create all puzzles
    private IEnumerator CoCreateAllPuzzles(int width, int height)
    {
        for (int i = 0; i < height; i++)
        {
            CreateRowPuzzles(i, width);
            yield return null;
        }
    }

    // Create one row puzzles
    private void CreateRowPuzzles(int rowIndex, int width)
    {
        /*

                        �ۡۡۡۡ�
                        �ۡۡۡۡ�
                        �ۡۡۡۡ�
                        �ۡۡۡۡ�
          rowindex -> { �ܡܡܡܡ� }

        */

        if (this.width == 0 || this.height == 0)
            return;

        for (int j = 0; j < width; j++)
        {
            puzzles[rowIndex, j] = CreatePuzzle();
            puzzles[rowIndex, j].gameObject.transform.position = board[rowIndex, j].transform.position;
        }
    }

    // Create one puzzle
    private Puzzle CreatePuzzle()
    {
        Puzzle p = Instantiate(puzzlePrefab);
        PuzzleType pt = (PuzzleType)UnityEngine.Random.Range(0, (int)PuzzleType.Count);
        p.SetType(pt, puzzleSpritePrefabs);

        return p;
    }

    /*----------------------------------------------------------------------------------------------------------------------------*/
}
