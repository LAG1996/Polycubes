using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
                GetAdjacentEdges(E);
                ListOfEdges.Add(E);
            }      
            else
                ListOfEdges.Add(E);

            HingeToEdge.Add(hinge_1, E);
            HingeToEdge.Add(hinge_2, E);
        }
            
    }

    public List<Transform> GetHingePair(Transform hinge)
    {
        Debug.Log("Return Hinge Pair...");
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
    //Output: Whether the edges are adjacent (they have like endpoints and are not equivalent).
    public bool AreAdjacentEdges(Transform hinge_1, Transform hinge_2)
    {
            //Need to check if the new cut is immediately adjacent to the last cut
            string dir = GetEdgeDirection(hinge_1, hinge_2);
            if (dir != "OUT_OF_BOUNDS" && dir != "INVALID")
                return true;

            return false;
    }

    private bool AreAdjacentEdges(EdgeNode edge_1, EdgeNode edge_2)
    {
        string dir = GetEdgeDirection(edge_1.GetLatticePosition() - edge_2.GetLatticePosition());
        if (dir != "OUT_OF_BOUNDS" && dir != "INVALID")
            return true;

        return false;
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

    public bool CheckPerpendicularity(Transform hinge_1, Transform hinge_2)
    {
        EdgeNode E_1 = HingeToEdge[hinge_1];

        List<EdgeNode> E1N = E_1.GetNeighbors();

        foreach(EdgeNode N in E1N)
        {
            if(CheckCollinearity(N.GetHinges()[0], hinge_2) != "NOT_COLLINEAR")
            {
                return true;
            }
        }

        return false;
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

    public string CheckCollinearity(Transform hinge_1, Transform hinge_2)
    {
        if(EdgesOnSamePlane(hinge_1, hinge_2))
        {
            if(Diff(PreciseVector.Vector3ToDecimalString(HingeToEdge[hinge_1].GetLatticePosition(), 1), PreciseVector.Vector3ToDecimalString(HingeToEdge[hinge_2].GetLatticePosition(), 1)) == 1)
            {
                Vector3 Dir = HingeToEdge[hinge_1].GetLatticePosition() - HingeToEdge[hinge_2].GetLatticePosition();

                List<EdgeNode> N = HingeToEdge[hinge_1].GetNeighbors();
                
                foreach(EdgeNode E in N)
                {
                    if(Diff(PreciseVector.Vector3ToDecimalString(E.GetLatticePosition() + Dir.normalized, 1), PreciseVector.Vector3ToDecimalString(HingeToEdge[hinge_2].GetLatticePosition(), 1)) <= 1)
                    {
                        return "COLLINEAR";
                    }
                }

                return "PARALLEL";
            }

            return "NOT_COLLINEAR";
        }


        return "NOT_COLLINEAR";
    }

    private string CheckCollinearity(EdgeNode edge_1, EdgeNode edge_2)
    {
        if (EdgesOnSamePlane(edge_1, edge_2))
        {
            if (Diff(PreciseVector.Vector3ToDecimalString(edge_1.GetLatticePosition(), 1), PreciseVector.Vector3ToDecimalString(edge_2.GetLatticePosition(), 1)) > 0)
            {
                Vector3 Dir = edge_1.GetLatticePosition() - edge_2.GetLatticePosition();

                List<EdgeNode> N = edge_1.GetNeighbors();

                foreach (EdgeNode E in N)
                {
                    if (Diff(PreciseVector.Vector3ToDecimalString(E.GetLatticePosition() + Dir.normalized, 1), PreciseVector.Vector3ToDecimalString(edge_2.GetLatticePosition(), 1)) == 1)
                    {
                        Debug.Log("The edge belonging to faces " + edge_1.GetHinges()[0].parent.name + " , " + edge_1.GetHinges()[1].parent.name + " is collinear to the edge belonging to faces " + edge_2.GetHinges()[0].parent.name + " , " 
                            + edge_2.GetHinges()[1].parent.name + " thanks to " + E.GetHinges()[0].parent.name + " , " + E.GetHinges()[1].parent.name);
                        return "COLLINEAR";
                    }
                }
                Debug.Log("The edge belonging to faces " + edge_1.GetHinges()[0].parent.name + " , " + edge_1.GetHinges()[1].parent.name + " is parallel to the edge belonging to faces " + edge_2.GetHinges()[0].parent.name + " , " + edge_2.GetHinges()[1].parent.name);
                return "PARALLEL";
            }

            return "NOT_COLLINEAR";
        }


        return "NOT_COLLINEAR";
    }

    public void AddNewCut(Transform hinge)
    {
        if(!HingeToCut.ContainsKey(hinge))
        {
            CutEdges.Add(HingeToEdge[hinge]);
            HingeToCut.Add(hinge, HingeToEdge[hinge]);
        }
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

    private void GetAdjacentEdges(EdgeNode E)
    {
        int count = ListOfEdges.Count;

        for(int i = 0; i < count; i++)
        {
            if(AreAdjacentEdges(E, ListOfEdges[i]))
            {
                if(CheckCollinearity(E, ListOfEdges[i]) != "PARALLEL")
                {
                    E.AddNewAdjacentEdge(ListOfEdges[i]);
                    ListOfEdges[i].AddNewAdjacentEdge(E);
                }
            }
        }
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

        private List<Vector3> Normal;

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


            AdjacentEdges = new List<EdgeNode>();
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
