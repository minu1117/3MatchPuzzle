using UnityEngine.Pool;

public class Row
{
    public void CreateRowPuzzle(IObjectPool<Puzzle> puzzlePool, Board board, int rowIndex, int width, int height)
    {
        /*

                        �ۡۡۡۡ�
                        �ۡۡۡۡ�
                        �ۡۡۡۡ�
                        �ۡۡۡۡ�
          rowindex -> { �ܡܡܡܡ� }

        */

        if (width == 0 || height == 0)
            return;

        for (int x = 0; x < width; x++)
        {
            Puzzle pz = puzzlePool.Get();
            (int, int) gn = (rowIndex, x);

            if (rowIndex < height)
            {
                board.SetPuzzle(pz, (rowIndex, x));
                SetDuplicationPuzzle(board, pz, x, rowIndex);
            }
            else
            {
                pz.SetRandomPuzzleType();
                pz.gameObject.SetActive(false);
            }

            pz.GridNum = gn;
            pz.RectTransform.localPosition = board.GetGridPosition(gn);
            board.SetPuzzle(pz, gn);
            board.SetGridNum(gn);
        }
    }

    private void SetNotDuplicationPuzzleType(Puzzle p, Puzzle cheakP, Puzzle prevP)
    {
        PuzzleType pt = p.type == PuzzleType.None ? (PuzzleType)UnityEngine.Random.Range(0, (int)PuzzleType.Count) : p.type;
        PuzzleType cheakPType = cheakP == null ? PuzzleType.None : cheakP.type;
        PuzzleType prevPType = prevP == null ? PuzzleType.None : prevP.type;

        // ���� ���� �˻�
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

    public void SetDuplicationPuzzle(Board board, Puzzle pz, int x, int y)
    {
        Puzzle lp = null;
        Puzzle bp = null;

        if (x > 0)
        {
            lp = board.GetPuzzle((y, x - 1));
        }
        if (y > 0)
        {
            bp = board.GetPuzzle((y - 1, x));
        }

        // ����, �Ʒ� Ÿ�� �˻� �� ��ġ���� �ʴ� ����� ����
        SetNotDuplicationPuzzleType(pz, lp, bp);
        SetNotDuplicationPuzzleType(pz, bp, lp);
    }
}
