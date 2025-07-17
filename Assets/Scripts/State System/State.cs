using UnityEngine;

public abstract class State<TState> where TState : System.Enum
{
    protected Transform _transform;
    public FSM<TState> fsm;

    protected State(Transform transform)
    {
        _transform = transform;
    }

    public abstract void OnEnter();
    public abstract void OnUpdate();
    public abstract void OnExit();
}
