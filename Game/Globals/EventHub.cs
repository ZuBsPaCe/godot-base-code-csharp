using Godot;

public partial class EventHub : Node
{
    public static EventHub Instance { get; set; }

    [Signal] public delegate void SwitchGameStateEventHandler(GameState gameState);
    [Signal] public delegate void SwitchMenuStateEventHandler(MenuState menuState);

    public static void EmitSwitchGameState(GameState gameState)
    {
        Instance.EmitSignal(SignalName.SwitchGameState, (int) gameState);
    }

    public static void EmitSwitchMenuState(MenuState menuState)
    {
        Instance.EmitSignal(SignalName.SwitchMenuState, (int)menuState);
    }
}
