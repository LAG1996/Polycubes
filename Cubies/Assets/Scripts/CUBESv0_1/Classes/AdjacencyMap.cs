using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AdjacencyMap {

    //*******************
    //  PRIVATE VARS
    //*******************
    private Dictionary<Vector3, Node> Nodes;
	//*******************
    //  CONSTRUCTOR
    //*******************
	public AdjacencyMap ()
    {
        Nodes = new Dictionary<Vector3, Node>();
	}

    public void AddAdjacentFaces(Transform face_1, Transform face_2)
    {
        Node n = new Node(face_1);
        Node m = new Node(face_2);

        if (NodeExists(face_1))
        {
            Node k;
            Nodes.TryGetValue(face_1.position, out k);

            if(!k.NeighborExists(m))
                k.AddNeighbor(m);
        }
        else if (NodeExists(face_2))
        {
            Node k;
            Nodes.TryGetValue(face_2.position, out k);

            if (!k.NeighborExists(n))
                k.AddNeighbor(n); ;
        }
        else
        {
            Nodes.Add(face_1.position, n);
            Nodes.Add(face_2.position, m);
        }
    }

    public bool NodeExists(Transform face)
    {
        return Nodes.ContainsKey(face.position);
    }

    public bool NodeExists(Vector3 pos)
    {
        return Nodes.ContainsKey(pos);
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
        private Transform face;
        private Dictionary<Vector3, Node> Neighbors;
        
        public Node(Transform face)
        {
            this.face = face;
            Neighbors = new Dictionary<Vector3, Node>();
        }
        
        public void AddNeighbor(Node f)
        {
            this.Neighbors.Add(f.face.position, f);
        }

        public void RemoveNeighborAtPosition(Vector3 pos)
        {
            Neighbors.Remove(pos);
        }

        public bool NeighborExists(Node n)
        {
            return Neighbors.ContainsValue(n);
        }

        ~Node()
        {
            Neighbors.Clear();
            Neighbors = null;
        }
    }
}
