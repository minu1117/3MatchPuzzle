using UnityEngine;
using UnityEngine.UI;

public class PuzzleSceneObjectHolder : MonoBehaviour
{
    [Header("Parents Objects")]
    public GameObject effectPoolParentsObject;

    [Header("Board")]
    public Board board;
    public Button boardMixButton;

    [Header("UI")]
    public Score score;
    public Button exitButton;
    public StageClearUI clearUI;

    public GameObject GetEffectPoolParent()
    {
        return effectPoolParentsObject;
    }
}
