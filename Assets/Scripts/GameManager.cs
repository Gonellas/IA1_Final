using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("Pathfinding")]
    [SerializeField] private Node[] _nodes;
    public LayerMask _wallsLayer;
    public LayerMask WallsLayer => _wallsLayer;

    public Pathfinding Pathfinding { get; private set; }

    [Header("Canvas")]
    public GameObject winCanvas;
    public TMPro.TextMeshProUGUI winText;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        Pathfinding = FindObjectOfType<Pathfinding>();

        if (Pathfinding == null)
        {
            Debug.LogError("Pathfinding no encontrado en la escena.");
        }
    }

    public void TeamDefeated(bool isRedTeam)
    {
        Debug.Log("TeamDefeated llamado");
        if (winCanvas == null || winText == null)
        {
            Debug.LogError("winCanvas o winText no asignados!");
            return;
        }
        winCanvas.SetActive(true);
        winText.text = isRedTeam ? "¡Blue Team Wins!" : "¡Red Team Wins!";
    }


    public bool InSight(Vector3 start, Vector3 end)
    {
        Vector3 dir = end - start;
        float dist = dir.magnitude;
        return !Physics.Raycast(start, dir.normalized, dist, _wallsLayer);
    }

    public Node ClosestNode(Vector3 position)
    {
        Node closest = null;
        float minDistance = Mathf.Infinity;

        foreach (Node node in _nodes)
        {
            float dist = Vector3.Distance(position, node.transform.position);
            if (dist < minDistance)
            {
                minDistance = dist;
                closest = node;
            }
        }

        return closest;
    }
}

