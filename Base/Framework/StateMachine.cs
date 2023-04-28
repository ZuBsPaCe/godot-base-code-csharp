using Godot;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;

public partial class StateMachine : Node
{
    private Action<StateMachine> _callback;
    private Queue<int> _stateQueue = new();
    private int _state = -1;

    public delegate void StateMachineFunc(StateMachine stateMachine);

    public StateMachineAction Action { get; private set; }

    public void Setup<T>(T initialState, Action<StateMachine> callback) where T : Enum
    {
        _callback = callback;
        SetState(initialState);
    }

    public override void _Process(double delta)
    {
        if (_stateQueue.TryDequeue(out int newState) && newState != _state)
        {
            if (_state >= 0)
            {
                Action = StateMachineAction.End;
                _callback?.Invoke(this);
            }

            _state = newState;
            Action = StateMachineAction.Start;
            _callback?.Invoke(this);
            Action = StateMachineAction.Process;
        }
        else
        {
            _callback?.Invoke(this);
        }
    }

    public T GetState<T>() where T : Enum
    {
        // https://stackoverflow.com/a/60022245/998987
        return Unsafe.As<int, T>(ref _state);
    }

    public void SetState<T>(T newState) where T : Enum
    {
        // https://stackoverflow.com/a/60022245/998987
        int intState = Unsafe.As<T, int>(ref newState);

        Debug.Assert(intState >= 0);
        _stateQueue.Enqueue(intState);
    }
}
