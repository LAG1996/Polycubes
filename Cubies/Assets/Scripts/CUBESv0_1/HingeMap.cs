using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Hinge Map Class
/*
     * SPECIFICATION:
     * ------------------Attribute: Description----------------
     *      Dictionary<Transform, EdgeNode> HingeToEdge : A mapping from a face's transform from Unity's built-in library to an EdgeNode object.
     *      
     *      Dictionary<Transform, EdgeNode> HingeToCut : A mapping from a face's transform from Unity's built-in library to an EdgeNode object that is listed as being "CUT"
     *      
     *      List<Node> CutEdges : The set of all edges in the adjacency map that were cut, or removed
     *      
     *      List<EdgeNode> ListOfEdges: The list of all EdgeNodes in the map (may be unnecessary since this is just the list of all values in HingeToEdge)
     *      
     * ------------------Method : Input : Output : Description----------------
     *      public HingeMap : N/A : N/A : Constructor class. Initializes all the of the attributes as empty sets.
     *      
     *      public SetEdge : Transform, Transform, Vector3 : N/A : Takes two incident hinges and maps them to an EdgeNode on the map.
     *      
     *      public GetHingePair : Transform : List<Transform> : Finds the hinge incident to the hinge inputted and then returns the pairing as a list.
     *      
     *      public EdgesOnSamePlane : Transform, Transform : bool : Entry-point for EdgesOnSamePlane method. Returns true if the two edges mapped to the inputted Transforms
     *                                      are co-planar.
     *                                      
     *      private EdgesOnSamePlane : EdgeNode, EdgeNode : bool : Checks if two EdgeNodes are co-planar in orthogonal space. Returns true if that is the case.
     *      
     *      public AreAdjacentEdges : Transform, Transform : bool : Entry-point for AreAdjacentEdges method. Returns true if the two edges mapped to the inputted Transforms
     *                                      share an endpoint (i.e. are adjacent).
     *                                      
     *      private AreAdjacentEdges : EdgeNode, EdgeNode : bool : Checks if two EdgeNodes share an endpoint (i.e. are adjacent) and are not incident.
     *      
     *      
     */
public class HingeMap {

    private Dictionary<Transform, EdgeNode> HingeToEdge;
    private Dictionary<Transform, EdgeNode> HingeToCut;
    private Dictionary<Transform, EdgeNode> HingeToInvalid;
    private Dictionary<EdgeNode, List<EdgeNode>> EdgeToListOfUnfolding;
    //private Dictionary<Transform, SingleFace> HingeToParentFace;

    private List<EdgeNode> CutEdges;
    private List<EdgeNode> ListOfEdges;
    private List<EdgeNode> InvalidEdges;

    private Queue<EdgeNode> VisitedEdges;

    public HingeMap()
    {
        HingeToEdge = new Dictionary<Transform, EdgeNode>();
        HingeToCut = new Dictionary<Transform, EdgeNode>();
        HingeToInvalid = new Dictionary<Transform, EdgeNode>();
        //HingeToParentFace = new Dictionary<Transform, SingleFace>();
        CutEdges = new List<EdgeNode>();
        ListOfEdges = new List<EdgeNode>();
        InvalidEdges = new List<EdgeNode>();
        EdgeToListOfUnfolding = new Dictionary<EdgeNode, List<EdgeNode>>();

        VisitedEdges = new Queue<EdgeNode>();
    }

    public void SetEdge(Transform hinge_1, Transform hinge_2, Vector3 OriginalPos)
    {
        if(!HingeToEdge.ContainsKey(hinge_1))
        {
            EdgeNode E = new EdgeNode(hinge_1, hinge_2, OriginalPos);
     
            if (ListOfEdges.Count >= 1)
            {
                SetAdjacentEdges(E);
                ListOfEdges.Add(E);
            }      
            else
                ListOfEdges.Add(E);

            //Set hinge_1 and hinge_2 to be keys that map to edge E
            HingeToEdge.Add(hinge_1, E);
            HingeToEdge.Add(hinge_2, E);
        }
            
    }

    public List<Transform> GetHingePair(Transform hinge)
    {
        return HingeToEdge[hinge].GetHinges();
    }

    /*
    public SingleFace GetParentFace(Transform hinge)
    {
        return HingeToParentFace[hinge];
    }*/

    public bool EdgesOnSamePlane(Transform hinge_1, Transform hinge_2)
    {
        return EdgesOnSamePlane(HingeToEdge[hinge_1], HingeToEdge[hinge_2]);
    }

    private bool EdgesOnSamePlane(EdgeNode edge_1, EdgeNode edge_2)
    {
        Vector3 commonNorm;

        if (edge_1 == edge_2)
            return true;

        if (EdgesOnSameNormal(edge_1, edge_2, out commonNorm))
        {
            Vector3 pos_1 = edge_1.GetLatticePosition();
            Vector3 pos_2 = edge_2.GetLatticePosition();

            if (commonNorm.x == 1.0f)
            {
                if ((pos_1.x - pos_2.x) != 0.0f)
                {
                    return false;
                }
            }
            else if (commonNorm.y == 1.0f)
            {
                if ((pos_1.y - pos_2.y) != 0.0f)
                {
                    return false;
                }
            }
            else
            {
                if ((pos_1.z - pos_2.z) != 0.0f)
                {
                    return false;
                }
            }

            return true;
        }

        return false;
    }

    //Input: Two Transforms that represent two edges of a graph.
    //Output: Whether the edges are adjacent (they have like endpoints and are not equivalent/incident).
    public bool AreAdjacentEdges(Transform hinge_1, Transform hinge_2)
    {
        return AreAdjacentEdges(HingeToEdge[hinge_1], HingeToEdge[hinge_2]);
    }

    //Utility method for AreAdjacentEdges
    private bool AreAdjacentEdges(EdgeNode edge_1, EdgeNode edge_2)
    {
        if(edge_1 != edge_2)
        {
            foreach (Vector3 e in edge_1.GetEndPoints())
            {
                if (e == edge_2.GetEndPoints()[0] || e == edge_2.GetEndPoints()[1])
                {
                    return true;
                }
            }
        }

        return false;
    }
    
    public void SetEdgeRelationships()
    {
        foreach(EdgeNode E in ListOfEdges)
        {
            foreach(EdgeNode F in ListOfEdges)
            {
                if(E != F)
                {
                    if (CheckEdgeRelation(E, F, "Collinear"))
                        E.CollinearEdges.Add(F);
                    else if (CheckEdgeRelation(E, F, "Parallel"))
                        E.ParallelEdges.Add(F);
                    else if (CheckEdgeRelation(E, F, "Perpendicular"))
                        E.PerpendicularEdges.Add(F);
                    
                }
            }
        }
    }

    private bool CheckEdgeRelation(EdgeNode e1, EdgeNode e2, string RelationType)
    {

        Vector3 A = e1.GetEndPoints()[0];
        Vector3 B = e1.GetEndPoints()[1];

        Vector3 C = e2.GetEndPoints()[0];
        Vector3 D = e2.GetEndPoints()[1];

        if(string.Compare(RelationType, "Collinear") == 0)
        {
            bool col = EndPointsAreCollinear(A, B, C, D);

            if (col)
            {
                if (e1.GetNeighbors().Contains(e2) && !e1.CollinearNeighbors.Contains(e2))
                {
                    e1.CollinearNeighbors.Add(e2);
                    e2.CollinearNeighbors.Add(e1);
                }
            }

            return col;
        }
        else if(string.Compare(RelationType, "Parallel") == 0)
        {
            return EdgesAreParallel(A, B, C, D);
        }
        else if (string.Compare(RelationType, "Perpendicular") == 0)
        {
            bool perp =  EdgesArePerpendicular(A, B, C, D);

            if(perp)
            {
                if(e1.GetNeighbors().Contains(e2) && !e1.PerpendicularNeighbors.Contains(e2))
                {
                    e1.PerpendicularNeighbors.Add(e2);
                    e2.PerpendicularNeighbors.Add(e1);
                }
            }

            return perp;
        }

        return false;
    }

    public bool EdgesArePerpendicular(Vector3 A, Vector3 B, Vector3 C, Vector3 D)
    {
        if ((EndPointsAreCollinear(A, C) && !EndPointsAreCollinear(A, D)) || (EndPointsAreCollinear(A, D) && !EndPointsAreCollinear(A, C)) || (!EndPointsAreCollinear(A, C) && !EndPointsAreCollinear(A, D)))
        {
            return EndPointsAreCollinear(B, D) && EndPointsAreCollinear(B, C);
        }
        else if ((EndPointsAreCollinear(B, C) && !EndPointsAreCollinear(B, D)) || (EndPointsAreCollinear(B, D) && !EndPointsAreCollinear(B, C)) || (!EndPointsAreCollinear(B, C) && !EndPointsAreCollinear(B, D)))
        {
            return EndPointsAreCollinear(A, D) && EndPointsAreCollinear(A, C);
        }
        return false;
    }

    public bool EdgesAreParallel(Vector3 A, Vector3 B, Vector3 C, Vector3 D)
    {
        if(EndPointsAreCollinear(A, C) && !EndPointsAreCollinear(A, D))
        {
            return EndPointsAreCollinear(B, D) && !EndPointsAreCollinear(B, C);
        }
        else if (EndPointsAreCollinear(A, D) && !EndPointsAreCollinear(A, C))
        {
            return EndPointsAreCollinear(B, C) && !EndPointsAreCollinear(B, D);
        }

        return false;
    }

    private bool EndPointsAreCollinear(Vector3 A, Vector3 B, Vector3 C, Vector3 D)
    {
        return (EndPointsAreCollinear(A, C) && EndPointsAreCollinear(A, D)) && (EndPointsAreCollinear(B, C) && EndPointsAreCollinear(B, D));
    }

    private bool EndPointsAreCollinear(Vector3 End_1, Vector3 End_2)
    {
        return Diff(PreciseVector.Vector3ToDecimalString(End_1, 1), PreciseVector.Vector3ToDecimalString(End_2, 1)) == 1 || Diff(PreciseVector.Vector3ToDecimalString(End_1, 1), PreciseVector.Vector3ToDecimalString(End_2, 1)) == 0;
    }

    public bool EdgesOnSameNormal(Transform hinge_1, Transform hinge_2, out Vector3 normal)
    {
        return EdgesOnSameNormal(HingeToEdge[hinge_1], HingeToEdge[hinge_2], out normal);
    }
    
    private bool EdgesOnSameNormal(EdgeNode edge_1, EdgeNode edge_2, out Vector3 normal)
    {
        List<Vector3> norm_1 = edge_1.GetNormal();
        List<Vector3> norm_2 = edge_2.GetNormal();
        normal = Vector3.zero;

        if (edge_1.GetEdgeType() == "SPECIAL" && edge_2.GetEdgeType() == "SPECIAL")
        {
            int match = 0;
            for(int i = 0; i < 2; i++)
                for(int j = 0; j < 2; j++)
                {
                    if(norm_1[i] == norm_2[j])
                    {
                        match++;
                    }
                }

            if(match == 2)
            {
                normal = norm_1[0];
            }
        }
        else if(edge_1.GetEdgeType() == "SPECIAL" && edge_2.GetEdgeType() != "SPECIAL")
        {
            for(int i = 0; i < 2; i++)
            {
                if (norm_1[i] == norm_2[0])
                {
                    normal = norm_2[0];
                }
            }
        }
        else if(edge_1.GetEdgeType() != "SPECIAL" && edge_2.GetEdgeType() == "SPECIAL")
        {
            for (int i = 0; i < 2; i++)
            {
                if (edge_2.GetNormal()[i] == norm_1[0])
                {
                    normal = norm_1[0];
                }
            }
        }
        else
        {
            if(norm_1[0] == norm_2[0])
            {
                normal = norm_1[0];
            }
        }

        return normal != Vector3.zero;
    }

    public void AddNewCut(Transform hinge)
    {
        if(!HingeToCut.ContainsKey(hinge))
        {
            EdgeNode E = HingeToEdge[hinge];
            CutEdges.Add(E);

            E.SetEdgeState("CUT");

            HingeToCut.Add(E.GetHinges()[0], E);
            HingeToCut.Add(E.GetHinges()[1], E);
        }
    }

    public void AddNewInvalid(Transform hinge)
    {
        if (!HingeToInvalid.ContainsKey(hinge))
        {
            EdgeNode E = HingeToEdge[hinge];
            InvalidEdges.Add(E);
            HingeToInvalid.Add(E.GetHinges()[0], E);
            HingeToInvalid.Add(E.GetHinges()[1], E);
        }
    }

    public void RemoveCut(Transform hinge)
    {
        if (!HingeToInvalid.ContainsKey(hinge))
        {
            EdgeNode E = HingeToEdge[hinge];
            CutEdges.Remove(E);

            E.SetEdgeState("UNTOUCHED");

            HingeToCut.Remove(E.GetHinges()[0]);
            HingeToCut.Remove(E.GetHinges()[1]);

        }
    }

    public void RemoveInvalid(Transform hinge)
    {
        if (!HingeToInvalid.ContainsKey(hinge))
        {
            EdgeNode E = HingeToEdge[hinge];
            InvalidEdges.Remove(E);
            HingeToInvalid.Remove(E.GetHinges()[0]);
            HingeToInvalid.Remove(E.GetHinges()[1]);
        }
    }


    private int Diff(string endPoint_1, string endPoint_2)
    {
        int diff_count = 0;
        string[] e1 = endPoint_1.Split(',');
        string[] e2 = endPoint_2.Split(',');

        for (int j = 0; j < 3; j++)
        {
            if (e1[j] != e2[j])
            {
                diff_count++;
            }
        }

        return diff_count;
    }

    private void SetAdjacentEdges(EdgeNode E)
    {
        int count = ListOfEdges.Count;

        for(int i = 0; i < count; i++)
        {
            if (AreAdjacentEdges(E, ListOfEdges[i]))
            {
                E.AddNewAdjacentEdge(ListOfEdges[i]);
                ListOfEdges[i].AddNewAdjacentEdge(E);
            }
        }
    }

    public void FindUnfoldingLines()
    {
        EdgeToListOfUnfolding = new Dictionary<EdgeNode, List<EdgeNode>>();
        foreach(EdgeNode Start in CutEdges)
        {
            List<EdgeNode> ClosestCuts = new List<EdgeNode>();
            //Look at adjacent edges on side 1
            foreach(EdgeNode N in Start.Neighbor_Side_1)
            {
                if(N.State != "CUT")
                {
                    List<EdgeNode> Path = new List<EdgeNode>();
                    EdgeNode End = CheckCollinear(Start, N, Start, ref Path);

                    if (End != null && PathExists(Start, End))
                    {
                        foreach(EdgeNode E in Path)
                        {
                            if (!EdgeToListOfUnfolding.ContainsKey(E))
                                EdgeToListOfUnfolding.Add(E, Path);
                        }
                    }
                }

                foreach(EdgeNode E in VisitedEdges)
                {
                    E.visited = false;
                }
            }

            //Look at adjacent edges on side 2
            foreach(EdgeNode N in Start.Neighbor_Side_2)
            {
                if (N.State != "CUT")
                {
                    List<EdgeNode> Path = new List<EdgeNode>();
                    EdgeNode End = CheckCollinear(Start, N, Start, ref Path);

                    if (End != null && PathExists(Start, End))
                    {
                        foreach (EdgeNode E in Path)
                        {
                            if (!EdgeToListOfUnfolding.ContainsKey(E))
                                EdgeToListOfUnfolding.Add(E, Path);
                        }
                    }
                }

                foreach (EdgeNode E in VisitedEdges)
                {
                    E.visited = false;
                }
            }
        }
    }
    
    private EdgeNode CheckCollinear(EdgeNode Start, EdgeNode Current, EdgeNode From, ref List<EdgeNode> Path)
    {
        if (Current.State == "CUT" && !Current.GetNeighbors().Contains(Start))
            return Current;
        else if(Current.PerpendicularNeighbors.Contains(Start))
        {
            Path.Add(Current);
            foreach (EdgeNode P in Current.PerpendicularNeighbors)
            {
                if (P.State == "CUT" && Start.ParallelEdges.Contains(P))
                {
                    return P;
                }
            }

            foreach (EdgeNode N in Current.CollinearNeighbors)
            {
                if (!N.PerpendicularNeighbors.Contains(From))
                {
                    return CheckCollinear(Start, N, Current, ref Path);
                }
            }
        }
        else
        {
            Path.Add(Current);
            foreach (EdgeNode P in Current.PerpendicularNeighbors)
            {
                if(P.State == "CUT" && Start.ParallelEdges.Contains(P))
                {
                    return P;
                }
            }
            foreach(EdgeNode N in Current.CollinearNeighbors)
            {
                if(N != From)
                {
                        return CheckCollinear(Start, N, Current, ref Path);
                }
            }
        }

        return null;
    }

    private bool PathExists(EdgeNode E1, EdgeNode E2)
    {
        E1.visited = true;
        VisitedEdges.Enqueue(E1);
        if(E1 == E2)
        {
            return true;
        }
        else
        {
            bool found = false;
            foreach(EdgeNode N in E1.GetNeighbors())
            {
                if(string.Compare(N.GetEdgeState(), "CUT") == 0 && !N.visited)
                {
                    found = PathExists(N, E2);
                }

                if (found)
                    break;
            }

            return found;
        }
    }

    public bool IsAdjacentToCut(Transform Hinge)
    {
        EdgeNode E = HingeToEdge[Hinge];

        foreach(EdgeNode N in E.GetNeighbors())
        {
            if(string.Compare(N.GetEdgeState(), "CUT") == 0)
            {
                return true;
            }
        }

        return false;
    }

    public List<Transform> GetUnfoldingHinges()
    {
        List<Transform> Lines = new List<Transform>();

        foreach (EdgeNode E in EdgeToListOfUnfolding.Keys)
        {
            Lines.Add(E.GetHinges()[0]);
        }

        return Lines;
    }

    public List<Transform> GetUnfoldingLines()
    {
        List<Transform> Lines = new List<Transform>();

        foreach(EdgeNode E in EdgeToListOfUnfolding.Keys)
        {
            Lines.Add(E.GetHinges()[0]);
            Lines.Add(E.GetHinges()[1]);
        }

        return Lines;
    }

    public List<Transform> GetUnfoldingLineFromHinge(Transform hinge)
    {
        if(EdgeToListOfUnfolding.ContainsKey(HingeToEdge[hinge]))
        {
            List<EdgeNode> EdgeLine = EdgeToListOfUnfolding[HingeToEdge[hinge]];
            List<Transform> TransLine = new List<Transform>();

            foreach (EdgeNode E in EdgeLine)
            {
                TransLine.Add(E.hinge_1);
            }

            return TransLine;
        }

        return new List<Transform>();
    }
    
    public List<Transform> GetAdjacentEdges(Transform Hinge)
    {
        EdgeNode E = HingeToEdge[Hinge];

        List<Transform> AdjacentHinges = new List<Transform>();

        foreach(EdgeNode N in E.GetNeighbors())
        {
            AdjacentHinges.Add(N.GetHinges()[0]);
            AdjacentHinges.Add(N.GetHinges()[1]);
        }

        return AdjacentHinges;
    }

    public List<Transform> GetCollinearEdges(Transform Hinge)
    {
        EdgeNode E = HingeToEdge[Hinge];
        Debug.Log(E.CollinearNeighbors.Count);

        List<Transform> CollinearHinges = new List<Transform>();
        
        foreach(EdgeNode N in E.CollinearEdges)
        {
            CollinearHinges.Add(N.GetHinges()[0]);
            CollinearHinges.Add(N.GetHinges()[1]);
        } 

        return CollinearHinges;
    }

    public List<Transform> GetPerpendicularEdges(Transform Hinge)
    {
        EdgeNode E = HingeToEdge[Hinge];

        List<Transform> PerpendicularEdges = new List<Transform>();

        foreach (EdgeNode N in E.PerpendicularEdges)
        {
            PerpendicularEdges.Add(N.GetHinges()[0]);
            PerpendicularEdges.Add(N.GetHinges()[1]);
        }

        return PerpendicularEdges;
    }

    public List<Transform> GetParallelEdges(Transform Hinge)
    {
        EdgeNode E = HingeToEdge[Hinge];

        List<Transform> ParallelEdges = new List<Transform>();

        foreach (EdgeNode N in E.ParallelEdges)
        {
            ParallelEdges.Add(N.GetHinges()[0]);
            ParallelEdges.Add(N.GetHinges()[1]);
        }

        return ParallelEdges;
    }

    public List<Transform> GetCutEdges()
    {
        List<Transform> hinges = new List<Transform>();
        foreach(EdgeNode E in CutEdges)
        {
            hinges.Add(E.GetHinges()[0]);
            hinges.Add(E.GetHinges()[1]);
        }

        return hinges;
    }

    public List<Transform> GetInvalidEdges()
    {
        List<Transform> hinges = new List<Transform>();
        foreach (EdgeNode E in CutEdges)
        {
            hinges.Add(E.GetHinges()[0]);
            hinges.Add(E.GetHinges()[1]);
        }

        return hinges;
    }

    public string GetEdgeState(Transform hinge)
    {
        return HingeToEdge[hinge].GetEdgeState();
    }

    public void SetEdgeState(string NewState, Transform hinge)
    {
        HingeToEdge[hinge].SetEdgeState(NewState);
    }

    public List<Transform> GetEdgeFromHinge(Transform H)
    {
        return HingeToEdge[H].GetHinges();
    }

    public List<Transform> GetAllEdges()
    {
        List<Transform> T = new List<Transform>();

        foreach(EdgeNode E in ListOfEdges)
        {
            T.Add(E.hinge_1);
            T.Add(E.hinge_2);
        }

        return T;
    }

    public List<Transform> GetNeighborsToCuts()
    {
        List<Transform> N = new List<Transform>();

        foreach(EdgeNode E in CutEdges)
        {
            foreach(EdgeNode Neigh in E.GetNeighbors())
            {
                if(Neigh.GetEdgeState() != "CUT")
                {
                    N.Add(Neigh.hinge_1);
                    N.Add(Neigh.hinge_2);
                }
            }
        }

        return N;
    }

    public bool IsCut(Transform H)
    {
        return HingeToEdge[H].GetEdgeState() == "CUT";
    }

    public int GetCutCount()
    {
        return CutEdges.Count;
    }

    public int GetEdgeCount()
    {
        return ListOfEdges.Count;
    }

    public void EdgeDump()
    {
        Debug.Log("-------------START EDGES----------------");
        foreach(EdgeNode E in ListOfEdges)
        {
            Debug.Log(E.GetLatticePosition());
        }
        Debug.Log("--------------END EDGES----------------");
    }

    protected class EdgeNode
    {
        public Transform hinge_1;
        public Transform hinge_2;
        private Vector3 LatticePos;

        private List<EdgeNode> AdjacentEdges;
        public List<EdgeNode> PerpendicularEdges;
        public List<EdgeNode> ParallelEdges;
        public List<EdgeNode> CollinearEdges;
        public List<EdgeNode> CollinearNeighbors;
        public List<EdgeNode> PerpendicularNeighbors;
        public List<EdgeNode> Neighbor_Side_1;
        public List<EdgeNode> Neighbor_Side_2;

        private List<Vector3> Normal;

        private List<Vector3> EndPoints;

        private string Type;

        public string State;
        public bool visited;
 
        public EdgeNode(Transform h1, Transform h2, Vector3 LP)
        {
            hinge_1 = h1;
            hinge_2 = h2;

            Vector3 norm_1 = hinge_1.parent.up;
            Vector3 norm_2 = hinge_2.parent.up;

            Normal = new List<Vector3>();

            if (norm_1 != norm_2)
                Normal.Add(norm_1);

            Normal.Add(norm_2);

            LatticePos = LP;

            if (Normal.Count > 1)
            {
                //Debug.Log("Edge " + hinge_1.parent.name + " , " + hinge_2.parent.name + " is special");
                Type = "SPECIAL";
            }
            else
                Type = "NORMAL";

            State = "UNTOUCHED";

            EndPoints = new List<Vector3>();

            EndPoints.Add(LatticePos + hinge_1.forward * 0.5f);
            EndPoints.Add(LatticePos + -hinge_1.forward * 0.5f);

            AdjacentEdges = new List<EdgeNode>();

            ParallelEdges = new List<EdgeNode>();
            PerpendicularEdges = new List<EdgeNode>();
            CollinearEdges = new List<EdgeNode>();
            CollinearNeighbors = new List<EdgeNode>();
            PerpendicularNeighbors = new List<EdgeNode>();
            Neighbor_Side_1 = new List<EdgeNode>();
            Neighbor_Side_2 = new List<EdgeNode>();

            visited = false;
        }

        public void AddNewAdjacentEdge(EdgeNode E)
        {
            AdjacentEdges.Add(E);

            if (E.GetEndPoints()[0] == EndPoints[0] || E.GetEndPoints()[1] == EndPoints[0])
                Neighbor_Side_1.Add(E);
            else
                Neighbor_Side_2.Add(E);
        }

        public List<Transform> GetHinges()
        {
            List<Transform> hinges = new List<Transform>();
            hinges.Add(hinge_1);
            hinges.Add(hinge_2);

            return hinges;
        }

        public List<EdgeNode> GetNeighbors()
        {
            return AdjacentEdges;
        }

        public List<Vector3> GetNormal()
        {
            return Normal;
        }

        public List<Vector3> GetEndPoints()
        {
            return EndPoints;
        }

        public Vector3 GetLatticePosition()
        {
            return LatticePos;
        }

        public string GetEdgeType()
        {
            return Type;
        }

        public string GetEdgeState()
        {
            return State;
        }

        public void SetEdgeState(string NewState)
        {
            State = NewState;
        }
    }
}
