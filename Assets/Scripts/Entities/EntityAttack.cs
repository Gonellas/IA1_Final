using UnityEngine;

public class EntityAttack : State<EntitiesStates>
{
    private AIAgent _agent;
    private Transform _target;

    public EntityAttack(AIAgent agent) : base(agent.transform)
    {
        _agent = agent;
    }

    public override void OnEnter()
    {
        _target = _agent.GetEnemyTarget();
    }

    public override void OnUpdate()
    {
        if (_target == null || !_agent.EnemyInSight())
        {
            fsm.ChangeState(EntitiesStates.FollowLeader);
            return;
        }

        Vector3 dir = (_target.position - _transform.position).normalized;
        float dist = Vector3.Distance(_transform.position, _target.position);

        if (dist > _agent.attackRange)
        {
            Vector3 avoidForce = _agent.obstacleAvoidance.GetAvoidanceForce(dir);
            Vector3 finalMove = dir * _agent.followSpeed + avoidForce;
            finalMove = Vector3.ClampMagnitude(finalMove, _agent.followSpeed);

            Vector3 nextPos = _transform.position + finalMove * Time.deltaTime;
            nextPos.y = _transform.position.y;
            _transform.position = nextPos;

            Vector3 planarDir = new Vector3(finalMove.x, 0, finalMove.z);

            if (planarDir.sqrMagnitude > 0.01f)
            {
                Quaternion targetRot = Quaternion.LookRotation(planarDir, Vector3.up);
                _transform.rotation = Quaternion.Lerp(_transform.rotation, targetRot, Time.deltaTime * 8f);
            }
        }
        else
        {
            if (_agent.CanAttack())
            {
                if (_target.TryGetComponent(out IDamageable dmg))
                {
                    dmg.TakeDamage(_agent.attackDamage);
                    _agent.PerformAttack();
                }
                else Debug.Log($"[EntityAttack] {_target.name} NO implementa IDamageable.");
            }
        }
    }

    public override void OnExit() { }
}
