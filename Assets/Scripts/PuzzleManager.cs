using System.Collections;
using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Pool;
using System.Linq;

public class PuzzleManager : MonoBehaviour
{
    // puzzle : Object Pooling
    [SerializeField] private GameObject backgroundTilePrefab;
    [SerializeField] private Puzzle puzzlePrefab;
    public SpriteRenderer[] puzzleSpritePrefabs;
    private IObjectPool<Puzzle> puzzlePool;
    private Board board;
    private Vector2 puzzleSpriteSize;
    public int width;
    public int height;

    private void Start()
    {
        Texture2D puzzle = puzzlePrefab.GetComponent<SpriteRenderer>().sprite.texture;
        puzzleSpriteSize = new Vector2(puzzle.width, puzzle.height);
        puzzlePool = new ObjectPool<Puzzle>(
            CreatePuzzle,
            GetPuzzle,
            OnRelease,
            DestroyPuzzle,
            maxSize : width * (height * 3)
            );

        CreateGrid(width, height);
        CreateBackgroundTiles(width, height);
        StartCoroutine(CoCreateAllPuzzles(width, height));
    }

    private void CreateGrid(int width, int height)
    {
        board = new Board(width, height);
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
        for (int i = 0; i < height * 2; i++)
        {
            for (int j = 0; j < width; j++)
            {
                float posX = j * tileSize.x;
                float posY = i * tileSize.y;
                Vector2 tilePosition = new Vector2(posX, posY);

                if (i < height)
                {
                    Instantiate(backgroundTilePrefab, tilePosition, Quaternion.identity);
                }
                board.SetGridPosition(tilePosition, (i, j));
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

        for (int i = height; i < height * 2; i++)
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
            Puzzle pz = puzzlePool.Get();
            (int, int) gn = (rowIndex, j);

            if (rowIndex < height)
            {
                board.SetPuzzle(pz, (rowIndex, j));
                Puzzle lp = null;
                Puzzle bp = null;

                if (j > 0)
                {
                    lp = board.GetPuzzle((rowIndex, j - 1));
                }
                if (rowIndex > 0)
                {
                    bp = board.GetPuzzle((rowIndex - 1, j));
                }

                // 왼쪽, 아래 타입 검사 후 매치되지 않는 퍼즐로 변경
                SetNotDuplicationPuzzleType(pz, lp, bp);
                SetNotDuplicationPuzzleType(pz, bp, lp);
            }
            else
            {
                SetRandomPuzzleType(pz);
                //pz.gameObject.SetActive(false);
            }
            pz.gameObject.name = $"{rowIndex},{j}";
            pz.gridNum = gn;
            pz.gameObject.transform.position = board.GetGridPosition(gn);
            board.SetPuzzle(pz, gn);
            board.SetGridNum(gn);
        }
    }

    // Create one puzzle
    private Puzzle CreatePuzzle()
    {
        Puzzle p = Instantiate(puzzlePrefab);
        SetRandomPuzzleType(p);
        return p;
    }

    private void GetPuzzle(Puzzle p)
    {
        p.gameObject.SetActive(true);
    }

    private void OnRelease(Puzzle p)
    {
        p.isConnected = false;
        p.isMatched = false;
        (int, int) gn = p.gridNum;

        board.SetPuzzle(null, (gn.Item1, gn.Item2));
        p.gameObject.SetActive(false);
    }

    private void DestroyPuzzle(Puzzle p)
    {
        Destroy(p.gameObject);
    }

    private void SetNotDuplicationPuzzleType(Puzzle p, Puzzle cheakP, Puzzle prevP)
    {
        PuzzleType pt = p.type == PuzzleType.None ? (PuzzleType)UnityEngine.Random.Range(0, (int)PuzzleType.Count) : p.type;
        PuzzleType cheakPType = cheakP == null ? PuzzleType.None : cheakP.type;
        PuzzleType prevPType = prevP == null ? PuzzleType.None : prevP.type;

        // 연결 상태 검사
        pt = CheckAndSetPuzzleType(p, pt, cheakP, cheakPType, prevPType);
        pt = CheckAndSetPuzzleType(p, pt, prevP, prevPType, cheakPType);

        SetPuzzleType(p, pt);
    }

    private void SetPuzzleType(Puzzle p, PuzzleType pt)
    {
        p.SetType(pt);
        p.SetSprite(puzzleSpritePrefabs);
    }

    private void SetRandomPuzzleType(Puzzle p)
    {
        PuzzleType pt = (PuzzleType)Random.Range(0, (int)PuzzleType.Count);
        SetPuzzleType(p, pt);
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

    private HashSet<Puzzle> destroyHash = new();

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
            Vector2 worldPos = Camera.main.ScreenToWorldPoint(new Vector2(clickStartPos.x, clickStartPos.y));
            clickedPuzzle = GetClickedPuzzle(worldPos);

            isMoved = false;
        }
        else if (Input.GetMouseButton(0))
        {
            currMousePos = Input.mousePosition;
            Vector2 moveDir = currMousePos - clickStartPos;
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

        if (destroyHash.Count == 0) 
        {
            CheakThreeMatchPuzzle();
        }
        if (destroyHash.Count > 0)
        {
            MoveAndFill();
        }
    }

    private void CheakThreeMatchPuzzle()
    {
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                Puzzle p1 = board.GetPuzzle((y, x));
                Puzzle p2 = null;
                Puzzle p3 = null;

                if (x < width - 2)
                {
                    p2 = board.GetPuzzle((y, x + 1));
                    p3 = board.GetPuzzle((y, x + 2));

                    SetMatchingPuzzles(p1, p2, p3);
                }

                if (y < height - 2)
                {
                    p2 = board.GetPuzzle((y + 1, x));
                    p3 = board.GetPuzzle((y + 2, x));

                    SetMatchingPuzzles(p1, p2, p3);
                }
            }
        }

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                Puzzle p = board.GetPuzzle((y, x));
                if (p != null && p.gameObject != null && p.isMatched)
                {
                    if (destroyHash.Contains(p) || destroyHash.Equals(p))
                    {
                        continue;
                    }

                    destroyHash.Add(p);
                }
            }
        }
    }

    private void MoveAndFill()
    {
        int maxX = 0;
        int maxY = 0;
        int minX = width;
        int minY = height;
        foreach (var p in destroyHash)
        {
            if (!p.gameObject.activeSelf)
                continue;

            int y = p.gridNum.Item1;
            int x = p.gridNum.Item2;
            minY = minY > y ? y : minY;
            maxY = maxY < y ? y : maxY;
            minX = minX > x ? x : minX;
            maxX = maxX < x ? x : maxX;

            puzzlePool.Release(p);
        }

        int moveY = (maxY - minY) + 1;
        MoveDown(minY, moveY, minX, maxX);
    }

    private void MoveDown(int minY, int moveY, int minX, int maxX)
    {
        for (int y = minY; y < board.grids.GetLength(0); y++)
        {
            for (int x = minX; x <= maxX; x++)
            {
                int yGrid = y - moveY < 0 ? 0 : y - moveY;
                Puzzle p = board.GetPuzzle((y, x));
                Puzzle moveP = board.GetPuzzle((yGrid, x));

                if (p != null && moveP == null)
                {
                    Vector2 movePos = board.GetGridPosition((yGrid, x));
                    p.SetGridNum(board.GetGridNum((yGrid, x)));
                    board.SetPuzzle(null, (y, x));
                    board.SetPuzzle(p, (yGrid, x));

                    if (y < height + moveY)
                    {
                        p.gameObject.SetActive(true);
                    }

                    StartCoroutine(p.CoMove(movePos, moveSpeed));
                }
            }
        }

        //foreach (var coroutine in moveRoutine)
        //{
        //    yield return coroutine;
        //}

        destroyHash.Clear();
        //FillBlankBoard(moveY);
    }

    private void FillBlankBoard(int moveY)
    {
        for (int y = 0; y < board.grids.GetLength(0); y++)
        {
            for (int x = 0; x < board.grids.GetLength(1); x++)
            {
                if (board.GetPuzzle((y,x)) == null)
                {
                    Puzzle p = puzzlePool.Get();
                    p.gridNum = (y, x);
                    p.SetPosition(board.GetGridPosition((y, x)));
                    p.gameObject.SetActive(false);
                    board.SetPuzzle(p, (y, x));
                }
            }
        }
    }

    private void SetMatchingPuzzles(Puzzle p1, Puzzle p2, Puzzle p3)
    {
        if (p1 != null && p2 != null && p3 != null)
        {
            if (p1.type == p2.type && p1.type == p3.type)
            {
                p1.isMatched = true;
                p2.isMatched = true;
                p3.isMatched = true;
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
                    newGn = (currGn.Item1, currGn.Item2-1);
                    gridSet = true;
                }
                break;
            case MouseMoveDir.Right:
                if (currGn.Item2 < width-1)
                {
                    newGn = (currGn.Item1, currGn.Item2 + 1);
                    gridSet = true;
                }
                break;
            case MouseMoveDir.Up:
                if (currGn.Item1 < height-1)
                {
                    newGn = (currGn.Item1+1, currGn.Item2);
                    gridSet = true;
                }
                break;
            case MouseMoveDir.Down:
                if (currGn.Item1 > 0)
                {
                    newGn = (currGn.Item1-1, currGn.Item2);
                    gridSet = true;
                }
                break;
            default:
                break;
        }

        if (!gridSet)
            return;

        Puzzle currPuzzle = board.GetPuzzle(currGn);
        Puzzle movePuzzle = board.GetPuzzle(newGn);
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

        moveCo1 = null;
        moveCo2 = null;
        Swap(currPuzzle, movePuzzle, currGn, newGn);
    }

     private void Swap(Puzzle currPuzzle, Puzzle movePuzzle, (int, int) currGn, (int, int) newGn)
    {
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
