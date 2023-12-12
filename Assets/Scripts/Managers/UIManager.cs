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
            option.ConnectButtonOnClick();
            option.SynchronizeVolumeSliderValue();
        }
    }
}
