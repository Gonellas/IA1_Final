using UnityEngine;
using System.Collections.Generic;

public class EntityFlee : State<EntitiesStates>
{
    private AIAgent _agent;
    private List<Node> _path;
    private int _nodeIndex;

    private float _timeSinceLastProgress = 0f;
    private float _recalcInterval = 2f; 
    private Vector3 _lastPos;
    private float _progressThreshold = 0.2f; 

    public EntityFlee(AIAgent agent) : base(agent.transform)
    {
        _agent = agent;
    }

    public override void OnEnter()
    {
        if (_agent.safeZone != null)
        {
            Node start = GameManager.Instance.ClosestNode(_agent.transform.position);
            Node end = GameManager.Instance.ClosestNode(_agent.safeZone.position);
            _path = GameManager.Instance.Pathfinding.ThetaStar(start, end);
            _nodeIndex = 0;
        }
        else _path = null;

        _lastPos = _transform.position;
        _timeSinceLastProgress = 0f;
    }

    public override void OnUpdate()
    {
        if (_agent.isHealing)
        {
            if (_agent.health >= _agent.maxHealth)
            {
                _agent.health = _agent.maxHealth;
                fsm.ChangeState(EntitiesStates.FollowLeader);
            }

            return;
        }

        if (_path != null && _nodeIndex < _path.Count)
        {
            Vector3 targetPos = _path[_nodeIndex].transform.position;
            Vector3 dir = (targetPos - _transform.position).normalized * _agent.fleeSpeed;
            Vector3 avoidForce = _agent.obstacleAvoidance.GetAvoidanceForce(dir);
            Vector3 finalMove = dir + avoidForce;
            finalMove = Vector3.ClampMagnitude(finalMove, _agent.fleeSpeed);

            Vector3 nextPos = _transform.position + finalMove * Time.deltaTime;
            nextPos.y = _transform.position.y;
            _transform.position = nextPos;

            Vector3 planarDir = new Vector3(finalMove.x, 0, finalMove.z);
            if (planarDir.sqrMagnitude > 0.01f)
            {
                Quaternion targetRot = Quaternion.LookRotation(planarDir, Vector3.up);
                _transform.rotation = Quaternion.Lerp(_transform.rotation, targetRot, Time.deltaTime * 8f);
            }

            if (Vector3.Distance(_transform.position, targetPos) < 0.3f)
            {
                _nodeIndex++;
            }
        }
        else if (_agent.safeZone != null)
        {
            Node start = GameManager.Instance.ClosestNode(_agent.transform.position);
            Node end = GameManager.Instance.ClosestNode(_agent.safeZone.position);
            _path = GameManager.Instance.Pathfinding.ThetaStar(start, end);
            _nodeIndex = 0;
        }
    }


    public override void OnExit()
    {
        Debug.Log($"[EntityFlee] {_agent.name} SALE de estado Flee.");
    }
}
