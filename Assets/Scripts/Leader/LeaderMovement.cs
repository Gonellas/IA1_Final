using UnityEngine;
using System.Collections.Generic;

public class LeaderMovement : State<LeaderStates>
{
    private Leader _leader;
    private List<Node> _path;
    private int _nodeIndex;

    public LeaderMovement(Leader leader) : base(leader.transform)
    {
        _leader = leader;
    }

    public override void OnEnter()
    {
        Node start = GameManager.Instance.ClosestNode(_leader.transform.position);
        Node end = GameManager.Instance.ClosestNode(_leader.TargetPosition);

        _path = GameManager.Instance.Pathfinding.ThetaStar(start, end);

        if (_path == null || _path.Count == 0)
        {
            _leader.LeaderFSM.ChangeState(LeaderStates.Idle);
            return;
        }

        _nodeIndex = 0;
    }

    public override void OnUpdate()
    {
        if (_path == null || _nodeIndex >= _path.Count)
        {
            _leader.LeaderFSM.ChangeState(LeaderStates.Idle);
            return;
        }

        Vector3 targetPos = _path[_nodeIndex].transform.position;

        if (!GameManager.Instance.InSight(_leader.transform.position, targetPos))
        {
            Node start = GameManager.Instance.ClosestNode(_leader.transform.position);
            Node end = GameManager.Instance.ClosestNode(_leader.TargetPosition);
            _path = GameManager.Instance.Pathfinding.ThetaStar(start, end);
            _nodeIndex = 0;
        }

        // 1 Dirección objetivo
        Vector3 dir = (targetPos - _leader.transform.position).normalized;
        // 2 fuerza avoidance
        Vector3 avoidForce = _leader.obstacleAvoidance.GetAvoidanceForce(dir);
        // 3 dirección normalizada
        Vector3 moveDir = (dir + avoidForce).normalized;
        // 4 Movimiento final
        Vector3 move = moveDir * _leader.Speed * Time.deltaTime;

        Vector3 newPos = _leader.transform.position + move;
        newPos.y = _leader.transform.position.y;
        _leader.transform.position = newPos;

        Vector3 planarDir = new Vector3(moveDir.x, 0, moveDir.z);
        if (planarDir.sqrMagnitude > 0.01f)
        {
            Quaternion targetRot = Quaternion.LookRotation(planarDir, Vector3.up);
            _transform.rotation = Quaternion.Lerp(_transform.rotation, targetRot, Time.deltaTime * 8f);
        }

        if (Vector3.Distance(_leader.transform.position, targetPos) < _leader.StopDistance)
        {
            _nodeIndex++;
            if (_nodeIndex >= _path.Count)
                _leader.LeaderFSM.ChangeState(LeaderStates.Idle);
        }
    }

    public override void OnExit() { }
}
