using UnityEngine.Pool;
using UnityEngine;

public class Row
{
    public void CreateRowPuzzle(IObjectPool<Puzzle> puzzlePool, BoardInfo info, Vector2 size, int rowIndex, int width, int height)
    {
        /*

                        ○○○○○
                        ○○○○○
                        ○○○○○
                        ○○○○○
          rowindex -> { ●●●●● }

        */

        if (width == 0 || height == 0)
            return;

        for (int x = 0; x < width; x++)
        {
            Puzzle pz = puzzlePool.Get();
            (int, int) gn = (rowIndex, x);

            Vector2 customSize = size - (size / 4);
            if (info.GetBlockedGrid(x, rowIndex))
            {
                pz.SetPuzzleType(PuzzleType.Blocked);
                customSize = size;
            }
            else if (rowIndex < height)
            {
                info.SetPuzzle(pz, x, rowIndex);
                SetDuplicationPuzzle(info, pz, x, rowIndex);
            }
            else
            {
                pz.SetRandomPuzzleType();
                pz.gameObject.SetActive(false);
            }

            pz.SetSize(customSize);
            pz.GridNum = gn;
            pz.RectTransform.localPosition = info.GetGridPosition(x, rowIndex);
            info.SetPuzzle(pz, x, rowIndex);
            info.SetGridNum(x, rowIndex);
        }
    }

    private void SetNotDuplicationPuzzleType(Puzzle p, Puzzle cheakP, Puzzle prevP)
    {
        PuzzleType pt = p.type == PuzzleType.None ? (PuzzleType)UnityEngine.Random.Range(0, (int)PuzzleType.Count) : p.type;
        PuzzleType cheakPType = cheakP == null ? PuzzleType.None : cheakP.type;
        PuzzleType prevPType = prevP == null ? PuzzleType.None : prevP.type;

        // 연결 상태 검사
        pt = CheckAndSetPuzzleType(p, pt, cheakP, cheakPType, prevPType);
        pt = CheckAndSetPuzzleType(p, pt, prevP, prevPType, cheakPType);

        p.SetPuzzleType(pt);
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

    public void SetDuplicationPuzzle(BoardInfo info, Puzzle pz, int x, int y)
    {
        Puzzle lp = null;
        Puzzle bp = null;

        if (x > 0)
        {
            lp = info.GetPuzzle(x - 1, y);
        }
        if (y > 0)
        {
            bp = info.GetPuzzle(x, y - 1);
        }

        // 왼쪽, 아래 타입 검사 후 매치되지 않는 퍼즐로 변경
        SetNotDuplicationPuzzleType(pz, lp, bp);
        SetNotDuplicationPuzzleType(pz, bp, lp);
    }
}
