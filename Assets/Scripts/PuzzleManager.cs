using System;
using System.Collections;
using UnityEngine;

public class PuzzleManager : MonoBehaviour
{
    // puzzle : Object Pooling
    [SerializeField] private GameObject backgroundTilePrefab;
    [SerializeField] private Puzzle puzzlePrefab;
    [SerializeField] private Sprite[] PuzzleSprites;
    private GameObject[,] board;
    private Puzzle[,] puzzles;
    public int width;
    public int height;

    private void Start()
    {
        CreateBoardVector(width, height);
        CreateBackgroundTiles(width, height);
    }

    private void CreateBoardVector(int width, int height)
    {
        board = new GameObject[width, height];
        puzzles = new Puzzle[width, height];
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
                board[j, i] = backgroundtile;
            }
        }
    }

    // Create all puzzles
    private IEnumerator CreateAllPuzzles(int width, int height)
    {
        if (width == 0 || height == 0)
            yield break;

        /*
         
                  �ۡۡۡۡ�
                  �ۡۡۡۡ�
                  �ۡۡۡۡ�
                  �ۡۡۡۡ�
           i -> { �ܡܡܡܡ� }

         */
        for (int i = 0; i < height; i++)
        {
            Puzzle[] puzzles = CreateHorizontalLinePuzzle(width);
            Array.Copy(puzzles, 0, puzzles, i, puzzles.Length);
            yield return null;
        }
    }

    // Create one row puzzles
    private Puzzle[] CreateHorizontalLinePuzzle(int width)
    {
        Puzzle[] tiles = new Puzzle[width];

        // 1 2 3 4 5 ---> one row
        for (int i = 0; i < width; i++)
        {
            tiles[i] = CreatePuzzle();
        }

        return tiles;
    }

    // Create one puzzle
    private Puzzle CreatePuzzle()
    {
        Puzzle p = Instantiate(puzzlePrefab);

        PuzzleType pt = (PuzzleType)UnityEngine.Random.Range(0, (int)PuzzleType.Count);
        int randomIndex = UnityEngine.Random.Range(0, PuzzleSprites.Length);

        p.SetType(pt);
        // p.SetSprite(PuzzleSprites[randomIndex]);

        return p;
    }
}
