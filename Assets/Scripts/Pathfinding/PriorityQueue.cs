using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PriorityQueue
{
    Dictionary<Node, float> _allNodes = new Dictionary<Node, float>();
    public int Count { get { return _allNodes.Count; } }

    //nos pide un nodo y el peso
    public void Put(Node node, float cost)
    {
        //si lo tiene lo sobreescribimos con el costo
        if (_allNodes.ContainsKey(node)) _allNodes[node] = cost;
        //si no lo tiene lo añadimos
        else _allNodes.Add(node, cost);
    }

    public Node Get()
    {
        Node node = null;
        //queremos devolver el nodo con menor costo
        float cost = Mathf.Infinity;

        //si el valor del nodo en el diccionario es menor al costo, lo guardamos
        foreach (var n in _allNodes)
        {
            if (n.Value < cost)
            {
                node = n.Key;
                cost = n.Value;
            }
        }
        _allNodes.Remove(node);

        return node;
    }
}
