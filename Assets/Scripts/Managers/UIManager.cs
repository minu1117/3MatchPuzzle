using UnityEngine;
using UnityEngine.UI;

public class UIManager : Manager<UIManager>
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
            option.Init();
            option.SynchronizeVolumeSliderValue();
        }
    }

    // 가로, 세로 크기에 맞춰 Grid Layout Group의 Cell Size, Spacing 조정 (넘치지 않게)
    public void FitToCell(GridLayoutGroup group, int width, int height)
    {
        RectTransform parentRect = group.GetComponent<RectTransform>();
        float parentWidth = parentRect.rect.width;
        float parentHeight = parentRect.rect.height;

        Vector2 cellSize = group.cellSize;
        Vector2 spacing = group.spacing;

        float xSize = parentWidth - ((cellSize.x * width) + (spacing.x * width));
        float ySize = parentHeight - ((cellSize.y * height) + (spacing.y * height));

        if (xSize < 0 || ySize < 0)
        {
            int xSpacing = (int)(cellSize.x / spacing.x);
            int ySpacing = (int)(cellSize.y / spacing.y);

            int xAddCount = 0;
            int yAddCount = 0;
            while (true)
            {
                cellSize.x--;
                cellSize.y--;

                xAddCount++;
                yAddCount++;

                if (xAddCount > 0 && xAddCount % xSpacing == 0)
                {
                    spacing.x--;
                }
                if (yAddCount > 0 && yAddCount % ySpacing == 0)
                {
                    spacing.y--;
                }

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
            int xSpacing = (int)(cellSize.x / spacing.x);
            int ySpacing = (int)(cellSize.y / spacing.y);

            int xAddCount = 0;
            int yAddCount = 0;
            while (true)
            {
                if (xAddCount > 0 && xAddCount % xSpacing == 0)
                {
                    spacing.x++;
                }
                if (yAddCount > 0 && yAddCount % ySpacing == 0)
                {
                    spacing.y++;
                }

                xSize = ((cellSize.x * width) + (spacing.x * width)) - parentWidth;
                ySize = ((cellSize.y * height) + (spacing.y * height)) - parentHeight;

                if (xSize < 0 && ySize < 0)
                {
                    cellSize.x++;
                    cellSize.y++;

                    xAddCount++;
                    yAddCount++;
                }
                else
                {
                    group.cellSize = cellSize;
                    group.spacing = spacing;
                    break;
                }
            }
        }
    }
}
