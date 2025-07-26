using System.Collections.Generic;
using UnityEngine;

public class Pathfinding : MonoBehaviour
{
    public List<Node> AStar(Node start, Node end)
    {
        if (start == null || end == null) return null;

        PriorityQueue openSet = new PriorityQueue();
        Dictionary<Node, Node> cameFrom = new Dictionary<Node, Node>();
        Dictionary<Node, float> costSoFar = new Dictionary<Node, float>();

        openSet.Put(start, 0);
        cameFrom[start] = null;
        costSoFar[start] = 0;

        while (openSet.Count > 0)
        {
            Node current = openSet.Get();

            if (current == end) return ReconstructPath(cameFrom, current);

            foreach (Node neighbor in current.GetNeighbors())
            {
                if (neighbor.isBlocked) continue;

                float newCost = costSoFar[current] + Vector3.Distance(current.transform.position, neighbor.transform.position);

                if (!costSoFar.ContainsKey(neighbor) || newCost < costSoFar[neighbor])
                {
                    costSoFar[neighbor] = newCost;
                    float priority = newCost + Vector3.Distance(neighbor.transform.position, end.transform.position);
                    openSet.Put(neighbor, priority);
                    cameFrom[neighbor] = current;
                }
            }
        }

        return null; // no path found
    }

    public List<Node> ThetaStar(Node start, Node end)
    {
        List<Node> path = AStar(start, end);
        if (path == null || path.Count < 3) return path;

        int current = 0;
        int nextNext = current + 2;

        while (nextNext < path.Count)
        {
            if (HasLineOfSight(path[current].transform.position, path[nextNext].transform.position))
            {
                path.RemoveAt(current + 1); 
            }
            else
            {
                current++;
                nextNext++;
            }
        }

        return path;
    }

    private bool HasLineOfSight(Vector3 start, Vector3 end)
    {
        Vector3 dir = end - start;
        float dist = dir.magnitude;

        return !Physics.Raycast(start, dir.normalized, dist, GameManager.Instance.WallsLayer);
    }

    private List<Node> ReconstructPath(Dictionary<Node, Node> cameFrom, Node current)
    {
        List<Node> path = new List<Node>();

        while (current != null)
        {
            path.Add(current);
            current = cameFrom[current];
        }

        path.Reverse();

        return path;
    }
}
