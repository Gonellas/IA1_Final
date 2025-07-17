using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Node : MonoBehaviour
{
    public List<Node> neighbors = new List<Node>();
    public int cost = 1;
    public bool isBlocked = false;

    public List<Node> GetNeighbors()
    {
        return neighbors;
    }

    private void OnDrawGizmos()
    {
        foreach (var node in neighbors)
        {
            Gizmos.DrawLine(transform.position, node.transform.position);
        }
    }
}
