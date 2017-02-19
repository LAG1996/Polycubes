using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AdjacencyMap {

    //*******************
    //  PRIVATE VARS
    //*******************
    private Dictionary<Transform, Node> Nodes;
    private Dictionary<string, List<Transform>> HingePair;
    private Dictionary<Transform, bool> IsActiveHinge;
	//*******************
    //  CONSTRUCTOR
    //*******************
	public AdjacencyMap ()
    {
        Nodes = new Dictionary<Transform, Node>();
        HingePair = new Dictionary<string, List<Transform>>();
        IsActiveHinge = new Dictionary<Transform, bool>();
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
            List<Transform> E = new List<Transform>();
            E.Add(hinge_1);
            E.Add(hinge_2);
            HingePair.Add(EdgePosition, E);

            IsActiveHinge.Add(hinge_1, true);
            IsActiveHinge.Add(hinge_2, true);
        }  
    }

    //Do a BFS of the adjacency map from face_1. Return if face_2 is found.
    public bool InSameSubGraph(Transform face_1, Transform face_2)
    {
        return false;
    }

    public bool EdgesOnSameNormal(string hinge_1, string hinge_2, out Vector3 normal)
    {
        List<Transform> pair_1 = HingePair[hinge_1];
        List<Transform> pair_2 = HingePair[hinge_2];


        for (int i = 0; i < 2; i++)
            for (int j = 0; j < 2; j++)
            {
                if (pair_1[i].parent.up == pair_2[j].parent.up)
                {
                    normal = pair_1[i].parent.up;
                    return true;
                }
            }
        normal = Vector3.zero;
        return false;
    }

    public void DisconnectFacesByEdge(string EdgePosition, ref List<Transform> Path)
    {
        List<Transform> Pair = HingePair[EdgePosition];
        
        if(IsActiveHinge[Pair[0]])
        {
            Debug.Log("Disconnecting " + Pair[0].parent.name + " and " + Pair[1].parent.name);
            Path.Add(Pair[0]);

            IsActiveHinge[Pair[0]] = false;
            IsActiveHinge[Pair[1]] = false;

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
