using Godot;

public partial class MainMenu : CanvasLayer
{
    private const float INITIAL_DELAY = 0.25f;
    private const float TRANSITION_DURATION = 0.4f;

    [Export] private PackedScene _sceneMenuStateMachine;
    [Export] private PackedScene _sceneButtonBar;
    [Export] private PackedScene _sceneStoryControl;
    [Export] private PackedScene _sceneSingleplayerControl;
    [Export] private PackedScene _sceneCouchCoopControl;
    [Export] private PackedScene _sceneDailyChallengeControl;
    [Export] private PackedScene _sceneSettingsControl;

    private StateMachine _menuStateMachine;
    private ButtonBar _buttonBar;

    private Control _currentControl;

    public override void _Ready()
    {
        EventHub.Instance.SwitchMenuState += EventHub_SwitchMenuState;

        // Initialize MenuState StateMachine
        _menuStateMachine = _sceneMenuStateMachine.Instantiate<StateMachine>();
        AddChild(_menuStateMachine, false, InternalMode.Front);
        _menuStateMachine.Setup(MenuState.None, SwitchMenuState);
    }

    private void EventHub_SwitchMenuState(MenuState menuState)
    {
        _menuStateMachine.SetState(menuState);
    }

    private void SwitchMenuState(StateMachine stateMachine)
    {
        if (stateMachine.Action != StateMachineAction.Start)
            return;

        HideCurrentControl();

        Control nextControl = null;

        switch (stateMachine.GetState<MenuState>())
        {
            case MenuState.None:
                {
                    if (_buttonBar == null)
                    {
                        _buttonBar = _sceneButtonBar.Instantiate<ButtonBar>();
                        _buttonBar.Position = new Vector2(1920, 0);
                        AddChild(_buttonBar);

                        Tween tween = _buttonBar.CreateTween();
                        tween.SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Cubic);
                        tween.TweenInterval(INITIAL_DELAY);

                        tween.TweenProperty(_buttonBar, "position", new Vector2(1344, 0), TRANSITION_DURATION);
                    }
                }
                break;

            case MenuState.Story:
                {
                    nextControl = _sceneStoryControl.Instantiate<Control>();
                }
                break;

            case MenuState.Singleplayer:
                {
                    nextControl = _sceneSingleplayerControl.Instantiate<Control>();
                }
                break;

            case MenuState.CouchCoop:
                {
                    nextControl = _sceneCouchCoopControl.Instantiate<Control>();
                }
                break;

            case MenuState.DailyChallenge:
                {
                    nextControl = _sceneDailyChallengeControl.Instantiate<Control>();
                }
                break;

            case MenuState.Settings:
                {
                    nextControl = _sceneSettingsControl.Instantiate<Control>();
                }
                break;
        }

        if (nextControl != null)
            ShowNextControl(nextControl);
    }

    private void ShowNextControl(Control nextControl)
    {
        _currentControl = nextControl;
        _currentControl.Position = new Vector2(0, -1080);
        _currentControl.Modulate = new Color(1, 1, 1, 0);
        AddChild(_currentControl);

        Tween tween = _currentControl.CreateTween();
        tween.SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Cubic);
        tween.TweenProperty(_currentControl, "position", new Vector2(0, 0), TRANSITION_DURATION);
        tween.Parallel().TweenProperty(_currentControl, "modulate", new Color(1, 1, 1, 1), TRANSITION_DURATION);

        Sounds.PlaySound(SoundType.MainMenuSelect);
    }

    private void HideCurrentControl()
    {
        if (_currentControl == null)
            return;

        Control control = _currentControl;
        _currentControl = null;

        Tween tween = control.CreateTween();
        tween.SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Cubic);
        tween.TweenProperty(control, "position", new Vector2(0, 1080), TRANSITION_DURATION);
        tween.Parallel().TweenProperty(control, "modulate", new Color(1, 1, 1, 0), TRANSITION_DURATION);
        tween.TweenCallback(Callable.From(() => control.QueueFree()));
    }
}
