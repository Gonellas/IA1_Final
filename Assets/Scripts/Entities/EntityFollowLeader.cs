using UnityEngine;
using System.Collections.Generic;

public class EntityFollowLeader : State<EntitiesStates>
{
    private AIAgent _agent;
    private Transform _leader;
    private List<Node> _path;
    private int _nodeIndex;
    private float _minDistanceToLeader = 1.5f;

    public EntityFollowLeader(AIAgent agent) : base(agent.transform)
    {
        _agent = agent;
    }

    public override void OnEnter()
    {
        _leader = _agent.leaderTransform;
        _path = null;
        _nodeIndex = 0;
    }

    public override void OnUpdate()
    {
        if (_leader == null)
        {
            fsm.ChangeState(EntitiesStates.Idle);
            return;
        }

        if (_agent.EnemyInSight())
        {
            fsm.ChangeState(EntitiesStates.Attacking);
            return;
        }

        float distanceToLeader = Vector3.Distance(_transform.position, _leader.position);

        if (!GameManager.Instance.InSight(_transform.position, _leader.position) || (_path != null && _nodeIndex < _path.Count))
        {
            if (_path == null || _nodeIndex >= _path.Count)
            {
                Node start = GameManager.Instance.ClosestNode(_transform.position);
                Node end = GameManager.Instance.ClosestNode(_leader.position);
                _path = GameManager.Instance.Pathfinding.ThetaStar(start, end);
                _nodeIndex = 0;
            }

            if (_path != null && _nodeIndex < _path.Count)
            {
                Vector3 target = _path[_nodeIndex].transform.position;
                Vector3 dir = (target - _transform.position).normalized * _agent.followSpeed;
                Vector3 avoidForce = _agent.obstacleAvoidance.GetAvoidanceForce(dir);

                Vector3 finalMove = dir + avoidForce;
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

                if (Vector3.Distance(_transform.position, target) < 0.3f)
                {
                    _nodeIndex++;
                    if (_nodeIndex >= _path.Count)
                        _path = null;
                }
            }
        }
        else
        {
            Vector3 toLeader = _leader.position - _transform.position;

            Vector3 separation = Vector3.zero;
            Collider[] nearby = Physics.OverlapSphere(_transform.position, _agent.separationRadius);
            foreach (var other in nearby)
            {
                if (other.transform != _transform && other.GetComponent<AIAgent>())
                {
                    Vector3 dirToOther = _transform.position - other.transform.position;
                    float dist = dirToOther.magnitude;
                    if (dist > 0)
                        separation += dirToOther.normalized / (dist * dist);
                }
            }

            if (distanceToLeader > _minDistanceToLeader)
            {
                Vector3 desired = toLeader.normalized * _agent.followSpeed;
                Vector3 moveDir = desired + separation * _agent.separationWeight;
                Vector3 avoidForce = _agent.obstacleAvoidance.GetAvoidanceForce(moveDir);

                Vector3 finalMove = moveDir + avoidForce;

                finalMove = Vector3.ClampMagnitude(finalMove, _agent.followSpeed);

                _agent.velocity += finalMove;
                _agent.velocity = Vector3.ClampMagnitude(_agent.velocity, _agent.followSpeed);

                Vector3 nextPos = _transform.position + _agent.velocity * Time.deltaTime;
                nextPos.y = _transform.position.y;
                _transform.position = nextPos;

                if (_agent.velocity.sqrMagnitude > 0.01f)
                {
                    Vector3 dir = new Vector3(_agent.velocity.x, 0, _agent.velocity.z);
                    if (dir.sqrMagnitude > 0.01f)
                    {
                        Quaternion targetRot = Quaternion.LookRotation(dir, Vector3.up);
                        _transform.rotation = Quaternion.Lerp(_transform.rotation, targetRot, Time.deltaTime * 8f);
                    }
                }
            }
        }
    }

    public override void OnExit() { }
}
