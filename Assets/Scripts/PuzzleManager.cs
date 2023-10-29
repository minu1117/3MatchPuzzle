using System.Collections;
using UnityEngine;
using System.Collections.Generic;

public class PuzzleManager : MonoBehaviour
{
    // puzzle : Object Pooling
    [SerializeField] private GameObject backgroundTilePrefab;
    [SerializeField] private Puzzle puzzlePrefab;
    public SpriteRenderer[] puzzleSpritePrefabs;
    private Board board;
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
        board = new Board(width, height);
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
                board.SetBgPosition(backgroundtile, (i, j));
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
            Puzzle pz = puzzles[rowIndex, j] = CreatePuzzle();
            Puzzle lp = null;
            Puzzle bp = null;

            if (j > 0)
            {
                lp = puzzles[rowIndex, j - 1];
            }
            if (rowIndex > 0)
            {
                bp = puzzles[rowIndex - 1, j];
            }

            // 왼쪽, 아래 타입 검사
            SetNotDuplicationPuzzleType(pz, lp, bp);
            SetNotDuplicationPuzzleType(pz, bp, lp);

            (int, int) gn = (rowIndex, j);

            pz.gameObject.transform.position = board.GetBgPosition(gn);
            pz.gridNum = gn;

            board.SetPuzzle(pz, gn);
            board.SetGridNum(gn);
        }
    }

    // Create one puzzle
    private Puzzle CreatePuzzle()
    {
        Puzzle p = Instantiate(puzzlePrefab);
        return p;
    }

    private void SetNotDuplicationPuzzleType(Puzzle p, Puzzle cheakP, Puzzle prevP)
    {
        PuzzleType pt = p.type == PuzzleType.None ? (PuzzleType)UnityEngine.Random.Range(0, (int)PuzzleType.Count) : p.type;
        PuzzleType cheakPType = cheakP == null ? PuzzleType.None : cheakP.type;
        PuzzleType prevPType = prevP == null ? PuzzleType.None : prevP.type;

        // 연결 상태 검사
        pt = CheckAndSetPuzzleType(p, pt, cheakP, cheakPType, prevPType);
        pt = CheckAndSetPuzzleType(p, pt, prevP, prevPType, cheakPType);

        p.SetType(pt);
        p.SetSprite(puzzleSpritePrefabs);
    }

    private PuzzleType CheckAndSetPuzzleType(Puzzle p, PuzzleType pt, Puzzle cheakP, PuzzleType cheakPType, PuzzleType prevPType)
    {
        PuzzleType newPt = pt;

        if (newPt == cheakPType)
        {
            if (cheakP.isConnected && !p.isConnected || newPt == prevPType)
            {
                newPt = GetNotDuplicationPuzzleType(cheakPType, prevPType);
            }
            else if (newPt != prevPType)
            {
                p.isConnected = true;
                cheakP.isConnected = true;
            }
        }

        return newPt;
    }

    private PuzzleType GetNotDuplicationPuzzleType(PuzzleType t, PuzzleType prevT)
    {
        bool isDuplicate = true;
        PuzzleType p = PuzzleType.None;

        while (isDuplicate)
        {
            p = (PuzzleType)UnityEngine.Random.Range(0, (int)PuzzleType.Count);
            if (p != t && p != prevT)
                isDuplicate = false;
        }

        return p;
    }

    /*--------------------------------------------------------- Move -------------------------------------------------------------------*/

    private Vector2 clickStartPos;
    private Vector2 currMousePos;
    private Puzzle clickedPuzzle;
    private bool isMoved = false;
    private float moveSpeed = 5f;

    private Coroutine moveCo1 = null;
    private Coroutine moveCo2 = null;

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
                    SwapPuzzles(dir);
                    isMoved = true;
                }
            }
        }

        CheakThreeMatchPuzzle();
    }

    private void CheakThreeMatchPuzzle()
    {
        List<Puzzle> destroyList = new List<Puzzle>();

        for (int y = 0; y < height; y++) 
        {
            for (int x = 0; x < width; x++)
            {
                Puzzle p1 = puzzles[y, x];

                if (x < width - 2)
                {
                    Puzzle p2 = puzzles[y, x+1];
                    Puzzle p3 = puzzles[y, x+2];

                    AddMatchingPuzzles(destroyList, p1, p2, p3);
                }

                if (y < height - 2)
                {
                    Puzzle p2 = puzzles[y+1, x];
                    Puzzle p3 = puzzles[y+2, x];

                    AddMatchingPuzzles(destroyList, p1, p2, p3);
                }
            }
        }

        foreach (var p in destroyList)
        {
            if (p != null && p.gameObject != null)
                Destroy(p.gameObject);
        }
    }

    private void AddMatchingPuzzles(List<Puzzle> list, Puzzle p1, Puzzle p2, Puzzle p3)
    {
        if (p2 != null && p3 != null)
        {
            if (p1.type == p2.type && p1.type == p3.type)
            {
                list.Add(p1);
                list.Add(p2);
                list.Add(p3);
            }
        }
    }

    private Puzzle GetClickedPuzzle(Vector3 worldPos)
    {
        Puzzle puzzle = null;

        RaycastHit2D hit = Physics2D.Raycast(worldPos, Vector2.zero);
        if (hit.collider != null)
        {
            if (hit.collider.gameObject.TryGetComponent(out puzzle))
            {
                return puzzle;
            }
        }

        return puzzle;
    }

    private void SwapPuzzles(MouseMoveDir dir)
    {
        (int, int) currGn = clickedPuzzle.gridNum;
        (int, int) newGn = (0, 0);

        bool gridSet = false;
        switch (dir)
        {
            case MouseMoveDir.Left:
                if (currGn.Item2 > 0)
                {
                    newGn = puzzles[currGn.Item1, currGn.Item2 - 1].gridNum;
                    gridSet = true;
                }
                break;
            case MouseMoveDir.Right:
                if (currGn.Item2 < width-1)
                {
                    newGn = puzzles[currGn.Item1, currGn.Item2 + 1].gridNum;
                    gridSet = true;
                }
                break;
            case MouseMoveDir.Up:
                if (currGn.Item1 < height-1)
                {
                    newGn = puzzles[currGn.Item1 + 1, currGn.Item2].gridNum;
                    gridSet = true;
                }
                break;
            case MouseMoveDir.Down:
                if (currGn.Item1 > 0)
                {
                    newGn = puzzles[currGn.Item1 - 1, currGn.Item2].gridNum;
                    gridSet = true;
                }
                break;
            default:
                break;
        }

        if (!gridSet)
            return;

        Puzzle currPuzzle = puzzles[currGn.Item1, currGn.Item2];
        Puzzle movePuzzle = puzzles[newGn.Item1, newGn.Item2];

        Vector2 currPos = currPuzzle.transform.position;
        Vector2 movePos = movePuzzle.transform.position;

        MovePuzzles(currPuzzle, movePuzzle, currPos, movePos, moveSpeed, currGn, newGn);
    }

    private void MovePuzzles(Puzzle currPuzzle, Puzzle movePuzzle, Vector2 currPos, Vector2 movePos, float moveSpeed, (int, int) currGn, (int, int) newGn)
    {
        moveCo1 = StartCoroutine(currPuzzle.CoMove(movePos, moveSpeed));
        moveCo2 = StartCoroutine(movePuzzle.CoMove(currPos, moveSpeed));

        // Wait
        StartCoroutine(WaitForMoveCoroutines(currPuzzle, movePuzzle, currGn, newGn, moveCo1, moveCo2));
    }

    private IEnumerator WaitForMoveCoroutines(Puzzle currPuzzle, Puzzle movePuzzle, (int, int) currGn, (int, int) newGn, params Coroutine[] coroutines)
    {
        foreach (var coroutine in coroutines)
        {
            yield return coroutine;
        }

        Swap(currPuzzle, movePuzzle, currGn, newGn);
    }

     private void Swap(Puzzle currPuzzle, Puzzle movePuzzle, (int, int) currGn, (int, int) newGn)
    {
        Puzzle tempPuzzle = puzzles[currGn.Item1, currGn.Item2];
        puzzles[currGn.Item1, currGn.Item2] = puzzles[newGn.Item1, newGn.Item2];
        puzzles[newGn.Item1, newGn.Item2] = tempPuzzle;

        currPuzzle.SetGridNum(newGn);
        movePuzzle.SetGridNum(currGn);

        board.SetPuzzle(currPuzzle, newGn);
        board.SetPuzzle(movePuzzle, currGn);
    }

    private MouseMoveDir CalcMouseMoveDirection(Vector2 moveDir)
    {
        // angle (radian -> degree)
        float angleInDegrees = Mathf.Atan2(moveDir.y, moveDir.x) * Mathf.Rad2Deg;

        MouseMoveDir dir = MouseMoveDir.None;

        float absX = Mathf.Abs(moveDir.x);
        float absY = Mathf.Abs(moveDir.y);

        // drag range
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
