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
    //private Dictionary<Transform, SingleFace> HingeToParentFace;

    private List<EdgeNode> CutEdges;
    private List<EdgeNode> ListOfEdges;

    public HingeMap()
    {
        HingeToEdge = new Dictionary<Transform, EdgeNode>();
        HingeToCut = new Dictionary<Transform, EdgeNode>();
        //HingeToParentFace = new Dictionary<Transform, SingleFace>();
        CutEdges = new List<EdgeNode>();
        ListOfEdges = new List<EdgeNode>();
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
                    else if (CheckEdgeRelation(E, F, "Perpendicular"))
                        E.PerpendicularEdges.Add(F);
                    else if (CheckEdgeRelation(E, F, "Parallel"))
                        E.ParallelEdges.Add(F);
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
            return EndPointsAreCollinear(A, B, C, D);
        }
        else if(string.Compare(RelationType, "Perpendicular") == 0)
        {
            return EdgesArePerpendicular(A, B, C, D);
        }
        else if(string.Compare(RelationType, "Parallel") == 0)
        {
            return EdgesAreParallel(A, B, C, D);
        }

        return false;
    }

    public bool EdgesArePerpendicular(Vector3 A, Vector3 B, Vector3 C, Vector3 D)
    {
        return (EndPointsAreCollinear(A, C) && EndPointsAreCollinear(A, D)) || (EndPointsAreCollinear(B, C) && EndPointsAreCollinear(B, D));
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

    public bool CutIsHead(Transform NewCut)
    {
        EdgeNode NC = HingeToCut[NewCut];

        List<EdgeNode> NCN = NC.GetNeighbors();

        foreach(EdgeNode N in NCN)
        {
            if(N.GetEdgeState() == "CUT")
            {
                return false;
            }
        }

        return true;
    }

    public bool CutIsLeaf(Transform NewCut, Transform LastCut)
    {
        EdgeNode NC = HingeToCut[NewCut];
        EdgeNode LC = HingeToCut[LastCut];

        List<EdgeNode> NCN = NC.GetNeighbors();

        foreach(EdgeNode N in NCN)
        {
            if(!LC.GetNeighbors().Contains(N))
            {
                if(N.GetEdgeState() == "CUT")
                {
                    return false;
                }
            }
        }

        return true;
    }

    public void AddNewCut(Transform hinge)
    {
        if(!HingeToCut.ContainsKey(hinge))
        {
            CutEdges.Add(HingeToEdge[hinge]);
            HingeToCut.Add(hinge, HingeToEdge[hinge]);
        }
    }

    public void RemoveCut(Transform hinge)
    {
        CutEdges.Remove(HingeToEdge[hinge]);
        HingeToCut.Remove(hinge);
    }

    public string GetEdgeDirection(Transform hinge_1, Transform hinge_2)
    {
        Vector3 Dir = HingeToEdge[hinge_1].GetLatticePosition() - HingeToEdge[hinge_2].GetLatticePosition();
        string dir = GetDirection(Dir);

        return dir;
    }

    public string GetEdgeDirection(Vector3 Dir)
    {
        string dir = GetDirection(Dir);

        return dir;
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

    //In this function, we get a string that denotes the direction one edge is from another.
    //To keep generality, we use global space, making the "direction" independent of the polycube's global rotation.
    //Also, if two edges are not immediately next to each other, return "OUT_OF_BOUNDS".
    //We know to edges are not immediately next to each other if the magnitude between them is > 1.
    private string GetDirection(Vector3 Direction)
    {
        if ((int)(Direction.magnitude) > 1)
            return "OUT_OF_BOUNDS";

        if (Direction == Vector3.up)
        {
            return "UP";
        }
        if (Direction == Vector3.down)
        {
            return "DOWN";
        }
        if (Direction == Vector3.right)
        {
            return "RIGHT";
        }
        if (Direction == Vector3.left)
        {
            return "LEFT";
        }
        if (Direction == Vector3.forward)
        {
            return "FORWARD";
        }
        if (Direction == Vector3.back)
        {
            return "BACK";
        }

        //Check for combinations of vertical-horizontal movement and horizontal-vertical movement
        if (Direction == Vector3.right * .5f + Vector3.up * .5f)
        {
            return "UP_RIGHT";
        }
        if (Direction == Vector3.right * .5f + Vector3.down * .5f)
        {
            return "DOWN_RIGHT";
        }
        if (Direction == Vector3.right * .5f + Vector3.forward * .5f)
        {
            return "FORWARD_RIGHT";
        }
        if (Direction == Vector3.right * .5f + Vector3.back * .5f)
        {
            return "BACK_RIGHT";
        }
        if (Direction == Vector3.left * .5f + Vector3.up * .5f)
        {
            return "UP_LEFT";
        }
        if (Direction == Vector3.left * .5f + Vector3.down * .5f)
        {
            return "DOWN_LEFT";
        }
        if (Direction == Vector3.left * .5f + Vector3.forward * .5f)
        {
            return "FORWARD_LEFT";
        }
        if (Direction == Vector3.left * .5f + Vector3.back * .5f)
        {
            return "BACK_LEFT";
        }

        //Check for combinations of forward and vertical movement
        if (Direction == Vector3.up * .5f + Vector3.forward * .5f)
        {
            return "UP_FORWARD";
        }
        if (Direction == Vector3.up * .5f + Vector3.back * .5f)
        {
            return "UP_BACK";
        }
        if (Direction == Vector3.down * .5f + Vector3.forward * .5f)
        {
            return "DOWN_FORWARD";
        }
        if (Direction == Vector3.down * .5f + Vector3.back * .5f)
        {
            return "DOWN_BACK";
        }


        return "INVALID";
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
        private Transform hinge_1;
        private Transform hinge_2;
        private Vector3 LatticePos;

        private List<EdgeNode> AdjacentEdges;
        public List<EdgeNode> PerpendicularEdges;
        public List<EdgeNode> ParallelEdges;
        public List<EdgeNode> CollinearEdges;

        private List<Vector3> Normal;

        private List<Vector3> EndPoints;

        private string Type;
        private string State;
 
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
        }

        public void AddNewAdjacentEdge(EdgeNode E)
        {
            AdjacentEdges.Add(E);
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
