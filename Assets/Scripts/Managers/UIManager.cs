public class UIManager : Manager<UIManager>
{
    public Option option;

    protected override void Awake()
    {
        base.Awake();
    }

    private void Start()
    {
        option = FindFirstObjectByType<Option>();
        ConnectOptionButtonOnClick();
    }

    public void ConnectOptionButtonOnClick()
    {
        option.ConnectButtonOnClick();
    }
}
