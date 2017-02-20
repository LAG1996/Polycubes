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

    //Do a BFS of the adjacency map from face_1. Return if face_2 is found.
    public bool InSameSubGraph(Transform face_1, Transform face_2)
    {
        return false;
    }

    public void DisconnectFacesByEdge(Transform hinge, HingeMap HM)
    {
        List<Transform> Pair = HM.GetHingePair(hinge);
        
        if(HM.GetEdgeState(hinge) == "UNTOUCHED")
        {
            Debug.Log("Disconnecting " + Pair[0].parent.name + " and " + Pair[1].parent.name);

            HM.SetEdgeState("CUT", hinge);

            Nodes[Pair[0].parent].RemoveNeighbor(Nodes[Pair[1].parent]);
            Nodes[Pair[1].parent].RemoveNeighbor(Nodes[Pair[0].parent]);
        }
        
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
