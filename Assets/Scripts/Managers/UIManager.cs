using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UIManager : Singleton<UIManager>
{
    public Option option;

    protected override void Awake()
    {
        base.Awake();
    }

    private void Start()
    {
        FindOptionObject();
    }

    public void FindOptionObject()
    {
        option = FindFirstObjectByType<Option>();
        if (option != null) 
        {
            string sceneName = SceneManager.GetActiveScene().name;
            if (sceneName == MySceneManager.Instance.modeChoiceSceneName)
            {
                option.OnMenuExitButton();
            }
            else if (sceneName == MySceneManager.Instance.boardCreateSceneName ||
                     sceneName == MySceneManager.Instance.stageSceneName)
            {
                option.OnMenuExitButton();
                option.OnModeChoiceExitButton();
            }
            else if (sceneName == MySceneManager.Instance.gameSceneName)
            {
                option.OnAllButtons();
            }
        }
    }

    // 가로, 세로 크기에 맞춰 Grid Layout Group의 Cell Size, Spacing 조정 (넘치지 않게)
    public void FitToCell(GridLayoutGroup group, int width, int height)
    {
        if (width == 0 || height == 0)
            return;

        RectTransform parentRect = group.GetComponent<RectTransform>();
        float parentWidth = parentRect.rect.width;
        float parentHeight = parentRect.rect.height;

        Vector2 cellSize = group.cellSize;
        Vector2 spacing = group.spacing;

        float xSize = parentWidth - ((cellSize.x * width) + (spacing.x * width));
        float ySize = parentHeight - ((cellSize.y * height) + (spacing.y * height));

        int xSpacing = (int)(cellSize.x / spacing.x);
        int ySpacing = (int)(cellSize.y / spacing.y);

        int xAddCount = 0;
        int yAddCount = 0;

        if (xSize < 0 || ySize < 0)
        {
            while (true)
            {
                cellSize.x--;
                cellSize.y--;

                xAddCount++;
                yAddCount++;

                if (xAddCount > 0 && xAddCount % xSpacing == 0) spacing.x--;
                if (yAddCount > 0 && yAddCount % ySpacing == 0) spacing.y--;

                xSize = parentWidth - ((cellSize.x * width) + (spacing.x * width));
                ySize = parentHeight - ((cellSize.y * height) + (spacing.y * height));

                if (xSize >= 0 && ySize >= 0)
                {
                    group.cellSize = cellSize;
                    group.spacing = spacing;
                    break;
                }
            }
        }
        else
        {
            while (true)
            {
                if (xSize < 0 && ySize < 0)
                {
                    cellSize.x++;
                    cellSize.y++;

                    xAddCount++;
                    yAddCount++;
                }

                if (xAddCount > 0 && xAddCount % xSpacing == 0) spacing.x++;
                if (yAddCount > 0 && yAddCount % ySpacing == 0) spacing.y++;

                xSize = ((cellSize.x * width) + (spacing.x * width)) - parentWidth;
                ySize = ((cellSize.y * height) + (spacing.y * height)) - parentHeight;

                if (xSize >= 0 || ySize >= 0)
                {
                    group.cellSize = cellSize;
                    group.spacing = spacing;
                    break;
                }
            }
        }
    }
}
