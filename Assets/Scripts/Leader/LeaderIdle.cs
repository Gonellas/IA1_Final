using UnityEngine;

public class LeaderIdle : State<LeaderStates>
{
    private Leader _leader;

    public LeaderIdle(Leader leader) : base(leader.transform)
    {
        _leader = leader;
    }

    public override void OnEnter() { }

    public override void OnUpdate()
    {
        var target = _leader.GetEnemyTarget();
        // ¡Ahora chequea por el enum!
        if (target != null && _leader.LeaderFSM.CurrentStateEnum != LeaderStates.Attacking)
        {
            Debug.Log($"[LeaderIdle] {_leader.name} ve a {target.name} y cambia a Attacking");
            _leader.LeaderFSM.ChangeState(LeaderStates.Attacking);
        }
    }

    public override void OnExit() { }
}
