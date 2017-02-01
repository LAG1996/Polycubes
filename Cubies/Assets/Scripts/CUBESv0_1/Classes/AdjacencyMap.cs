using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AdjacencyMap {

    //*******************
    //  PRIVATE VARS
    //*******************
    private Dictionary<Transform, Node> Nodes;
	//*******************
    //  CONSTRUCTOR
    //*******************
	public AdjacencyMap ()
    {
        Nodes = new Dictionary<Transform, Node>();
	}

    public void AddNeighbors(Transform node, Transform neighbor)
    {
        if(!Nodes.ContainsKey(node))
        {
            AddNewNode(node);
        }

        if(!Nodes.ContainsKey(neighbor))
        {
            AddNewNode(neighbor);
        }

        Nodes[node].AddNeighbor(Nodes[neighbor]);
        Nodes[neighbor].AddNeighbor(Nodes[node]);
        
    }

    private void AddNewNode(Transform t)
    {
        Node newNode = new Node(t);
        Nodes.Add(t, newNode);
    }

    public bool NodeExists(Transform face)
    {
        return Nodes.ContainsKey(face);
    }

    public void DataDump()
    {
        foreach(Transform k in Nodes.Keys)
        {
            string neighbors = "";

            foreach(Node n in Nodes[k].Neighbors)
            {
                neighbors += n.face.name +",";
            }

            Debug.Log(k.name + "'s neighbors: {" + neighbors + "}");
        }
    }

    public override string ToString()
    {
        return Nodes.Values.ToString();
    }

    ~AdjacencyMap()
    {
        Nodes.Clear();
        Nodes = null;
    }

    protected class Node
    {
        public Transform face;
        public List<Node> Neighbors;
        
        public Node(Transform face)
        {
            this.face = face;
            Neighbors = new List<Node>();
        }
        
        public void AddNeighbor(Node f)
        {
            if(!NeighborExists(f))
                Neighbors.Add(f);
        }

        public void RemoveNeighbor(Node f)
        {
            Neighbors.Remove(f);
        }

        public bool NeighborExists(Node n)
        {
            return Neighbors.Contains(n);
        }

        ~Node()
        {
            Neighbors.Clear();
            Neighbors = null;
        }
    }
}
