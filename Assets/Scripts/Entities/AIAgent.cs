using UnityEngine;

public class AIAgent : MonoBehaviour, IDamageable
{
    public EntitiesFSM fsm;
    public Vector3 Velocity { get; set; }
    public bool isHealing = false;
    [SerializeField] private bool _isRedTeam = true;

    [Header("Flocking")]
    public Transform LeaderTransform;
    public float SeparationRadius = 2f;
    public float SeparationWeight = 1.5f;
    public float FollowSpeed = 3f;

    [Header("Combate")]
    public float Health = 100f;
    public float MaxHealth = 100f;
    public float FleeThreshold = 10f;
    public float AttackRange = 2f;
    public float AttackCooldown = 2f;
    public int AttackDamage = 5;
    private float _lastAttackTime = 0f;

    [Header("Flee")]
    public float FleeSpeed = 4f;
    public Transform safeZone; // Usado para pathfinding

    [Header("Enemy Detection")]
    public LayerMask enemyLayer;
    public float visionRadius = 10f;
    public float visionAngle = 90f;

    // Radio de curado para safezone
    public float safeZoneRadius = 1f;

    [Header("Obstacle Avoidance")]
    public float avoidDistance = 2f;
    public float avoidForce = 6f;
    public LayerMask obstacleLayer;

    [HideInInspector] public ObstacleAvoidance obstacleAvoidance;


    private void Start()
    {
        fsm = gameObject.AddComponent<EntitiesFSM>();
        fsm.AddState(EntitiesStates.Idle, new EntityIdle(this));
        fsm.AddState(EntitiesStates.FollowLeader, new EntityFollowLeader(this));
        fsm.AddState(EntitiesStates.Attacking, new EntityAttack(this));
        fsm.AddState(EntitiesStates.Fleeing, new EntityFlee(this));
        fsm.ChangeState(EntitiesStates.Idle);

        obstacleAvoidance = new ObstacleAvoidance(
            transform,
            avoidDistance,
            avoidForce,
            obstacleLayer
        );
    }


    private void Update()
    {
        fsm.Update();

        // Curación manual sin Rigidbody
        bool foundSafeZone = false;
        Collider[] hits = Physics.OverlapSphere(transform.position, safeZoneRadius);
        foreach (var hit in hits)
        {
            if (hit.CompareTag("SafeZone"))
            {
                foundSafeZone = true;
                break;
            }
        }
        isHealing = foundSafeZone;

        if (isHealing && Health < MaxHealth)
        {
            Health += 20f * Time.deltaTime;
            if (Health > MaxHealth) Health = MaxHealth;
        }
    }

    public bool InFOV(Transform target)
    {
        if (target == null) return false;

        Vector3 dirToTarget = (target.position - transform.position).normalized;
        float angle = Vector3.Angle(transform.forward, dirToTarget);
        float dist = Vector3.Distance(transform.position, target.position);

        // Chequea ángulo y radio
        if (angle < visionAngle / 2 && dist <= visionRadius)
        {
            if (!Physics.Linecast(transform.position, target.position, GameManager.Instance._wallsLayer))
                return true;
        }
        return false;
    }

    public Transform GetEnemyTarget()
    {
        Collider[] enemies = Physics.OverlapSphere(transform.position, visionRadius, enemyLayer);
        float minDist = Mathf.Infinity;
        Transform nearest = null;
        int detected = 0;

        foreach (var enemy in enemies)
        {
            if (enemy.transform == transform) continue;

            bool isEnemy = false;
            if (CompareTag("RedTeam"))
                isEnemy = enemy.CompareTag("BlueTeam");
            else if (CompareTag("BlueTeam"))
                isEnemy = enemy.CompareTag("RedTeam");

            if (!isEnemy) continue;

            if (InFOV(enemy.transform))
            {
                float dist = Vector3.Distance(transform.position, enemy.transform.position);
                if (dist < minDist)
                {
                    minDist = dist;
                    nearest = enemy.transform;
                    detected++;
                }
            }
        }

        return nearest;
    }

    public bool EnemyInSight()
    {
        return GetEnemyTarget() != null;
    }

    public Vector3 GetNearestEnemyPosition()
    {
        Transform t = GetEnemyTarget();
        return t != null ? t.position : transform.position;
    }

    public bool CanAttack()
    {
        return Time.time >= _lastAttackTime + AttackCooldown;
    }

    public void PerformAttack()
    {
        _lastAttackTime = Time.time;
    }

    public void TakeDamage(int amount)
    {
        if (isHealing) return;
        Health -= amount;
        Debug.Log($"{name} recibió {amount} de daño. Vida actual: {Health}");

        if (Health <= 0)
        {
            fsm.ChangeState(EntitiesStates.Fleeing);
            return;
        }
        if (Health <= FleeThreshold && fsm.CurrentState != null && !(fsm.CurrentState is EntityFlee))
        {
            Debug.Log($"{name} entra en estado Flee porque la vida ({Health}) <= FleeThreshold ({FleeThreshold})");
            fsm.ChangeState(EntitiesStates.Fleeing);
        }
    }

    // Si querés los OnDrawGizmosSelected para ver el FOV:
    private void OnDrawGizmosSelected()
    {
        // Dibuja el radio de visión
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, visionRadius);

        // Dibuja el ángulo de visión
        Vector3 left = Quaternion.Euler(0, -visionAngle / 2, 0) * transform.forward;
        Vector3 right = Quaternion.Euler(0, visionAngle / 2, 0) * transform.forward;
        Gizmos.color = Color.cyan;
        Gizmos.DrawLine(transform.position, transform.position + left * visionRadius);
        Gizmos.DrawLine(transform.position, transform.position + right * visionRadius);
    }
}
