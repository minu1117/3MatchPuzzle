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
    [SerializeField] private Grid backgroundTilePrefab;
    [SerializeField] private GridLayoutGroup backgroundParentsObject;

    [Header("Puzzle")]
    [SerializeField] private GameObject puzzleParentsObject;
    [SerializeField] private Puzzle puzzlePrefab;
    private IObjectPool<Puzzle> puzzlePool;

    private BoardInfo info;
    private Vector2 puzzleSpriteSize;
    private GameManager gameManager;

    private List<Task> tasks = new();

    public void Init(int width, int height)
    {
        gameManager = FindAnyObjectByType<GameManager>();

        info.CreateGrids(backgroundTilePrefab, backgroundParentsObject);
        info.LoadGridsBlockData();
        puzzlePool = new ObjectPool<Puzzle>(
            CreatePuzzle,
            GetPuzzle,
            OnRelease,
            DestroyPuzzle,
            maxSize: width * (height * 3)
            );

        StartCoroutine(CoCreateBoard(width, height));
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

        backgroundParentsObject.constraint = info.GetConstraintType();
        backgroundParentsObject.constraintCount = info.GetConstaintCount();

        // 셀 사이즈, 간격 값 조정
        UIManager.Instance.FitToCell(backgroundParentsObject, width, height);
        Vector2 cellSize = backgroundParentsObject.cellSize;
        Vector2 spacing = backgroundParentsObject.spacing;

        yield return null;

        // 첫 위치
        Vector2 startPosition = backgroundParentsObject.transform.GetChild(0).GetComponent<RectTransform>().localPosition;

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

                info.SetGridPosition(pos, x, y);
            }
        }

        puzzleParentsObject.transform.localPosition = backgroundParentsObject.transform.localPosition;
        StartCoroutine(CoCreateAllPuzzles(cellSize, width, height));
    }

    // Create all puzzles
    private IEnumerator CoCreateAllPuzzles(Vector2 size, int width, int height)
    {
        for (int i = 0; i < height * 2; i++)
        {
            info.rows[i].CreateRowPuzzle(puzzlePool, info, size, i, width, height);
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
        info.SetPuzzle(null, x, y);
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
        MoveAndFillAsync();
    }

    private void CheckThreeMatchPuzzle()
    {
        if (destroyHash.Count > 0)
            return;

        for (int y = 0; y < info.height; y++)
        {
            for (int x = 0; x < info.width; x++)
            {
                Puzzle p1 = info.GetPuzzle(x, y);
                Puzzle p2 = null;
                Puzzle p3 = null;

                if (p1.type == PuzzleType.Blocked)
                    continue;

                if (x < info.width - 2)
                {
                    p2 = info.GetPuzzle(x + 1, y);
                    p3 = info.GetPuzzle(x + 2, y);

                    SetMatchingPuzzles(p1, p2, p3);
                }

                if (y < info.height - 2)
                {
                    p2 = info.GetPuzzle(x, y + 1);
                    p3 = info.GetPuzzle(x, y + 2);

                    SetMatchingPuzzles(p1, p2, p3);
                }
            }
        }

        for (int y = 0; y < info.height; y++)
        {
            for (int x = 0; x < info.width; x++)
            {
                Puzzle p = info.GetPuzzle(x, y);
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
    }

    private async void MoveAndFillAsync()
    {
        if (destroyHash.Count == 0 || moveAsyncRunning)
            return;

        moveAsyncRunning = true;

        int addScore = 0;

        int maxX = 0;
        int maxY = 0;
        int minX = info.width;
        int minY = info.height;
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

        if (!gameManager.GetStageInfo().isInfinityMode)
        {
            if (gameManager.puzzleSceneHolder.score.GetScore() >= gameManager.GetStageInfo().clearScore)
            {
                gameManager.puzzleSceneHolder.clearUI.OnActive();
                gameManager.puzzleSceneHolder.clearUI.SetClickCountText(clickCount);
                gameManager.puzzleSceneHolder.clearUI.SetClearTimeText((int)playTime);

                float maxTime = gameManager.GetStageInfo().maxPlayTime;
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
        for (int y = minY; y < info.grids.GetLength(0); y++)
        {
            for (int x = minX; x <= maxX; x++)
            {
                Puzzle p = info.GetPuzzle(x, y);
                if (p == null)  // 현재 위치에 퍼즐 없을 때
                {
                    if (info.GetBlocked(x, y))  // 현재 위치(x,y)보다 위에 보드가 막혀있을 때
                    {
                        // 현재 위치에 퍼즐 새로 생성
                        var createPuzzle = puzzlePool.Get();
                        createPuzzle.gameObject.SetActive(true);
                        createPuzzle.GridNum = info.GetGridNum(x, y);

                        info.SetPuzzle(createPuzzle, x, y);
                        Vector2 pos = info.GetGridPosition(x, y);
                        createPuzzle.SetPosition(pos);

                        Vector2 size = backgroundParentsObject.cellSize;
                        tasks.Add(createPuzzle.Expands(size, moveTime));
                    }

                    continue;
                }

                // 현재 y의 가장 아래에 비어있는 y 찾기
                int yGrid = 0;
                for (int k = y-1; k >= 0; k--)
                {
                    Puzzle findPuzzle = info.GetPuzzle(x, k);
                    if (findPuzzle == null)
                    {
                        yGrid = k;
                    }
                    else
                    {
                        break;
                    }
                }

                if (info.GetPuzzle(x, yGrid) == null)
                {
                    Vector2 movePos = info.GetGridPosition(x, yGrid);

                    p.GridNum = info.GetGridNum(x, yGrid);
                    info.SetPuzzle(null, x, y);
                    info.SetPuzzle(p, x, yGrid);

                    if (yGrid < info.height)
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
        for (int y = startY; y < info.grids.GetLength(0); y++)
        {
            for (int x = startX; x <= endX; x++)
            {
                if (info.GetPuzzle(x, y) == null)
                {
                    Puzzle p = puzzlePool.Get();
                    p.GridNum = info.GetGridNum(x, y);
                    p.SetPosition(info.GetGridPosition(x, y));
                    info.SetPuzzle(p, x, y);

                    if (y > info.height)
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
                if (currGn.Item2 < info.width - 1)
                {
                    newGn = (currGn.Item1, currGn.Item2 + 1);
                    gridSet = true;
                }
                break;
            case MouseMoveDir.Up:
                if (currGn.Item1 < info.height - 1)
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

        Puzzle currPuzzle = info.GetPuzzle(currGn.Item2, currGn.Item1);
        Puzzle movePuzzle = info.GetPuzzle(newGn.Item2, newGn.Item1);

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

        info.SetPuzzle(currPuzzle, newGn.Item2, newGn.Item1);
        info.SetPuzzle(movePuzzle, currGn.Item2, currGn.Item1);
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
        for (int y = 0; y < info.height; y++)
        {
            for (int x = 0; x < info.width; x++)
            {
                Puzzle puzzle = info.GetPuzzle(x, y);

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
