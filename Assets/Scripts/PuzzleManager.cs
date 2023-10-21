using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UIElements;

public partial class PuzzleManager : MonoBehaviour
{
    // puzzle : Object Pooling
    [SerializeField] private GameObject backgroundTilePrefab;
    [SerializeField] private Puzzle puzzlePrefab;
    public SpriteRenderer[] puzzleSpritePrefabs;
    private GameObject[,] board;
    private Puzzle[,] puzzles;
    private Vector2 puzzleSpriteBound;
    public int width;
    public int height;

    private void Start()
    {
        puzzleSpriteBound = GetSpriteBounds(puzzlePrefab.GetComponent<SpriteRenderer>());

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

                        ○○○○○
                        ○○○○○
                        ○○○○○
                        ○○○○○
          rowindex -> { ●●●●● }

        */

        if (this.width == 0 || this.height == 0)
            return;

        for (int j = 0; j < width; j++)
        {
            puzzles[rowIndex, j] = CreatePuzzle();
            puzzles[rowIndex, j].gameObject.transform.position = board[rowIndex, j].transform.position;
            puzzles[rowIndex, j].gridNum = (rowIndex, j);
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

    private Vector2 clickStartPos;
    private Vector2 currMousePos;
    private Puzzle clickedPuzzle;

    private enum MouseMoveDir
    {
        None = -1,
        Left = 0,
        Right = 1,
        Up = 2,
        Down = 3,
    }

    public void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            clickStartPos = Input.mousePosition;
            Vector3 worldPos = Camera.main.ScreenToWorldPoint(new Vector3(clickStartPos.x, clickStartPos.y, 0));
            clickedPuzzle = GetClickedPuzzle(worldPos);
        }
        else if (Input.GetMouseButton(0))
        {
            currMousePos = Input.mousePosition;
            Vector3 moveDir = currMousePos - clickStartPos;
            MouseMoveDir dir = CalcMouseMoveDirection(moveDir);

            if (dir != MouseMoveDir.None && clickedPuzzle != null)
            {
                ChangePuzzle(dir);
            }
        }
    }

    private Puzzle GetClickedPuzzle(Vector3 worldPos)
    {
        Puzzle puzzle;

        RaycastHit2D hit = Physics2D.Raycast(worldPos, Vector2.zero);
        if (hit.collider != null)
        {
            if (hit.collider.gameObject.TryGetComponent(out puzzle))
            {
                (int, int) g = puzzle.gridNum;
                return puzzles[g.Item1, g.Item2];
            }
        }

        return null;
    }

    private void ChangePuzzle(MouseMoveDir dir) 
    {
        (int, int) currGn = clickedPuzzle.gridNum;
        (int, int) newGn = (0, 0);

        switch (dir)
        {
            case MouseMoveDir.Left:
                if (currGn.Item2 < 0)
                {
                    newGn = puzzles[currGn.Item1, currGn.Item2 - 1].gridNum;
                }
                else
                    return;
                break;
            case MouseMoveDir.Right:
                if (currGn.Item2 < width-1)
                {
                    newGn = puzzles[currGn.Item1, currGn.Item2 + 1].gridNum;
                }
                else
                    return;
                break;
            case MouseMoveDir.Up:
                if (currGn.Item1 < height-1)
                {
                    newGn = puzzles[currGn.Item1 + 1, currGn.Item2].gridNum;
                }
                else
                    return;
                break;
            case MouseMoveDir.Down:
                if (currGn.Item2 >= 0)
                {
                    newGn = puzzles[currGn.Item1, currGn.Item2-1].gridNum;
                }
                else
                    return;
                break;
            default:
                break;
        }

        puzzles[currGn.Item1, currGn.Item2].gridNum = newGn;
        puzzles[currGn.Item1, currGn.Item2].gridNum = currGn;

        Vector2 currPos = board[currGn.Item1, currGn.Item2].transform.position;
        Vector2 movePos = board[newGn.Item1, newGn.Item2].transform.position;

        puzzles[currGn.Item1, currGn.Item2].gameObject.transform.position = Vector2.Lerp(currPos, movePos, 1f);
        puzzles[newGn.Item1, newGn.Item2].gameObject.transform.position = Vector2.Lerp(movePos, currPos, 1f);
    }

    private MouseMoveDir CalcMouseMoveDirection(Vector3 moveDir)
    {
        // 이동 각도 계산 (라디안에서 도로 변환)
        float angleInDegrees = Mathf.Atan2(moveDir.y, moveDir.x) * Mathf.Rad2Deg;

        MouseMoveDir dir = MouseMoveDir.None;
        if (angleInDegrees > 45 && angleInDegrees < 135) // Up
        {
            if (Mathf.Abs(moveDir.y) >= puzzleSpriteBound.y / 2)
            {
                dir = MouseMoveDir.Up;
            }
        }
        else if (angleInDegrees > -135 && angleInDegrees < -45) // Down
        {
            if (Mathf.Abs(moveDir.y) >= puzzleSpriteBound.y / 2)
            {
                dir = MouseMoveDir.Down;
            }
        }
        else if ((angleInDegrees >= 135 && angleInDegrees <= 180) || (angleInDegrees >= -180 && angleInDegrees <= -135)) // Left
        {
            if (Mathf.Abs(moveDir.x) >= puzzleSpriteBound.x / 2)
            {
                dir = MouseMoveDir.Left;
            }
        }
        else if (angleInDegrees >= -45 && angleInDegrees <= 45) // Right
        {
            if (Mathf.Abs(moveDir.x) >= puzzleSpriteBound.x / 2)
            {
                dir = MouseMoveDir.Right;
            }
        }

        return dir;
    }
}
