using UnityEngine;

public enum LeaderStates
{
    Idle,
    Moving,
    Attacking
}

[RequireComponent(typeof(Collider))]
public class Leader : MonoBehaviour, IDamageable
{
    public LeaderFSM LeaderFSM { get; private set; }
    public Vector3 TargetPosition { get; private set; }
    public float Speed => _speed;
    public float StopDistance => _stopDistance;
    public float ViewRadius => _viewRadius;
    public float ViewAngle => _viewAngle;
    public bool IsRedTeam => _isRedTeam;
    public float AttackDamage => _attackDamage;

    [Header("Settings")]
    [SerializeField] private bool _isRedTeam = true;
    [SerializeField] private float _speed = 5f;
    [SerializeField] private float _stopDistance = 0.5f;
    [SerializeField] private float _viewRadius = 10f;
    [SerializeField] private float _viewAngle = 90f;

    [Header("Combat")]
    [SerializeField] private int _maxHealth = 100;
    [SerializeField] private float _attackCooldown = 2f;
    [SerializeField] private float _attackDamage = 10f;
    [SerializeField] private int _currentHealth;
    private float _lastAttackTime = 0f;

    [Header("Obstacle Avoidance")]
    public float avoidDistance = 2f;
    public float avoidForce = 6f;
    public LayerMask obstacleLayer;

    public ObstacleAvoidance obstacleAvoidance { get; private set; }


    private void Start()
    {
        _currentHealth = _maxHealth;

        obstacleAvoidance = new ObstacleAvoidance(
            transform,
            avoidDistance,
            avoidForce,
            obstacleLayer
        );


        LeaderFSM = gameObject.AddComponent<LeaderFSM>();
        LeaderFSM.AddState(LeaderStates.Idle, new LeaderIdle(this));
        LeaderFSM.AddState(LeaderStates.Moving, new LeaderMovement(this));
        LeaderFSM.AddState(LeaderStates.Attacking, new LeaderAttack(this));
        LeaderFSM.ChangeState(LeaderStates.Idle);
    }


    private void Update()
    {
        if (GameManager.Instance.isGameOver)
            return;

        LeaderFSM.Update();

        if (_isRedTeam && Input.GetMouseButtonDown(0)) SetTargetFromMouse();
        else if (!_isRedTeam && Input.GetMouseButtonDown(1)) SetTargetFromMouse();
    }

    private void SetTargetFromMouse()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            TargetPosition = hit.point;
            LeaderFSM.ChangeState(LeaderStates.Moving);
        }
    }

    public bool InFOV(Transform target)
    {
        if (target == null) return false;

        Vector3 dirToTarget = (target.position - transform.position).normalized;
        float angle = Vector3.Angle(transform.forward, dirToTarget);
        float dist = Vector3.Distance(transform.position, target.position);

        if (angle < ViewAngle / 2 && dist <= ViewRadius)
        {
            if (!Physics.Linecast(transform.position, target.position, GameManager.Instance._wallsLayer))
                return true;
        }
        return false;
    }

    public void TakeDamage(int damage)
    {
        _currentHealth -= damage;

        if (_currentHealth <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        GameManager.Instance.TeamDefeated(_isRedTeam);
        gameObject.SetActive(false);
    }

    public bool CanAttack()
    {
        return Time.time >= _lastAttackTime + _attackCooldown;
    }

    public void PerformAttack()
    {
        _lastAttackTime = Time.time;
    }

    public Transform GetEnemyTarget()
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, ViewRadius);
        float minDist = Mathf.Infinity;
        Transform nearest = null;

        foreach (var hit in hits)
        {
            bool isEnemy = (IsRedTeam && hit.CompareTag("BlueTeam")) || (!IsRedTeam && hit.CompareTag("RedTeam"));

            if (!isEnemy) continue;

            if (InFOV(hit.transform))
            {
                float dist = Vector3.Distance(transform.position, hit.transform.position);

                if (dist < minDist)
                {
                    minDist = dist;
                    nearest = hit.transform;
                }
            }
        }
        return nearest;
    }

    public bool CanSeeEnemy(Transform target)
    {
        return InFOV(target);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, ViewRadius);
        Vector3 left = Quaternion.Euler(0, -ViewAngle / 2, 0) * transform.forward;
        Vector3 right = Quaternion.Euler(0, ViewAngle / 2, 0) * transform.forward;
        Gizmos.color = Color.cyan;
        Gizmos.DrawLine(transform.position, transform.position + left * ViewRadius);
        Gizmos.DrawLine(transform.position, transform.position + right * ViewRadius);
    }
}
