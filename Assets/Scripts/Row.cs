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

                // ����, �Ʒ� Ÿ�� �˻� �� ��ġ���� �ʴ� ����� ����
                SetNotDuplicationPuzzleType(pz, lp, bp);
                SetNotDuplicationPuzzleType(pz, bp, lp);
            }
            else
            {
                pz.SetRandomPuzzleType();
                pz.gameObject.SetActive(false);
            }

            pz.gridNum = gn;
            pz.gameObject.transform.position = board.GetGridPosition(gn);
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
}
