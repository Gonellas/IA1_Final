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
        Vector3 planarDir = new Vector3(dir.x, 0, dir.z);
        if (planarDir.sqrMagnitude > 0.01f)
        {
            Quaternion targetRot = Quaternion.LookRotation(planarDir, Vector3.up);
            _transform.rotation = Quaternion.Lerp(_transform.rotation, targetRot, Time.deltaTime * 8f);
        }

        float distancia = Vector3.Distance(_transform.position, _target.position);

        if (distancia < 2f)
        {
            if (_leader.CanAttack())
            {
                if (_target.TryGetComponent(out IDamageable dmg))
                {
                    Debug.Log($"[LeaderAttack] {_leader.gameObject.name} atacó a {_target.gameObject.name}");
                    dmg.TakeDamage((int)_leader.AttackDamage);
                    _leader.PerformAttack();
                }
            }
        }
    }

    public override void OnExit() { }
}
