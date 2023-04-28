using Godot;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;

public class Runner
{
    private bool _aborted;
    private List<AnimationPlayer> _animationPlayers = new();
    private List<Tween> _tweens = new();

    public bool Aborted { get => _aborted; }

    public void Abort()
    {
        _aborted = true;

        foreach (var animationPlayer in _animationPlayers)
            animationPlayer.Stop(false);

        _animationPlayers.Clear();

        foreach (var tween in _tweens)
            tween.Kill();

        _tweens.Clear();
    }

    public async Task RunAnimation(AnimationPlayer animationPlayer, string animation)
    {
        Debug.Assert(!_aborted);
        if (_aborted)
            return;

        _animationPlayers.Add(animationPlayer);

        animationPlayer.Play(animation);
        await animationPlayer.ToSignal(animationPlayer, AnimationPlayer.SignalName.AnimationFinished);

        _animationPlayers.Remove(animationPlayer);
    }

    public async Task RunTween(Tween tween)
    {
        Debug.Assert(!_aborted);
        if (_aborted)
            return;

        _tweens.Add(tween);

        await tween.ToSignal(tween, Tween.SignalName.Finished);

        _tweens.Remove(tween);
    }
}
