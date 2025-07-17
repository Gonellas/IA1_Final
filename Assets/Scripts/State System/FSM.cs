using System.Collections.Generic;
using UnityEngine;

public abstract class FSM<TState> : MonoBehaviour where TState : System.Enum
{
    protected State<TState> _currentState;
    protected TState _currentStateEnum;
    protected Dictionary<TState, State<TState>> _allStates = new Dictionary<TState, State<TState>>();

    public State<TState> CurrentState => _currentState;
    public TState CurrentStateEnum => _currentStateEnum;

    public void AddState(TState name, State<TState> state)
    {
        if (!_allStates.ContainsKey(name))
        {
            _allStates.Add(name, state);
            state.fsm = this;
        }
        else
        {
            _allStates[name] = state;
        }
    }

    public void ChangeState(TState name)
    {
        if (_allStates.ContainsKey(name))
        {
            _currentState?.OnExit();
            _currentState = _allStates[name];
            _currentStateEnum = name;
            _currentState.OnEnter();
        }
        else
        {
            Debug.LogWarning($"El estado {name} no existe en la FSM.");
        }
    }

    public void Update()
    {
        _currentState?.OnUpdate();
    }
}
