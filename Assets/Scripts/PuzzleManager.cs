using UnityEngine;

public class PuzzleManager : MonoBehaviour
{
    [SerializeField] private Board board;

    private void Start()
    {
        CreateBoard();
    }

    private void CreateBoard()
    {
        board.Init();
    }
}
