using UnityEngine;

public abstract class BaseState<TState> : State<TState> where TState : System.Enum
{
    protected Transform _transform;
    protected Vector3 _velocity;
    protected float _speed;
    protected float _force;
    protected float _viewRadius;
    protected float _viewAngle;
    protected FSM<TState> _fsm;

    public BaseState(
        Transform transform,
        Vector3 velocity,
        float speed,
        float force,
        float viewRadius,
        float viewAngle,
        FSM<TState> fsm
    ) : base(transform)
    {
        _transform = transform;
        _velocity = velocity;
        _speed = speed;
        _force = force;
        _viewRadius = viewRadius;
        _viewAngle = viewAngle;
        _fsm = fsm;
    }

    protected Vector3 Seek(Vector3 targetPosition)
    {
        Vector3 desired = (targetPosition - _transform.position).normalized * _speed;
        Vector3 steering = desired - _velocity;
        return Vector3.ClampMagnitude(steering, _force);
    }

    protected bool InFOV(Transform obj)
    {
        Vector3 dir = obj.position - _transform.position;

        if (dir.magnitude <= _viewRadius)
        {
            float angle = Vector3.Angle(_transform.forward, dir);
            if (angle <= _viewAngle * 0.5f)
            {
                return GameManager.Instance.InSight(_transform.position, obj.position);
            }
        }

        return false;
    }

    protected void ForceToAdd(Vector3 dir)
    {
        _velocity += dir;
        _velocity = Vector3.ClampMagnitude(_velocity, _speed);
    }
}
