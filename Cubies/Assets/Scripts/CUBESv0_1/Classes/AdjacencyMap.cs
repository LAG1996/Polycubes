using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AdjacencyMap {

    //*******************
    //  PRIVATE VARS
    //*******************
    private Dictionary<Transform, Node> Nodes;
    private Dictionary<string, List<Transform>> HingePair;
    private List<AdjacencyMap> SubMaps;
	//*******************
    //  CONSTRUCTOR
    //*******************
	public AdjacencyMap ()
    {
        Nodes = new Dictionary<Transform, Node>();
        HingePair = new Dictionary<string, List<Transform>>();
        SubMaps = new List<AdjacencyMap>();
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

    public void SetNeighborHinges(Transform hinge_1, Transform hinge_2, string EdgePosition)
    {
        if(!HingePair.ContainsKey(EdgePosition))
        {
            Debug.Log(hinge_1.parent.name + "'s hinge and " + hinge_2.parent.name + "'s hinge at " + EdgePosition);
            List<Transform> E = new List<Transform>();
            E.Add(hinge_1);
            E.Add(hinge_2);
            HingePair.Add(EdgePosition, E);
        }  
    }

    public void DisconnectFacesByEdge(string EdgePosition)
    {
        List<Transform> Pair = HingePair[EdgePosition];

        Nodes[Pair[0].parent].RemoveNeighbor(Nodes[Pair[1]]);
        Nodes[Pair[1].parent].RemoveNeighbor(Nodes[Pair[0]]);
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
        Debug.Log("-------FACE : NEIGHBORS--------");
        foreach(Transform k in Nodes.Keys)
        {
            string neighbors = "";

            foreach(Node n in Nodes[k].Neighbors)
            {
                neighbors += n.face.name +",";
            }

            Debug.Log(k.name + " : {" + neighbors + "}");
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
