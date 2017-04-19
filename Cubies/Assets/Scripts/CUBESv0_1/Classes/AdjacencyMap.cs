using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Adjacency Map class
/*
     * SPECIFICATION:
     * ------------------Attribute: Description----------------
     *      Dictionary<Transform, Node> Nodes : A mapping from a face's transform from Unity's built-in library to a Node object.
     *      
     *      Queue<Node> VisitedNodes : A list of nodes visited when the graph is traversed.
     *      
     * ------------------Method : Input : Output : Description----------------
     *      
     *      AdjacencyMap : N/A : N/A : Constructor method. Initializes attributes.
     *      
     *      AddNeighbors : Transform,Transform : N/A : Adds an edge in the adjacency map between the nodes that are represented by the two inputted transforms.
     *      
     *      CheckForValidCuts : HingeMap, out List<Transforms> : List<Transforms> (BY REFERENCE) : Adds temporary cuts (or removes edges temporarily) adjacent to existing
     *                                      cuts and marks edges that would cause the graph to be disconnected when cut as unviable cuts. The validity of a cut is 
     *                                      determined in the method CutIfAllowed.
     *      
     *      CutIfAllowed : Transform, HingeMap, out List<Transform> : bool : Uses BFS traversal to determine whether a graph is disconnected after a cut is made. If the
     *                                      case is yes, the graph is disconnected, the edge removed by the cut is re-added to the graph and the function returns a FALSE
     *                                      flag.
     *                                      
     *      Reconnect : Node, Node, Transform, HingeMap : N/A : Reconnects inputted nodes by calling AddNeighbors and sets the edge that connects them as an UNTOUCHED edge,
     *                                        effectively reversing a cut, which would have the edge in a CUT state.
     *      
     *      InSameSubGraph : Node, Node : bool : Does a BFS traversal of the sub graph and checks if there exists a path between two edges. Returns true if a path exists, false if not.
     *      
     *      DisconnectFacesByEdge : Transform, HingeMap, out List<Transform> : N/A : Removes edge between two nodes in the graph (the edge and the nodes being mapped to the inputted Transform)
     *                                                     and also sets the state of the corresponding node in the HingeMap to CUT.
     *                                                     
     *      AddNewNode : Transform : N/A : Maps the inputted transform to a new node in the adjacency map.
     *      
     *      NodeExists : Transform : bool : Returns true if the transform maps to a node in the adjacency map. False if not.
     */
public class AdjacencyMap {

    //*******************
    //  PRIVATE VARS
    //*******************
    private Dictionary<Transform, Node> Nodes;
    private Queue<Node> VisitedNodes;
    //*******************
    //  CONSTRUCTOR
    //*******************
    public AdjacencyMap()
    {
        Nodes = new Dictionary<Transform, Node>();
        VisitedNodes = new Queue<Node>();
    }

    public void AddNeighbors(Transform node, Transform neighbor)
    {
        if (!Nodes.ContainsKey(node))
        {
            AddNewNode(node);
        }

        if (!Nodes.ContainsKey(neighbor))
        {
            AddNewNode(neighbor);
        }

        Nodes[node].AddNeighbor(Nodes[neighbor]);
        Nodes[neighbor].AddNeighbor(Nodes[node]);

    }
    
    public bool CutIfAllowed(Transform hinge, HingeMap HM, out List<Transform> ValidPair)
    {
        bool canCut = true;
        ValidPair = HM.GetHingePair(hinge);

            Node n1 = Nodes[ValidPair[0]];
            Node n2 = Nodes[ValidPair[1]];


            DisconnectFacesByEdge(hinge, HM, out ValidPair);
        
            if(!InSameSubGraph(n1, n2))
            {
                Reconnect(n1, n2, hinge, HM);
                canCut = false;
            }

        while(VisitedNodes.Count > 0)
        {
            VisitedNodes.Dequeue().visited = false;
        }

        return canCut;
    }

    public void GetSubGraphsAroundUnfoldLine(HingeMap HM, Transform hinge, ref List<Transform> SG_1, ref List<Transform> SG_2)
    {
        List<Transform> Line = HM.GetUnfoldingLineFromHinge(hinge);
        List<Transform> Pair = HM.GetHingePair(hinge);

        List<Transform> Dummy = new List<Transform>();

        if(Line.Count > 0)
        {
            foreach (Transform H in Line)
            {
                DisconnectFacesByEdge(H, HM, out Dummy);
            }

            Node n1 = Nodes[Pair[0]];
            Node n2 = Nodes[Pair[1]];

            SG_1 = GetGraph(n1);
            SG_2 = GetGraph(n2);


            foreach (Transform H in Line)
            {
                ReconnectAroundEdge(H, HM, out Dummy);
            }

            while (VisitedNodes.Count > 0)
            {
                VisitedNodes.Dequeue().visited = false;
            }
        }
    }

    private List<Transform> GetGraph(Node N)
    {
        List<Transform> Faces = new List<Transform>();
        Queue<Node> N_Faces = new Queue<Node>();
        N_Faces.Enqueue(N);

        Faces.Add(N.body);
        N.visited = true;
        VisitedNodes.Enqueue(N);

        while(N_Faces.Count > 0)
        {
            Node Current = N_Faces.Dequeue();

            foreach(Node Neighbor in Current.Neighbors)
            {
                if(!Neighbor.visited)
                {
                    N_Faces.Enqueue(Neighbor);

                    Faces.Add(Neighbor.body);
                    Neighbor.visited = true;
                    VisitedNodes.Enqueue(Neighbor);
                }
            }
        }

        return Faces;
    }

    private void TempCut(Node n1, Node n2)
    {
        n1.RemoveNeighbor(n2);
        n2.RemoveNeighbor(n1);
    }

    public List<Transform> FindInvalidEdges(HingeMap HM)
    {
        List<Transform> Edges = HM.GetNeighborsToCuts();
        List<Transform> InvalidCuts = new List<Transform>();

        foreach (Transform H in Edges)
        {
            List<Transform> Pair;

            if(!CutIfAllowed(H, HM, out Pair))
            {
                InvalidCuts.Add(Pair[0]);
                InvalidCuts.Add(Pair[1]);
            }
            else
            {
                ReconnectAroundEdge(H, HM, out Pair);
            }      
        }

        return InvalidCuts;
    }

    public void ReconnectAroundEdge(Transform hinge, HingeMap HM, out List<Transform> Pair)
    {
        Pair = HM.GetHingePair(hinge);

        Node n1 = Nodes[Pair[0]];
        Node n2 = Nodes[Pair[1]];

        Reconnect(n1, n2, hinge, HM);
    }
    
    //Reconnect faces and return the edge between them to an "UNTOUCHED" state.
    private void Reconnect(Node node_1, Node node_2, Transform hinge, HingeMap HM)
    {
        AddNeighbors(node_1.face, node_2.face);
        HM.RemoveCut(hinge);
    }

    //Do a BFS of the adjacency map from face_1. Return true if face_2 is found.
    private bool InSameSubGraph(Node face_1, Node face_2)
    {
        if(face_1 == face_2)
        {
            return true;
        }
        else
        {
            face_1.visited = true;
            VisitedNodes.Enqueue(face_1);
            bool found = false;
                foreach (Node N in face_1.Neighbors)
                {
                    if(!N.visited)
                        found = InSameSubGraph(N, face_2);
                    if (found)
                        break;
                }

            return found;
        }
    }

    private void DisconnectFacesByEdge(Transform hinge, HingeMap HM, out List<Transform> Pair)
    {
        Pair = HM.GetHingePair(hinge);
        HM.AddNewCut(hinge);

        Nodes[Pair[0]].RemoveNeighbor(Nodes[Pair[1]]);
        Nodes[Pair[1]].RemoveNeighbor(Nodes[Pair[0]]);
    }

    private void AddNewNode(Transform t)
    {
        Node newNode = new Node(t);

        Nodes.Add(t, newNode);
        foreach (Transform P in t)
        {
            Nodes.Add(P, newNode);
        }
    }

    public bool NodeExists(Transform face)
    {
        return Nodes.ContainsKey(face);
    }

    public Transform GetBodyFromHinge(Transform hinge)
    {
        return Nodes[hinge].body;
    }

    public List<Transform> GetHingesOfNode(Transform T)
    {
        return Nodes[T].Hinges;
    }

    public List<Transform> GetFaces()
    {
        List<Transform> faces = new List<Transform>();
        foreach(Transform F in Nodes.Keys)
        {
            faces.Add(F);
        }

        return faces;
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

    //Node class for the adjacency map
    /*
     * SPECIFICATION:
     * ------------------Attribute: Description----------------
     *      Transform face : Points to the Transform of a Unity Game object that represents a face on the polycube (this includes the face's body and edges, which
     *                      are separate children to the face object)
     *      
     *      
     *      List<Node> Neighbors : Points to a list of each face's neighbors on the graph (i.e. all faces adjacent to the face in question)
     *      
     *      
     *      bool visited : Helper variable for adjacency graph traversal that tells whether this node was visited during traversal. Should be reset at the end of traversal.
     *      
     *      
     * ------------------Method : Input : Output : Description----------------
     *      Node : Transform : N/A : Constructor for Node object. Initializes attributes, setting its face transform to be the transform in its input parameter.
     *      
     *      AddNeighbor : Node : N/A : Adds a neighbor to the node's list of neighbors. Does not allow for duplicate neighbors.
     *      
     *      RemoveNeighbor : Node : N/A : Removes the node inputted from the current node's list of neighbors if that inputted node is indeed in that list.
     *      
     *      NeighborExists : Node : bool : Tells us whether the current node has the inputted node as a neighbor
     *      
     *      ~Node : N/A : N/A : Deconstructor. Just does some trash cleanup. 
     */
    protected class Node
    {
        public Transform face;
        public Transform body;
        public List<Node> Neighbors;
        public List<Transform> Hinges;

        public bool visited;
        
        public Node(Transform face)
        {
            this.face = face;
            this.body = face.FindChild("body");
            Hinges = new List<Transform>();

            foreach(Transform H in face)
            {
                if(H.name == "edge")
                {
                    Hinges.Add(H);
                }
            }

            Neighbors = new List<Node>();
            visited = false;
        }
        
        public void AddNeighbor(Node f)
        {
            if(!NeighborExists(f))
                Neighbors.Add(f);
        }

        public void RemoveNeighbor(Node f)
        {
            if(NeighborExists(f))
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
