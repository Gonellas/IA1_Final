using UnityEngine;

public class LeaderAttack : State<LeaderStates>
{
    private Leader _leader;
    private Transform _target;

    public LeaderAttack(Leader leader) : base(leader.transform)
    {
        _leader = leader;
    }

    public override void OnEnter()
    {
        _target = _leader.GetEnemyTarget();
    }

    public override void OnUpdate()
    {
        if (_target == null || !_leader.CanSeeEnemy(_target))
        {
            _leader.LeaderFSM.ChangeState(LeaderStates.Idle);

            return;
        }

        Vector3 dir = (_target.position - _transform.position).normalized;
        float distancia = Vector3.Distance(_transform.position, _target.position);

        Vector3 avoidForce = _leader.obstacleAvoidance.GetAvoidanceForce(dir);
        Vector3 finalMove = dir + avoidForce;
        finalMove = Vector3.ClampMagnitude(finalMove, _leader.Speed);

        Vector3 planarDir = new Vector3(finalMove.x, 0, finalMove.z);

        if (planarDir.sqrMagnitude > 0.01f)
        {
            Quaternion targetRot = Quaternion.LookRotation(planarDir, Vector3.up);
            _transform.rotation = Quaternion.Lerp(_transform.rotation, targetRot, Time.deltaTime * 8f);
        }

        if (distancia > 2f)
        {
            Vector3 move = finalMove * Time.deltaTime;
            Vector3 nextPos = _transform.position + move;
            nextPos.y = _transform.position.y;
            _transform.position = nextPos;
        }
        else
        {
            if (_leader.CanAttack())
            {
                if (_target.TryGetComponent(out IDamageable dmg))
                {
                    dmg.TakeDamage((int)_leader.AttackDamage);
                    _leader.PerformAttack();
                }
            }
        }
    }

    public override void OnExit() { }
}
