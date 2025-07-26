using UnityEngine;

public class EntityIdle : State<EntitiesStates>
{
    private AIAgent _agent;

    public EntityIdle(AIAgent agent) : base(agent.transform)
    {
        _agent = agent;
    }

    public override void OnEnter() { }

    public override void OnUpdate()
    {
        if (_agent.leaderTransform != null)
        {
            fsm.ChangeState(EntitiesStates.FollowLeader);
        }
    }

    public override void OnExit() { }
}
