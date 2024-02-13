using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Pool;
using UnityEngine.UI;

public class Board : MonoBehaviour
{
    [Header("Background Tile")]
    [SerializeField] private Grid gridPrefab;
    [SerializeField] private GridLayoutGroup gridParents;

    [Header("Puzzle")]
    [SerializeField] private GameObject puzzleParentsObject;
    [SerializeField] private Puzzle puzzlePrefab;
    private IObjectPool<Puzzle> puzzlePool;

    private BoardInfo info;
    private Vector2 puzzleSpriteSize;
    private GameManager gameManager;

    private Row[] rows { get; set; }
    private Grid[,] grids { get; set; } // grids[y,x]

    private List<Task> tasks = new();

    public void Init(int width, int height)
    {
        gameManager = FindAnyObjectByType<GameManager>();

        info.LoadGridsBlockData();
        CreateGrids(gridPrefab, gridParents);
        puzzlePool = new ObjectPool<Puzzle>(
            CreatePuzzle,
            GetPuzzle,
            OnRelease,
            DestroyPuzzle,
            maxSize: width * (height * 3)
            );

        StartCoroutine(CoCreateBoard(width, height));
    }

    public void CreateGrids(Grid backgroundTilePrefab, GridLayoutGroup backgroundParentsObject)
    {
        rows = new Row[info.data.height * 2];
        grids = new Grid[info.data.height * 2, info.data.width];

        for (int y = 0; y < info.data.height * 2; y++)
        {
            rows[y] = new Row();
            for (int x = 0; x < info.data.width; x++)
            {
                Grid grid = Instantiate(backgroundTilePrefab, backgroundParentsObject.transform);
                grid.GridNum = (y, x);
                grid.IsBlocked = info.GetBlockedGrid(x,y);
                grids[y, x] = grid;

                if (y >= info.data.height)
                {
                    grid.gameObject.SetActive(false);
                }
            }
        }
    }

    public Puzzle GetPuzzle(int x, int y)
    {
        if (y >= info.data.height * 2 ||
            x >= info.data.width ||
            y < 0 ||
            x < 0)
            return null;

        return grids[y, x].Puzzle;
    }

    public void SetPuzzle(Puzzle p, int x, int y)
    {
        grids[y, x].Puzzle = p;
    }

    public void SetGridNum(int x, int y)
    {
        grids[y, x].GridNum = (y, x);
    }

    public void SetGridPosition(Vector2 pos, int x, int y)
    {
        grids[y, x].Position = pos;
    }

    public Vector2 GetGridPosition(int x, int y)
    {
        return grids[y, x].Position;
    }

    public (int, int) GetGridNum(int x, int y)
    {
        return grids[y, x].GridNum;
    }

    public bool GetBlocked(int checkX, int yIndex)
    {
        for (int y = yIndex; y < info.data.height; y++)
        {
            if (IsBlocked(checkX, y))
            {
                return true;
            }
        }

        return false;
    }

    private bool IsBlocked(int x, int y)
    {
        return grids[y, x].IsBlocked;
    }

    public Vector2 GetGridSize()
    {
        return gridParents.cellSize;
    }

    public void SetBoardInfo(BoardInfo info)
    {
        this.info = info;
    }

    public BoardInfo GetInfo()
    {
        return info;
    }

    private IEnumerator CoCreateBoard(int width, int height)
    {
        if (width == 0 || height == 0)
            yield break;

        gridParents.constraint = info.GetConstraintType();
        gridParents.constraintCount = info.GetConstaintCount();

        // �� ������, ���� �� ����
        UIManager.Instance.FitToCell(gridParents, width, height);
        Vector2 cellSize = gridParents.cellSize;
        Vector2 spacing = gridParents.spacing;

        yield return null;

        // ù ��ġ
        Vector2 startPosition = gridParents.transform.GetChild(0).GetComponent<RectTransform>().localPosition;

        // X, Y ���� ���
        float objectIntervalX = cellSize.x + spacing.x;
        float objectIntervalY = cellSize.y + spacing.y;

        for (int y = 0; y < height * 2; y++)
        {
            for (int x = 0; x < width; x++)
            {
                // y�� x�� �þ �� ���� �� ���ؼ� ��ġ ����
                Vector2 pos = startPosition;
                if (y > 0)
                {
                    pos.y += objectIntervalY * y;
                }
                if (x > 0)
                {
                    pos.x += objectIntervalX * x;
                }

                SetGridPosition(pos, x, y);
            }
        }

        puzzleParentsObject.transform.localPosition = gridParents.transform.localPosition;
        StartCoroutine(CoCreateAllPuzzles(cellSize, width, height));
    }

    // Create all puzzles
    private IEnumerator CoCreateAllPuzzles(Vector2 size, int width, int height)
    {
        for (int i = 0; i < height * 2; i++)
        {
            rows[i].CreateRowPuzzle(puzzlePool, this, info, size, i, width, height);
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
        Vector2 position = p.GetPosition();
        PuzzleType type = p.type;

        EffectManager.Instance.GetEffect(type, position);

        p.isConnected = false;
        p.isMatched = false;
        (int, int) gn = p.GridNum;

        int x = gn.Item2;
        int y = gn.Item1;
        SetPuzzle(null, x, y);
        p.gameObject.SetActive(false);
    }

    private void DestroyPuzzle(Puzzle p)
    {
        Destroy(p.gameObject);
    }

    /*--------------------------------------------------------- Move -------------------------------------------------------------------*/

    public float moveTime;
    private Vector2 clickStartPos;
    private Vector2 currMousePos;
    private Puzzle clickedPuzzle;
    private bool isMoved = false;
    private readonly HashSet<Puzzle> destroyHash = new();
    private bool moveAsyncRunning = false;
    private MouseMoveDir saveDir;

    private bool allowClick = true;
    private int clickCount = 0;
    private float playTime = 0;

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
        playTime += Time.deltaTime;
    }

    private void CheckThreeMatchPuzzle()
    {
        if (destroyHash.Count > 0)
            return;

        for (int y = 0; y < info.data.height; y++)
        {
            for (int x = 0; x < info.data.width; x++)
            {
                Puzzle p1 = GetPuzzle(x, y);
                Puzzle p2 = null;
                Puzzle p3 = null;

                if (p1.type == PuzzleType.Blocked)
                    continue;

                if (x < info.data.width - 2)
                {
                    p2 = GetPuzzle(x + 1, y);
                    p3 = GetPuzzle(x + 2, y);

                    SetMatchingPuzzles(p1, p2, p3);
                }

                if (y < info.data.height - 2)
                {
                    p2 = GetPuzzle(x, y + 1);
                    p3 = GetPuzzle(x, y + 2);

                    SetMatchingPuzzles(p1, p2, p3);
                }
            }
        }

        for (int y = 0; y < info.data.height; y++)
        {
            for (int x = 0; x < info.data.width; x++)
            {
                Puzzle p = GetPuzzle(x, y);
                if (p == null)
                    continue;

                if (p.type == PuzzleType.Blocked)
                    continue;

                if (p.isMatched)
                {
                    if (destroyHash.Contains(p) || destroyHash.Equals(p))
                    {
                        continue;
                    }

                    destroyHash.Add(p);
                }
            }
        }

        MoveAndFillAsync();
    }

    private async void MoveAndFillAsync()
    {
        if (destroyHash.Count == 0 || moveAsyncRunning)
            return;

        moveAsyncRunning = true;

        int addScore = 0;

        int maxX = 0;
        int maxY = 0;
        int minX = info.data.width;
        int minY = info.data.height;
        foreach (var p in destroyHash)
        {
            if (!p.gameObject.activeSelf)
                continue;

            int y = p.GridNum.Item1;
            int x = p.GridNum.Item2;
            minY = minY > y ? y : minY;
            maxY = maxY < y ? y : maxY;
            minX = minX > x ? x : minX;
            maxX = maxX < x ? x : maxX;

            addScore += p.scoreNum;
            puzzlePool.Release(p);
        }

        gameManager.puzzleSceneHolder.score.AddScore(addScore);
        SoundManager.Instance.PlayExplodingSound();

        if (!gameManager.GetStageInfo().data.isInfinityMode)
        {
            if (gameManager.puzzleSceneHolder.score.GetScore() >= gameManager.GetStageInfo().data.clearScore)
            {
                gameManager.puzzleSceneHolder.clearUI.OnActive();
                gameManager.puzzleSceneHolder.clearUI.SetClickCountText(clickCount);
                gameManager.puzzleSceneHolder.clearUI.SetClearTimeText((int)playTime);

                float maxTime = gameManager.GetStageInfo().data.maxPlayTime;
                float normalizeTime = (maxTime - playTime) / maxTime;
                int starCount = (int)Mathf.Round(normalizeTime * 3f);
                gameManager.puzzleSceneHolder.clearUI.StartFillStars(starCount);
                return;
            }
        }

        await MoveDownAsync(maxY, minY, minX, maxX);
    }

    private async Task MoveDownAsync(int maxY, int minY, int minX, int maxX)
    {
        for (int y = minY; y < grids.GetLength(0); y++)
        {
            for (int x = minX; x <= maxX; x++)
            {
                Puzzle p = GetPuzzle(x, y);
                if (p == null)  // ���� ��ġ�� ���� ���� ��
                {
                    if (GetBlocked(x, y))  // ���� ��ġ(x,y)���� ���� ���尡 �������� ��
                    {
                        // ���� ��ġ�� ���� ���� ����
                        var createPuzzle = puzzlePool.Get();
                        createPuzzle.gameObject.SetActive(true);
                        createPuzzle.GridNum = GetGridNum(x, y);

                        SetPuzzle(createPuzzle, x, y);
                        Vector2 pos = GetGridPosition(x, y);
                        createPuzzle.SetPosition(pos);

                        Vector2 size = gridParents.cellSize;
                        tasks.Add(createPuzzle.Expands(size - size/4, moveTime));
                    }

                    continue;
                }

                // ���� y�� ���� �Ʒ��� ����ִ� y ã��
                int yGrid = 0;
                for (int k = y-1; k >= 0; k--)
                {
                    Puzzle findPuzzle = GetPuzzle(x, k);
                    if (findPuzzle == null)
                    {
                        yGrid = k;
                    }
                    else
                    {
                        break;
                    }
                }

                if (GetPuzzle(x, yGrid) == null)
                {
                    Vector2 movePos = GetGridPosition(x, yGrid);

                    p.GridNum = GetGridNum(x, yGrid);
                    SetPuzzle(null, x, y);
                    SetPuzzle(p, x, yGrid);

                    if (yGrid < info.data.height)
                    {
                        p.gameObject.SetActive(true);
                    }

                    tasks.Add(p.Move(movePos, moveTime));
                }
            }
        }

        await Task.WhenAll(tasks);

        FillBlankBoard(maxY, minX, maxX);
        destroyHash.Clear();
        moveAsyncRunning = false;

        CheckThreeMatchPuzzle();
        if (destroyHash.Count == 0)
        {
            allowClick = true;
        }
    }

    private void FillBlankBoard(int startY, int startX, int endX)
    {
        for (int y = startY; y < grids.GetLength(0); y++)
        {
            for (int x = startX; x <= endX; x++)
            {
                if (GetPuzzle(x, y) == null)
                {
                    Puzzle p = puzzlePool.Get();
                    p.GridNum = GetGridNum(x, y);
                    p.SetPosition(GetGridPosition(x, y));
                    SetPuzzle(p, x, y);

                    if (y > info.data.height)
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
            if (p1.type == PuzzleType.None || p2.type == PuzzleType.None || p3.type == PuzzleType.None)
                return;

            if (p1.type == p2.type && p1.type == p3.type)
            {
                p1.isMatched = true;
                p2.isMatched = true;
                p3.isMatched = true;
            }
        }
    }

    public void SetClickedPuzzle(Puzzle p)
    {
        if (p.type != PuzzleType.Blocked && allowClick)
        {
            clickedPuzzle = p;
            clickStartPos = Input.mousePosition;
            clickCount++;
        }
    }

    public void Swap()
    {
        if (!isMoved && allowClick)
        {
            currMousePos = Input.mousePosition;
            Vector2 moveDir = currMousePos - clickStartPos;
            MouseMoveDir dir = CalcMouseMoveDirection(moveDir);
            saveDir = dir;

            SwapPuzzles(dir);
        }
    }

    private void SwapPuzzles(MouseMoveDir dir)
    {
        if (dir == MouseMoveDir.None || clickedPuzzle == null)
            return;

        if (clickedPuzzle.type == PuzzleType.Blocked)
            return;

        allowClick = false;
        isMoved = true;
        (int, int) currGn = clickedPuzzle.GridNum;
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
                if (currGn.Item2 < info.data.width - 1)
                {
                    newGn = (currGn.Item1, currGn.Item2 + 1);
                    gridSet = true;
                }
                break;
            case MouseMoveDir.Up:
                if (currGn.Item1 < info.data.height - 1)
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

        Puzzle currPuzzle = GetPuzzle(currGn.Item2, currGn.Item1);
        Puzzle movePuzzle = GetPuzzle(newGn.Item2, newGn.Item1);

        if (currPuzzle == null || movePuzzle == null || movePuzzle.type == PuzzleType.Blocked)
        {
            allowClick = true;
            isMoved = false;
            return;
        }

        Vector2 currPos = currPuzzle.transform.localPosition;
        Vector2 movePos = movePuzzle.transform.localPosition;
        MovePuzzlesAsync(currPuzzle, movePuzzle, currPos, movePos, moveTime, currGn, newGn);
    }

    private async void MovePuzzlesAsync(Puzzle currPuzzle, Puzzle movePuzzle, Vector2 currPos, Vector2 movePos, float moveTime, (int, int) currGn, (int, int) newGn)
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

            CheckThreeMatchPuzzle();
            if (destroyHash.Count == 0 && saveDir != MouseMoveDir.None)
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

                SwapPuzzles(dir);
                saveDir = MouseMoveDir.None;

                int subScore = 0;
                subScore += currPuzzle.scoreNum;
                subScore += movePuzzle.scoreNum;
                gameManager.puzzleSceneHolder.score.SubScore(subScore);
            }
            else
            {
                if (destroyHash.Count == 0)
                    allowClick = true;

                clickedPuzzle = null;
                isMoved = false;
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"Error in MovePuzzles : {ex.Message}\n{ex.StackTrace}");
        }
    }

    private void Swap(Puzzle currPuzzle, Puzzle movePuzzle, (int, int) currGn, (int, int) newGn)
    {
        currPuzzle.GridNum = newGn;
        movePuzzle.GridNum = currGn;

        SetPuzzle(currPuzzle, newGn.Item2, newGn.Item1);
        SetPuzzle(movePuzzle, currGn.Item2, currGn.Item1);
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

    public void Mix()
    {
        for (int y = 0; y < info.data.height; y++)
        {
            for (int x = 0; x < info.data.width; x++)
            {
                Puzzle puzzle = GetPuzzle(x, y);

                if (puzzle != null && puzzle.type != PuzzleType.Blocked)
                    puzzle.SetRandomPuzzleType();
            }
        }

        CheckThreeMatchPuzzle();
    }

    public void StopTask()
    {
        DOTween.KillAll();
    }
}
