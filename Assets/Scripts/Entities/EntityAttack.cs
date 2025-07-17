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
        if (_target == null)
        {
            fsm.ChangeState(EntitiesStates.FollowLeader);
            return;
        }

        Vector3 dir = (_target.position - _transform.position).normalized;
        Vector3 planarDir = new Vector3(dir.x, 0, dir.z);

        if (planarDir.sqrMagnitude > 0.01f)
        {
            Quaternion targetRot = Quaternion.LookRotation(planarDir, Vector3.up);
            _transform.rotation = Quaternion.Lerp(_transform.rotation, targetRot, Time.deltaTime * 8f);
        }

        float dist = Vector3.Distance(_transform.position, _target.position);

        if (dist > _agent.AttackRange)
        {
            // Se acerca si está lejos
            Vector3 move = dir * _agent.FollowSpeed * Time.deltaTime;
            Vector3 nextPos = _transform.position + move;
            nextPos.y = _transform.position.y;
            _transform.position = nextPos;
        }
        else
        {
            // Ataca si está en rango y puede atacar
            if (_agent.CanAttack())
            {
                if (_target.TryGetComponent(out IDamageable dmg))
                {
                    Debug.Log($"[EntityAttack] {_agent.name} ataca a {_target.name} por {_agent.AttackDamage} de daño.");
                    dmg.TakeDamage(_agent.AttackDamage);
                    _agent.PerformAttack();
                }
                else
                {
                    Debug.Log($"[EntityAttack] {_target.name} NO implementa IDamageable.");
                }
            }
        }

        if (!_agent.EnemyInSight())
        {
            fsm.ChangeState(EntitiesStates.FollowLeader);
        }
        else fsm.ChangeState(EntitiesStates.Attacking);
    }

    public override void OnExit() { }
}
