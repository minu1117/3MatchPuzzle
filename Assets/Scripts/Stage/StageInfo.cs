using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "stage", menuName = "Stage Object/Stage")]
public class StageInfo : ScriptableObject
{
    public Board board;

    public int boardWidth;
    public int boardHeight;

    public int clearScore;
    public List<int> blockGridX;
    public List<int> blockGridY;
}
