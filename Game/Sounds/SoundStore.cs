using Godot;
using System;

public partial class SoundStore : Node
{
    [Export] private AudioStreamPlayer _mainMenuHover;
    [Export] private AudioStreamPlayer _mainMenuSelect;

    public override void _Ready()
    {
        Sounds.RegisterSound(SoundType.MainMenuHover, _mainMenuHover);
        Sounds.RegisterSound(SoundType.MainMenuSelect, _mainMenuSelect);
    }
}
