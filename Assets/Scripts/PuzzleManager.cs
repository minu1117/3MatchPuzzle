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
    private Vector2 puzzleSpriteSize;
    public int width;
    public int height;

    private void Start()
    {
        Texture2D puzzle = puzzlePrefab.GetComponent<SpriteRenderer>().sprite.texture;
        puzzleSpriteSize = new Vector2(puzzle.width, puzzle.height);

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
    private bool isMoved = false;

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

            isMoved = false;
        }
        else if (Input.GetMouseButton(0))
        {
            currMousePos = Input.mousePosition;
            Vector3 moveDir = currMousePos - clickStartPos;
            MouseMoveDir dir = CalcMouseMoveDirection(moveDir);

            if (!isMoved)
            {
                if (dir != MouseMoveDir.None && clickedPuzzle != null)
                {
                    ChangePuzzle(dir);
                    isMoved = true;
                }
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
                if (currGn.Item2 > 0)
                {
                    newGn = puzzles[currGn.Item1, currGn.Item2 - 1].gridNum;
                    Debug.Log("Left");
                }
                else
                    return;
                break;
            case MouseMoveDir.Right:
                if (currGn.Item2 < width-1)
                {
                    newGn = puzzles[currGn.Item1, currGn.Item2 + 1].gridNum;
                    Debug.Log("Rigth");
                }
                else
                    return;
                break;
            case MouseMoveDir.Up:
                if (currGn.Item1 < height-1)
                {
                    newGn = puzzles[currGn.Item1 + 1, currGn.Item2].gridNum;
                    Debug.Log("Up");
                }
                else
                    return;
                break;
            case MouseMoveDir.Down:
                if (currGn.Item2 >= 0)
                {
                    newGn = puzzles[currGn.Item1 - 1, currGn.Item2].gridNum;
                    Debug.Log("Down");
                }
                else
                    return;
                break;
            default:
                break;
        }

        Puzzle currPuzzle = puzzles[currGn.Item1, currGn.Item2];
        Puzzle movePuzzle = puzzles[newGn.Item1, newGn.Item2];

        Vector2 currBoardPos = board[currGn.Item1, currGn.Item2].transform.position;
        Vector2 moveBoardPos = board[newGn.Item1, newGn.Item2].transform.position;

        currPuzzle.SetGridNum(newGn);
        movePuzzle.SetGridNum(currGn);

        // change
        Puzzle tempPuzzle = puzzles[currGn.Item1, currGn.Item2];
        puzzles[currGn.Item1, currGn.Item2] = puzzles[newGn.Item1, newGn.Item2];
        puzzles[newGn.Item1, newGn.Item2] = tempPuzzle;

        float speed = 5f;
        StartCoroutine(currPuzzle.CoMove(moveBoardPos, speed));
        StartCoroutine(movePuzzle.CoMove(currBoardPos, speed));
    }

    private MouseMoveDir CalcMouseMoveDirection(Vector2 moveDir)
    {
        // 이동 각도 계산 (라디안에서 도로 변환)
        float angleInDegrees = Mathf.Atan2(moveDir.y, moveDir.x) * Mathf.Rad2Deg;

        MouseMoveDir dir = MouseMoveDir.None;

        float absX = Mathf.Abs(moveDir.x);
        float absY = Mathf.Abs(moveDir.y);

        float spriteX = puzzleSpriteSize.x / 9;
        float spriteY = puzzleSpriteSize.y / 9;

        if (angleInDegrees > 45 && angleInDegrees < 135) // Up
        {
            if (absY >= spriteY)
            {
                dir = MouseMoveDir.Up;
            }
        }
        else if (angleInDegrees > -135 && angleInDegrees < -45) // Down
        {
            if (absY >= spriteY)
            {
                dir = MouseMoveDir.Down;
            }
        }
        else if ((angleInDegrees >= 135 && angleInDegrees <= 180) || (angleInDegrees >= -180 && angleInDegrees <= -135)) // Left
        {
            if (absX >= spriteX)
            {
                dir = MouseMoveDir.Left;
            }
        }
        else if (angleInDegrees >= -45 && angleInDegrees <= 45) // Right
        {
            if (absX >= spriteX)
            {
                dir = MouseMoveDir.Right;
            }
        }

        return dir;
    }
}
