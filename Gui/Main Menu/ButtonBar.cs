using Godot;
using System;
using System.Collections.Generic;

public partial class ButtonBar : MarginContainer
{
    private Button _storyButton;
    private Button _singleplayerButton;
    private Button _couchCoopButton;
    private Button _dailyChallengeButton;
    private Button _settingsButton;
    private Button _exitButton;

    private List<Button> _allButtons;

    private Button _currentButton;

    public override void _Ready()
    {
        _storyButton = GetNode<Button>("%StoryButton");
        _singleplayerButton = GetNode<Button>("%SingleplayerButton");
        _couchCoopButton = GetNode<Button>("%CouchCoopButton");
        _dailyChallengeButton = GetNode<Button>("%DailyChallengeButton");
        _settingsButton = GetNode<Button>("%SettingsButton");
        _exitButton = GetNode<Button>("%ExitButton");

        _allButtons = new()
        {
            _storyButton,
            _singleplayerButton,
            _couchCoopButton,
            _dailyChallengeButton,
            _settingsButton,
            _exitButton
        };
    }

    public override void _UnhandledKeyInput(InputEvent @event)
    {
        if (_allButtons.TrueForAll(button => !button.HasFocus()))
        {
            _storyButton.GrabFocus();
        }
    }

    public void StoryButton_Toggled(bool pressed)
    {
        if (pressed)
        {
            SetCurrentButton(_storyButton, MenuState.Story);
        }
    }

    public void StoryButton_FocusEntered()
    {
        SetCurrentButton(_storyButton, MenuState.Story);
    }

    public void AllButtons_MouseEntered()
    {
        Sounds.PlaySound(SoundType.MainMenuHover);
    }

    public void SingleplayerButton_Toggled(bool pressed)
    {
        if (pressed)
            SetCurrentButton(_singleplayerButton, MenuState.Singleplayer);
    }

    public void SingleplayerButton_FocusEntered()
    {
        SetCurrentButton(_singleplayerButton, MenuState.Singleplayer);
    }

    public void CouchCoopButton_Toggled(bool pressed)
    {
        if (pressed)
            SetCurrentButton(_couchCoopButton, MenuState.CouchCoop);
    }

    public void CouchCoopButton_FocusEntered()
    {
        SetCurrentButton(_couchCoopButton, MenuState.CouchCoop);
    }

    public void DailyChallengeButton_Toggled(bool pressed)
    {
        if (pressed)
            SetCurrentButton(_dailyChallengeButton, MenuState.DailyChallenge);
    }

    public void DailyChallengeButton_FocusEntered()
    {
        SetCurrentButton(_dailyChallengeButton, MenuState.DailyChallenge);
    }

    public void SettingsButton_Toggled(bool pressed)
    {
        if (pressed)
            SetCurrentButton(_settingsButton, MenuState.Settings);
    }

    public void SettingsButton_FocusEntered()
    {
        SetCurrentButton(_settingsButton, MenuState.Settings);
    }

    public void ExitButton_Pressed()
    {
        GetTree().Quit();
    }

    public void ExitButton_FocusEntered()
    {
        SetCurrentButton(_exitButton, MenuState.None);
    }

    private void SetCurrentButton(Button button, MenuState menuState)
    {
        if (_currentButton != null)
            _currentButton.ButtonPressed = false;

        _currentButton = button;
        EventHub.EmitSwitchMenuState(menuState);
    }

}
