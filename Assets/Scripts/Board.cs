using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Pool;

public class Board : MonoBehaviour
{
    [SerializeField] private GameObject backgroundTilePrefab;
    [SerializeField] private Puzzle puzzlePrefab;
    private IObjectPool<Puzzle> puzzlePool;
    private Vector2 puzzleSpriteSize;
    public int width;
    public int height;

    public Grid[,] grids;
    public Row[] rows;
    public int boardSize;

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

        boardSize = width * height;

        Texture2D puzzle = puzzlePrefab.GetComponent<SpriteRenderer>().sprite.texture;
        puzzleSpriteSize = new Vector2(puzzle.width, puzzle.height);
        puzzlePool = new ObjectPool<Puzzle>(
            CreatePuzzle,
            GetPuzzle,
            OnRelease,
            DestroyPuzzle,
            maxSize: width * (height * 3)
            );

        CreateBackgroundTiles(width, height);
        StartCoroutine(CoCreateAllPuzzles(width, height));
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
                SetGridPosition(tilePosition, (i, j));
            }
        }
    }

    private Vector2 GetSpriteBounds(SpriteRenderer sprite)
    {
        float width = sprite.bounds.size.x;
        float height = sprite.bounds.size.y;

        return new Vector2(width, height);
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
        Puzzle p = Instantiate(puzzlePrefab);
        p.SetRandomPuzzleType();
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

        SetPuzzle(null, (gn.Item1, gn.Item2));
        p.gameObject.SetActive(false);
    }

    private void DestroyPuzzle(Puzzle p)
    {
        Destroy(p.gameObject);
    }

    public Puzzle GetPuzzle((int, int) gridNum)
    {
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

    public void SetGridPosition(Vector3 pos, (int, int) gridNum)
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

    public float moveSpeed = 3f;
    private Vector2 clickStartPos;
    private Vector2 currMousePos;
    private Puzzle clickedPuzzle;
    private bool isMoved = false;

    private HashSet<Puzzle> destroyHash = new();

    private bool moveAsyncRunning = false;

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

        int moveY = (maxY - minY) + 1;
        await MoveDownAsync(minY, moveY, minX, maxX);
    }

    private async Task MoveDownAsync(int minY, int moveY, int minX, int maxX)
    {
        List<Task> moveTasks = new();
        for (int y = minY; y < grids.GetLength(0); y++)
        {
            for (int x = minX; x <= maxX; x++)
            {
                int yGrid = y - moveY < 0 ? 0 : y - moveY;
                Puzzle p = GetPuzzle((y, x));
                Puzzle moveP = GetPuzzle((yGrid, x));

                if (p != null && moveP == null)
                {
                    Vector2 movePos = GetGridPosition((yGrid, x));
                    p.SetGridNum(GetGridNum((yGrid, x)));
                    SetPuzzle(null, (y, x));
                    SetPuzzle(p, (yGrid, x));

                    if (y < height + moveY)
                    {
                        p.gameObject.SetActive(true);
                    }

                    moveTasks.Add(p.Move(movePos, moveSpeed));
                }
            }
        }

        await Task.WhenAll(moveTasks);

        //FillBlankBoard(moveY);
        destroyHash.Clear();
        moveAsyncRunning = false;
    }

    private void FillBlankBoard(int moveY)
    {
        for (int y = 0; y < grids.GetLength(0); y++)
        {
            for (int x = 0; x < grids.GetLength(1); x++)
            {
                if (GetPuzzle((y, x)) == null)
                {
                    Puzzle p = puzzlePool.Get();
                    p.gridNum = (y, x);
                    p.SetPosition(GetGridPosition((y, x)));
                    p.gameObject.SetActive(false);
                    SetPuzzle(p, (y, x));
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
        Vector2 currPos = currPuzzle.transform.position;
        Vector2 movePos = movePuzzle.transform.position;

        _ = MovePuzzlesAsync(currPuzzle, movePuzzle, currPos, movePos, moveSpeed, currGn, newGn);
    }

    private async Task MovePuzzlesAsync(Puzzle currPuzzle, Puzzle movePuzzle, Vector2 currPos, Vector2 movePos, float moveSpeed, (int, int) currGn, (int, int) newGn)
    {
        try
        {
            List<Task> taskList = new()
            {
                currPuzzle.Move(movePos, moveSpeed),
                movePuzzle.Move(currPos, moveSpeed)
            };

            await Task.WhenAll(taskList);
            Swap(currPuzzle, movePuzzle, currGn, newGn);
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
