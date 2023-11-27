using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Pool;
using UnityEngine.UI;

public class Board : MonoBehaviour
{
    [SerializeField] private GameObject backgroundTilePrefab;
    [SerializeField] private GridLayoutGroup backgroundParentsObject;
    [SerializeField] private GameObject puzzleParentsObject;
    [SerializeField] private Puzzle puzzlePrefab;
    private IObjectPool<Puzzle> puzzlePool;

    private Row[] rows;
    private Grid[,] grids;

    private Vector2 puzzleSpriteSize;
    public int width;
    public int height;

    public void Start()
    {
        Init();
    }

    public void Init()
    {
        rows = new Row[height * 2];
        grids = new Grid[height * 2, width];
        for (int h = 0; h < height * 2; h++)
        {
            rows[h] = new Row();
            for (int w = 0; w < width; w++)
            {
                grids[h, w] = new Grid();
            }
        }

        puzzlePool = new ObjectPool<Puzzle>(
            CreatePuzzle,
            GetPuzzle,
            OnRelease,
            DestroyPuzzle,
            maxSize: width * (height * 3)
            );

        StartCoroutine(CreateBackgroundTiles(width, height));
    }

    private IEnumerator CreateBackgroundTiles(int width, int height)
    {
        if (width == 0 || height == 0)
            yield break;

        for (int i = 0; i < height * 2; i++)
        {
            for (int j = 0; j < width; j++)
            {
                if (i < height)
                {
                    // GridLayout Group 내부에 Instantiate (위치 자동 지정)
                    Instantiate(backgroundTilePrefab, backgroundParentsObject.transform);
                }

                yield return null;
            }
        }

        // 첫 위치, 셀 사이즈, 간격 값
        Vector2 startPosition = backgroundParentsObject.transform.GetChild(0).GetComponent<RectTransform>().localPosition;
        Vector2 cellSize = backgroundParentsObject.cellSize;
        Vector2 spacing = backgroundParentsObject.spacing;

        // X, Y 간격 계산
        float objectIntervalX = cellSize.x + spacing.x;
        float objectIntervalY = cellSize.y + spacing.y;

        for (int y = 0; y < height * 2; y++)
        {
            for (int x = 0; x < width; x++)
            {
                // y와 x가 늘어날 때 마다 값 더해서 위치 선정
                Vector2 pos = startPosition;
                if (y > 0)
                {
                    pos.y += objectIntervalY * y;
                }
                if (x > 0)
                {
                    pos.x += objectIntervalX * x;
                }

                SetGridPosition(pos, (y,x));
            }
        }

        StartCoroutine(CoCreateAllPuzzles(width, height));
    }

    // Create all puzzles
    private IEnumerator CoCreateAllPuzzles(int width, int height)
    {
        for (int i = 0; i < height * 2; i++)
        {
            rows[i].CreateRowPuzzle(puzzlePool, this, i, width, height);
            yield return null;
        }
    }

    // Create one puzzle
    private Puzzle CreatePuzzle()
    {
        Puzzle p = Instantiate(puzzlePrefab, puzzleParentsObject.transform);
        p.SetRandomPuzzleType();
        p.AddPointerDownEventTrigger(SetClickedPuzzle);
        p.AddDragEventTrigger(Swap);
        return p;
    }

    private void GetPuzzle(Puzzle p)
    {
        p.SetRandomPuzzleType();
    }

    private void OnRelease(Puzzle p)
    {
        p.isConnected = false;
        p.isMatched = false;
        (int, int) gn = p.gridNum;

        SetPuzzle(null, (gn.Item1, gn.Item2));
        p.gameObject.SetActive(false);
    }

    private void DestroyPuzzle(Puzzle p)
    {
        Destroy(p.gameObject);
    }

    public Puzzle GetPuzzle((int, int) gridNum)
    {
        if (gridNum.Item1 >= height * 2 ||
            gridNum.Item2 >= width ||
            gridNum.Item1 < 0 ||
            gridNum.Item2 < 0)
            return null;

        return grids[gridNum.Item1, gridNum.Item2].Puzzle;
    }

    public void SetPuzzle(Puzzle p, (int, int) gridNum)
    {
        grids[gridNum.Item1, gridNum.Item2].Puzzle = p;
    }

    public void SetGridNum((int, int) gridNum)
    {
        grids[gridNum.Item1, gridNum.Item2].GridNum = gridNum;
    }

    public void SetGridPosition(Vector2 pos, (int, int) gridNum)
    {
        grids[gridNum.Item1, gridNum.Item2].Position = pos;
    }

    public Vector2 GetGridPosition((int, int) gridNum)
    {
        return grids[gridNum.Item1, gridNum.Item2].Position;
    }

    public (int, int) GetGridNum((int, int) gn)
    {
        return grids[gn.Item1, gn.Item2].GridNum;
    }

    /*--------------------------------------------------------- Move -------------------------------------------------------------------*/

    public float moveTime;
    private Vector2 clickStartPos;
    private Vector2 currMousePos;
    private Puzzle clickedPuzzle;
    private bool isMoved = false;
    private HashSet<Puzzle> destroyHash = new();
    private bool moveAsyncRunning = false;
    private MouseMoveDir saveDir;

    private enum MouseMoveDir
    {
        None    = -1,

        Left    = 0,
        Right   = 1,
        Up      = 2,
        Down    = 3,

        Count,
    }

    public void Update()
    {
        if (destroyHash.Count == 0)
        {
            CheakThreeMatchPuzzle();
        }

        if (destroyHash.Count > 0 && !moveAsyncRunning)
        {
            MoveAndFillAsync();
        }
    }

    private void CheakThreeMatchPuzzle()
    {
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                Puzzle p1 = GetPuzzle((y, x));
                Puzzle p2 = null;
                Puzzle p3 = null;

                if (x < width - 2)
                {
                    p2 = GetPuzzle((y, x + 1));
                    p3 = GetPuzzle((y, x + 2));

                    SetMatchingPuzzles(p1, p2, p3);
                }

                if (y < height - 2)
                {
                    p2 = GetPuzzle((y + 1, x));
                    p3 = GetPuzzle((y + 2, x));

                    SetMatchingPuzzles(p1, p2, p3);
                }
            }
        }

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                Puzzle p = GetPuzzle((y, x));
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

    private async void MoveAndFillAsync()
    {
        moveAsyncRunning = true;

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

        await MoveDownAsync(maxY, minY, minX, maxX);
    }

    private async Task MoveDownAsync(int maxY, int minY, int minX, int maxX)
    {
        int moveY = (maxY - minY) + 1;
        List<Task> moveTasks = new();

        for (int y = minY; y < grids.GetLength(0); y++)
        {
            for (int x = minX; x <= maxX; x++)
            {
                Puzzle p = GetPuzzle((y, x));
                if (p == null) continue;

                // 현재 y의 가장 아래에 비어있는 y 찾기
                int yGrid = 0;
                for (int k = y-1; k >= 0; k--)
                {
                    if (GetPuzzle((k, x)) == null) 
                        yGrid = k;
                    else 
                        break;
                }

                if (GetPuzzle((yGrid, x)) == null)
                {
                    Vector2 movePos = GetGridPosition((yGrid, x));

                    p.SetGridNum(GetGridNum((yGrid, x)));
                    SetPuzzle(null, (y, x));
                    SetPuzzle(p, (yGrid, x));

                    if (yGrid < height)
                    {
                        p.gameObject.SetActive(true);
                    }

                    moveTasks.Add(p.Move(movePos, moveTime));
                }
            }
        }

        await Task.WhenAll(moveTasks);

        FillBlankBoard(maxY, minX, maxX);
        destroyHash.Clear();
        moveAsyncRunning = false;
    }

    private void FillBlankBoard(int startY, int startX, int endX)
    {
        for (int y = startY; y < grids.GetLength(0); y++)
        {
            for (int x = startX; x <= endX; x++)
            {
                if (GetPuzzle((y, x)) == null)
                {
                    Puzzle p = puzzlePool.Get();
                    p.SetGridNum((y, x));
                    p.SetPosition(GetGridPosition((y, x)));
                    SetPuzzle(p, (y, x));

                    if (y > height)
                    {
                        p.gameObject.SetActive(false);
                    }
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

    public Puzzle GetClickedPuzzle(Vector2 worldPos)
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

    public void SetClickedPuzzle(Puzzle p)
    {
        clickedPuzzle = p;
        clickStartPos = Input.mousePosition;
        isMoved = false;
    }

    public void Swap()
    {
        currMousePos = Input.mousePosition;
        Vector2 moveDir = currMousePos - clickStartPos;
        MouseMoveDir dir = CalcMouseMoveDirection(moveDir);
        saveDir = dir;

        if (!isMoved)
        {
            if (dir != MouseMoveDir.None && clickedPuzzle != null)
            {
                Debug.Log(dir.ToString());
                SwapPuzzles(dir);
                isMoved = true;
            }
        }
    }

    private void SwapPuzzles(MouseMoveDir dir)
    {
        if (dir == MouseMoveDir.None) return;

        (int, int) currGn = clickedPuzzle.gridNum;
        (int, int) newGn = (0, 0);

        bool gridSet = false;
        switch (dir)
        {
            case MouseMoveDir.Left:
                if (currGn.Item2 > 0)
                {
                    newGn = (currGn.Item1, currGn.Item2 - 1);
                    gridSet = true;
                }
                break;
            case MouseMoveDir.Right:
                if (currGn.Item2 < width - 1)
                {
                    newGn = (currGn.Item1, currGn.Item2 + 1);
                    gridSet = true;
                }
                break;
            case MouseMoveDir.Up:
                if (currGn.Item1 < height - 1)
                {
                    newGn = (currGn.Item1 + 1, currGn.Item2);
                    gridSet = true;
                }
                break;
            case MouseMoveDir.Down:
                if (currGn.Item1 > 0)
                {
                    newGn = (currGn.Item1 - 1, currGn.Item2);
                    gridSet = true;
                }
                break;
            default:
                break;
        }

        if (!gridSet)
            return;

        Puzzle currPuzzle = GetPuzzle(currGn);
        Puzzle movePuzzle = GetPuzzle(newGn);
        Vector2 currPos = currPuzzle.transform.localPosition;
        Vector2 movePos = movePuzzle.transform.localPosition;

        _ = MovePuzzlesAsync(currPuzzle, movePuzzle, currPos, movePos, moveTime, currGn, newGn);
    }

    private async Task MovePuzzlesAsync(Puzzle currPuzzle, Puzzle movePuzzle, Vector2 currPos, Vector2 movePos, float moveTime, (int, int) currGn, (int, int) newGn)
    {
        try
        {
            List<Task> taskList = new()
            {
                currPuzzle.Move(movePos, moveTime),
                movePuzzle.Move(currPos, moveTime)
            };

            await Task.WhenAll(taskList);
            Swap(currPuzzle, movePuzzle, currGn, newGn);

            CheakThreeMatchPuzzle();
            if (destroyHash.Count == 0 && isMoved)
            {
                var dir = MouseMoveDir.None;
                switch (saveDir)
                {
                    case MouseMoveDir.Left:
                        dir = MouseMoveDir.Right;
                        break;
                    case MouseMoveDir.Right:
                        dir = MouseMoveDir.Left;
                        break;
                    case MouseMoveDir.Up:
                        dir = MouseMoveDir.Down;
                        break;
                    case MouseMoveDir.Down:
                        dir = MouseMoveDir.Up;
                        break;
                }

                isMoved = false;
                SwapPuzzles(dir);
                clickedPuzzle = null;
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"Error in MovePuzzles : {ex.Message}\n{ex.StackTrace}");
        }
    }

    private void Swap(Puzzle currPuzzle, Puzzle movePuzzle, (int, int) currGn, (int, int) newGn)
    {
        currPuzzle.SetGridNum(newGn);
        movePuzzle.SetGridNum(currGn);

        SetPuzzle(currPuzzle, newGn);
        SetPuzzle(movePuzzle, currGn);
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
