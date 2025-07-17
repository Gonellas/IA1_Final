using UnityEngine;
using System.Collections.Generic;

public class EntityFlee : State<EntitiesStates>
{
    private AIAgent _agent;
    private List<Node> _path;
    private int _nodeIndex;

    private float _timeSinceLastProgress = 0f;
    private float _recalcInterval = 2f; // segundos sin avanzar antes de recalcular
    private Vector3 _lastPos;
    private float _progressThreshold = 0.2f; // distancia mínima que debe haber avanzado

    public EntityFlee(AIAgent agent) : base(agent.transform)
    {
        _agent = agent;
    }

    public override void OnEnter()
    {
        Debug.Log($"[EntityFlee] {_agent.name} ENTRA en estado Flee. Vida: {_agent.Health}, Safezone: {_agent.safeZone}");

        if (_agent.safeZone != null)
        {
            Node start = GameManager.Instance.ClosestNode(_agent.transform.position);
            Node end = GameManager.Instance.ClosestNode(_agent.safeZone.position);
            _path = GameManager.Instance.Pathfinding.ThetaStar(start, end);
            _nodeIndex = 0;
        }
        else
        {
            _path = null;
            Debug.LogWarning($"[EntityFlee] {_agent.name} NO tiene safezone asignada.");
        }
        _lastPos = _transform.position;
        _timeSinceLastProgress = 0f;
    }

    public override void OnUpdate()
    {
        if (_agent.isHealing)
        {
            Debug.Log($"[EntityFlee] {_agent.name} está en zona segura. Vida: {_agent.Health}");
            if (_agent.Health >= _agent.MaxHealth)
            {
                _agent.Health = _agent.MaxHealth;
                Debug.Log($"[EntityFlee] {_agent.name} recuperó la vida, sale de Flee y vuelve a FollowLeader.");
                fsm.ChangeState(EntitiesStates.FollowLeader); // o Attacking si preferís
            }
            return;
        }

        if (_path != null && _nodeIndex < _path.Count)
        {
            Vector3 targetPos = _path[_nodeIndex].transform.position;
            Vector3 dir = (targetPos - _transform.position).normalized;
            Vector3 move = dir * _agent.FleeSpeed * Time.deltaTime;
            Vector3 nextPos = _transform.position + move;
            nextPos.y = _transform.position.y;
            _transform.position = nextPos;

            Debug.Log($"[EntityFlee] {_agent.name} huye hacia safezone. Nodo {(_nodeIndex + 1)}/{_path.Count} Posición objetivo: {targetPos}");

            if (Vector3.Distance(_transform.position, targetPos) < 0.3f)
            {
                _nodeIndex++;
                _timeSinceLastProgress = 0f;
                _lastPos = _transform.position;

                if (_nodeIndex >= _path.Count)
                    Debug.Log($"[EntityFlee] {_agent.name} llegó al último nodo del path hacia safezone.");
            }
            else
            {
                // Si no avanzó lo suficiente en este frame, acumulá tiempo sin progreso
                if (Vector3.Distance(_transform.position, _lastPos) < _progressThreshold)
                {
                    _timeSinceLastProgress += Time.deltaTime;
                    if (_timeSinceLastProgress >= _recalcInterval)
                    {
                        Debug.LogWarning($"[EntityFlee] {_agent.name} lleva {_recalcInterval} segundos sin avanzar. Recalculando path a safezone...");
                        Node start = GameManager.Instance.ClosestNode(_agent.transform.position);
                        Node end = GameManager.Instance.ClosestNode(_agent.safeZone.position);
                        _path = GameManager.Instance.Pathfinding.ThetaStar(start, end);
                        _nodeIndex = 0;
                        _timeSinceLastProgress = 0f;
                        _lastPos = _transform.position;
                    }
                }
                else
                {
                    _timeSinceLastProgress = 0f;
                    _lastPos = _transform.position;
                }
            }
        }
        else if (_agent.safeZone != null)
        {
            // Llegó al final del path pero no a la safezone → intenta recalcular (por seguridad extra)
            Node start = GameManager.Instance.ClosestNode(_agent.transform.position);
            Node end = GameManager.Instance.ClosestNode(_agent.safeZone.position);
            _path = GameManager.Instance.Pathfinding.ThetaStar(start, end);
            _nodeIndex = 0;
            _timeSinceLastProgress = 0f;
            _lastPos = _transform.position;
            Debug.LogWarning($"[EntityFlee] {_agent.name} recalcula path a safezone (finalizó path pero no llegó a zona segura).");
        }
    }

    public override void OnExit()
    {
        Debug.Log($"[EntityFlee] {_agent.name} SALE de estado Flee.");
    }
}
