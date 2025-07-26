using UnityEngine;

public class AIAgent : MonoBehaviour, IDamageable
{
    public EntitiesFSM fsm;
    public Vector3 velocity { get; set; }
    public bool isHealing = false;
    [SerializeField] private bool _isRedTeam = true;

    [Header("Flocking")]
    public Transform leaderTransform;
    public float separationRadius = 2f;
    public float separationWeight = 1.5f;
    public float followSpeed = 3f;

    [Header("Combat")]
    public float health = 100f;
    public float maxHealth = 100f;
    public float fleeThreshold = 10f;
    public float attackRange = 2f;
    public float attackCooldown = 2f;
    public int attackDamage = 5;
    private float _lastAttackTime = 0f;

    [Header("Flee")]
    public float fleeSpeed = 4f;
    public Transform safeZone; 

    [Header("Enemy Detection")]
    public LayerMask enemyLayer;
    public float visionRadius = 10f;
    public float visionAngle = 90f;

    public float safeZoneRadius = 1f;

    [Header("Obstacle Avoidance")]
    public float avoidDistance = 2f;
    public float avoidForce = 6f;
    public LayerMask obstacleLayer;

    public ObstacleAvoidance obstacleAvoidance;

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

        if (GameManager.Instance.isGameOver)
            return;

        fsm.Update();

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

        if (isHealing && health < maxHealth)
        {
            health += 20f * Time.deltaTime;
            if (health > maxHealth) health = maxHealth;
        }
    }

    public bool InFOV(Transform target)
    {
        if (target == null) return false;

        Vector3 dirToTarget = (target.position - transform.position).normalized;
        float angle = Vector3.Angle(transform.forward, dirToTarget);
        float dist = Vector3.Distance(transform.position, target.position);

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
        return Time.time >= _lastAttackTime + attackCooldown;
    }

    public void PerformAttack()
    {
        _lastAttackTime = Time.time;
    }

    public void TakeDamage(int amount)
    {
        if (isHealing) return;

        health -= amount;

        if (health <= 0)
        {
            fsm.ChangeState(EntitiesStates.Fleeing);
            return;
        }
        if (health <= fleeThreshold && fsm.CurrentState != null && !(fsm.CurrentState is EntityFlee))
        {
            fsm.ChangeState(EntitiesStates.Fleeing);
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, visionRadius);

        Vector3 left = Quaternion.Euler(0, -visionAngle / 2, 0) * transform.forward;
        Vector3 right = Quaternion.Euler(0, visionAngle / 2, 0) * transform.forward;
        Gizmos.color = Color.cyan;
        Gizmos.DrawLine(transform.position, transform.position + left * visionRadius);
        Gizmos.DrawLine(transform.position, transform.position + right * visionRadius);
    }
}
