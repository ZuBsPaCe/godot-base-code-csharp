using Godot;

public partial class Main : Node
{
    [Export] private PackedScene _sceneGameStateMachine;
    [Export] private PackedScene _sceneMainMenu;

    private StateMachine _gameStateMachine;

    private MainMenu _mainMenu;


    public override void _Ready()
    {
        // Initialize Sound Singleton
        Sounds.Instance = new Sounds();
        AddChild(Sounds.Instance, false, InternalMode.Front);


        // Initialize EventHub Singleton
        EventHub.Instance = new EventHub();
        AddChild(EventHub.Instance, false, InternalMode.Front);
        EventHub.Instance.SwitchGameState += EventHub_SwitchGameState;


        // Initialize GameState StateMachine
        _gameStateMachine = _sceneGameStateMachine.Instantiate<StateMachine>();
        AddChild(_gameStateMachine, false, InternalMode.Front);
        _gameStateMachine.Setup(GameState.MainMenu, SwitchGameState);
    }

    private void EventHub_SwitchGameState(GameState gameState)
    {
        _gameStateMachine.SetState(gameState);
    }

    private void SwitchGameState(StateMachine stateMachine)
    {
        switch (stateMachine.GetState<GameState>())
        {
            case GameState.MainMenu:
                if (stateMachine.Action == StateMachineAction.Start)
                {
                    Debug.Assert(_mainMenu == null, "SwitchGameState");
                    _mainMenu = _sceneMainMenu.Instantiate<MainMenu>();
                    AddChild(_mainMenu);
                }
                break;
        }
    }
}
