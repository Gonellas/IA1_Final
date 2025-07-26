using UnityEngine;

public class ObstacleAvoidance
{
    private Transform _transform;
    private float _avoidDistance;
    private float _avoidForce;
    private LayerMask _obstacleLayer;

    public ObstacleAvoidance(Transform transform, float avoidDistance, float avoidForce, LayerMask obstacleLayer)
    {
        _transform = transform;
        _avoidDistance = avoidDistance;
        _avoidForce = avoidForce;
        _obstacleLayer = obstacleLayer;
    }

    public Vector3 GetAvoidanceForce(Vector3 currentDirection)
    {
        RaycastHit hit;
        Vector3 origin = _transform.position + Vector3.up * 0.5f;
        Vector3 rayDir = currentDirection.normalized;

        Debug.DrawRay(origin, rayDir * _avoidDistance, Color.red, 0.1f);

        if (Physics.Raycast(origin, rayDir, out hit, _avoidDistance, _obstacleLayer))
        {
            Vector3 avoidDir = Vector3.Cross(hit.normal, Vector3.up).normalized;
            Debug.DrawRay(hit.point, avoidDir * _avoidForce, Color.green, 0.2f);

            return avoidDir * _avoidForce;
        }

        return Vector3.zero;
    }
}
