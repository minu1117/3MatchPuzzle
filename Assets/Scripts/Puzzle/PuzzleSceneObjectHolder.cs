using UnityEngine;
using UnityEngine.UI;

public class PuzzleSceneObjectHolder : MonoBehaviour
{
    [Header("Parents Objects")]
    [SerializeField] private GridLayoutGroup backgroundParentsObject;
    [SerializeField] private GameObject puzzleParentsObject;
    public GameObject boardParentObject;
    public GameObject effectPoolParentsObject;

    [Header("Board")]
    public Button boardMixButton;

    [Header("UI")]
    public Score score;
    public Button exitButton;

    public GameObject GetEffectPoolParent()
    {
        return effectPoolParentsObject;
    }

    public GridLayoutGroup GetBackgroundParent()
    {
        return backgroundParentsObject;
    }

    public GameObject GetPuzzleParent()
    {
        return puzzleParentsObject;
    }
}
