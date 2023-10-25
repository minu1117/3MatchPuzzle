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

            SetNotDuplicationPuzzleType(pz, lp, bp);
            SetNotDuplicationPuzzleType(pz, bp, lp);

            pz.gameObject.transform.position = board[rowIndex, j].transform.position;
            pz.gridNum = (rowIndex, j);
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
                    SwapPuzzle(dir);
                    isMoved = true;
                }
            }
        }

        // Test
        if (Input.GetKeyDown(KeyCode.A))
        {
            foreach (var puzzle in puzzles)
            {
                Destroy(puzzle.gameObject);
            }
        }
        if (Input.GetKeyDown(KeyCode.S))
        {
            StartCoroutine(CoCreateAllPuzzles(width, height));
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

    private void SwapPuzzle(MouseMoveDir dir)
    {
        (int, int) currGn = clickedPuzzle.gridNum;
        (int, int) newGn = (0, 0);

        switch (dir)
        {
            case MouseMoveDir.Left:
                if (currGn.Item2 > 0)
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
                if (currGn.Item1 > 0)
                {
                    newGn = puzzles[currGn.Item1 - 1, currGn.Item2].gridNum;
                }
                else
                    return;
                break;
            default:
                break;
        }

        Puzzle currPuzzle = puzzles[currGn.Item1, currGn.Item2];
        Puzzle movePuzzle = puzzles[newGn.Item1, newGn.Item2];

        Vector2 currPos = board[currGn.Item1, currGn.Item2].transform.position;
        Vector2 movePos = board[newGn.Item1, newGn.Item2].transform.position;

        currPuzzle.SetGridNum(newGn);
        movePuzzle.SetGridNum(currGn);

        // Swap
        Puzzle tempPuzzle = puzzles[currGn.Item1, currGn.Item2];
        puzzles[currGn.Item1, currGn.Item2] = puzzles[newGn.Item1, newGn.Item2];
        puzzles[newGn.Item1, newGn.Item2] = tempPuzzle;

        StartCoroutine(currPuzzle.CoMove(movePos, moveSpeed));
        StartCoroutine(movePuzzle.CoMove(currPos, moveSpeed));

        //List<Puzzle> puzzleList = new();
        //List<(int, int)> grids = new();
        //GetPeripheralPuzzles(ref puzzleList, ref grids, currPuzzle);
        //CheakPeripheralPuzzlesType(currPuzzle, puzzleList);
    }

    //private void GetPeripheralPuzzles(ref List<Puzzle> puzzleList, ref List<(int, int)> grids, Puzzle mainPuzzle)
    //{
    //    (int, int) gn = mainPuzzle.gridNum;

    //    if (gn.Item2 > 0)
    //    {
    //        grids.Add((gn.Item1, gn.Item2 - 1));
    //        GetPeripheralPuzzles(ref puzzleList, ref grids, puzzles[gn.Item1, gn.Item2 - 1]);
    //    }
    //    if (gn.Item2 < width - 1)
    //    {
    //        grids.Add((gn.Item1, gn.Item2 + 1));
    //        GetPeripheralPuzzles(ref puzzleList, ref grids, puzzles[gn.Item1, gn.Item2 + 1]);
    //    }
    //    if (gn.Item1 < height - 1)
    //    {
    //        grids.Add((gn.Item1 + 1, gn.Item2));
    //        GetPeripheralPuzzles(ref puzzleList, ref grids, puzzles[gn.Item1 + 1, gn.Item2]);
    //    }
    //    if (gn.Item1 > 0)
    //    {
    //        grids.Add((gn.Item1 - 1, gn.Item2));
    //        GetPeripheralPuzzles(ref puzzleList, ref grids, puzzles[gn.Item1 - 1, gn.Item2]);
    //    }

    //    for (int i = 0; i < grids.Count; i++)
    //    {
    //        (int, int) grid = grids[i];
    //        puzzleList.Add(puzzles[grid.Item1, grid.Item2]);
    //    }
    //}

    //private void CheakPeripheralPuzzlesType(Puzzle mainPuzzle, List<Puzzle> peripheralPuzzles)
    //{
    //    int matchCount = 0;
    //    for (int i = 0; i < peripheralPuzzles.Count; i++) 
    //    {
    //        if (mainPuzzle.type == peripheralPuzzles[i].type)
    //        {
    //            mainPuzzle.isConnected = true;
    //            peripheralPuzzles[i].isConnected = true;
    //            matchCount++;
    //        }
    //    }

    //    if (matchCount == 0)
    //    {
    //        mainPuzzle.isConnected = false;
    //        return;
    //    }
    //    else if (matchCount >= 3)
    //    {
    //        Destroy(mainPuzzle.gameObject);
    //        for (int i = peripheralPuzzles.Count - 1; i >= 0; i--)
    //        {
    //            if (peripheralPuzzles[i].isMatched)
    //            {
    //                Destroy(peripheralPuzzles[i].gameObject);
    //            }
    //        }
    //    }
    //}

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
