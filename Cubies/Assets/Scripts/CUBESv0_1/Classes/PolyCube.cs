using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PolyCube
{
    /***********************************
    *   PUBLIC VARIABLES
    ************************************/
    public bool SeeSubGraph = false;
    public bool InRotation = false;
    public Transform Selected_Hinge = null;
    public const float MAX_HINGE_ROTATION = 90.0f;

    public static float ROTATION_SPEED = 9.0f;
    public static Dictionary<Transform, PolyCube> TransToPolyCube = new Dictionary<Transform, PolyCube>();
    public static List<PolyCube> PolyCubesToHandleRotation = new List<PolyCube>();

    /***********************************
    *   PRIVATE VARIABLES
    ************************************/
    private Dictionary<string, SingleFace> MapOfFaces = new Dictionary<string, SingleFace>();
    private Dictionary<string, SingleCube> MapOfCubes = new Dictionary<string, SingleCube>();
    private Dictionary<Transform, SingleFace> TransToFace = new Dictionary<Transform, SingleFace>();
    private Dictionary<Transform, string> OriginalHingePos = new Dictionary<Transform, string>();
    private List<Transform> CutHeads;

    private AdjacencyMap DualGraph = new AdjacencyMap();
    private HingeMap EdgeGraph = new HingeMap();

    private Queue<SingleFace> FacesToParent = new Queue<SingleFace>();
    private Transform RotateEdge;
    private Queue<Transform> FacesToRotate = new Queue<Transform>();
    private Dictionary<Transform, Material> PaintedHinges = new Dictionary<Transform, Material>();
    private List<Transform> CutHinges = new List<Transform>();
    private List<Transform> CannotCut = new List<Transform>();
    private List<Transform> UnfoldingLines = new List<Transform>();
    private Transform FirstHingeCut = null;

    private Dictionary<Transform, List<Transform>> TransToSubGraph_1 = new Dictionary<Transform, List<Transform>>();
    private Dictionary<Transform, List<Transform>> TransToSubGraph_2 = new Dictionary<Transform, List<Transform>>();

    private Queue<Transform> HingesToUnfoldAround = new Queue<Transform>();

    private float _CUBE_SCALE;
    private float _SPACING;
    private int cubeCount;
    private int faceCount;

    /************************************
    *   CONSTRUCTOR
    *************************************/
    public PolyCube(float cubeScale, float spacing)
    {
        Debug.Log(cubeScale);
        _CUBE_SCALE = cubeScale;
        _SPACING = spacing;
        cubeCount = 0;
        faceCount = 0;
        CutHeads = new List<Transform>();
    }

    public static bool HandleRotations(PolyCube P, int MODE)
    {
        if(MODE == 0)
        {
            //Unfold
            Queue<Transform> InCompleteRotations = new Queue<Transform>();
            while (P.HingesToUnfoldAround.Count > 0)
            {
                Transform H = P.HingesToUnfoldAround.Dequeue();
                if(P.EdgeGraph.GetRotation(H) < 90)
                {
                    P.EdgeGraph.IncrementRotation(H, (int)ROTATION_SPEED);
                    H.Rotate(0.0f, 0.0f, ROTATION_SPEED);

                    if (P.EdgeGraph.GetRotation(H) < MAX_HINGE_ROTATION)
                    {
                        InCompleteRotations.Enqueue(H);
                    }
                }
            }

            if (InCompleteRotations.Count == 0)
            {
                return false;
            }
            else
            {
                while (InCompleteRotations.Count > 0)
                {
                    P.HingesToUnfoldAround.Enqueue(InCompleteRotations.Dequeue());
                }
                return true;
            }
        }

        return false;
    }

    public static PolyCube GetPolyCubeFromTransform(Transform T)
    {
        return PolyCube.TransToPolyCube[T];
    }

    public static void MapTransformsToPolyCube(PolyCube p)
    {
        foreach(SingleFace F in p.MapOfFaces.Values)
        {
            Debug.Log(F.Trans.name);
            PolyCube.TransToPolyCube.Add(F.body, p);
            foreach(Transform H in F.edges)
            {
                PolyCube.TransToPolyCube.Add(H, p);
            }
        }
    }

    //Adds a new cube to the polycube structure
    public void AddCube(Vector3 position, GameObject cube)
    {
        if (position != null && cube != null)
        {

            Clean(cube);

            //Some garbage collection here. In the case that we have an empty cube object, we destroy it.
            //We also bar any attempt at adding its faces (which no longer exist) to the adjacency graph.
            if (cube.transform.childCount > 0)
            {
                cubeCount += 1;

                SingleCube c = new SingleCube(position, cube.transform);

                while (FacesToParent.Count > 0)
                {
                    SingleFace f = FacesToParent.Dequeue();
                    c.AddNewFace(f, ref OriginalHingePos);
                    f.Parent = c;
                }

                MapOfCubes.Add(PreciseVector.Vector3ToDecimalString(position, 1), c);
                cube.transform.position = cube.transform.position * _SPACING;
                cube.SetActive(true);
            }
            else
            {
                cubeCount -= 1;
                Object.Destroy(cube);
            }
        }
    }

    public void Repaint(Material DefaultMaterial, Material CutMaterial, Material InvalidCutMaterial, Material UnfoldMaterial, Material DefaultFace)
    {
        foreach (Transform C in OriginalHingePos.Keys)
        {
            C.GetComponent<Renderer>().material = DefaultMaterial;
        }

        foreach (Transform C in CutHinges)
        {
            C.GetComponent<Renderer>().material = CutMaterial;
        }

        foreach (Transform C in CannotCut)
        {
            C.GetComponent<Renderer>().material = InvalidCutMaterial;
        }

        foreach (Transform C in UnfoldingLines)
        {
            C.GetComponent<Renderer>().material = UnfoldMaterial;
        }

        foreach(SingleFace F in MapOfFaces.Values)
        {
            F.body.GetComponent<Renderer>().material = DefaultFace;
        }
    }

    public void BuildDualGraph()
    {
        foreach (SingleFace face in MapOfFaces.Values)
        {
            FindAdjacentFaces(face);
        }

        EdgeGraph.SetEdgeRelationships();
    }

    //Input: Transform of hinge, a color for labeling cut edges, a color for edges that cannot be cut because cutting them would disconnect the dual graph
    //Description: Manipulate the dual graph so that nodes in the graph are disconnected (i.e. edges are removed from the graph) according to the hinge the user
    //              selected for cutting. Also, find the edges on the polycube that, when cut, would cause the graph to be disconnected and label them as "invalid"
    public void CutPolyCube(Transform NewCut)
    {
        if (!EdgeGraph.IsCut(NewCut))
        {
                List<Transform> ValidCuts;
                bool valid = DualGraph.CutIfAllowed(NewCut, EdgeGraph, out ValidCuts);

            if (valid)
            {
                CutHinges.Add(ValidCuts[0]);
                CutHinges.Add(ValidCuts[1]);
            }
            else
            {
                Debug.Log("Cannot cut there");
            }
        }
        else
        {
            List<Transform> HingePair;
            DualGraph.ReconnectAroundEdge(NewCut, EdgeGraph, out HingePair);


            CutHinges.Remove(HingePair[0]);
            CutHinges.Remove(HingePair[1]);
        }

        List<Transform> InvalidCuts = DualGraph.FindInvalidEdges(EdgeGraph);
        CannotCut = InvalidCuts;

        EdgeGraph.FindUnfoldingLines();
        List<Transform> UnfoldingHinges = EdgeGraph.GetUnfoldingHinges();
        UnfoldingLines = EdgeGraph.GetUnfoldingLines();

        TransToSubGraph_1 = new Dictionary<Transform, List<Transform>>();
        TransToSubGraph_2 = new Dictionary<Transform, List<Transform>>();

        //If unfolding hinges have been found, clear the subgraph dictionary because we are going to make new entries
        if (UnfoldingHinges.Count > 0)
        {
            
            foreach (Transform H in UnfoldingHinges)
            {
                GetSubGraphs(H);
            }
        }
        
    }

    private void GetSubGraphs(Transform hinge)
    {
        List<Transform> SG_1 = new List<Transform>();
        List<Transform> SG_2 = new List<Transform>();

        DualGraph.GetSubGraphsAroundUnfoldLine(EdgeGraph, hinge, ref SG_1, ref SG_2); //Get the subgraphs generated when the unfold line that contains the hinge is "cut"

        //If subgraphs have been found, add the hinges to the proper dictionary, and have each body transform map to the subgraph that it is a part of
        if(SG_1.Count > 0)
        {
            List<Transform> Pair = EdgeGraph.GetHingePair(hinge);

            Transform body_1 = DualGraph.GetBodyFromHinge(Pair[0]);
            Transform body_2 = DualGraph.GetBodyFromHinge(Pair[1]);

            TransToSubGraph_1.Add(Pair[0], SG_1);
            TransToSubGraph_2.Add(Pair[1], SG_2);

            if(!TransToSubGraph_1.ContainsKey(body_1))
                TransToSubGraph_1.Add(body_1, SG_1);
            
            if(!TransToSubGraph_2.ContainsKey(body_2))
            TransToSubGraph_2.Add(body_2, SG_2);
        }
    }

    public void ShowSubGraphs(Transform hinge, Material SG_1, Material SG_2)
    {
        if(TransToSubGraph_1.Count > 0)
        {
            List<Transform> Pair = EdgeGraph.GetHingePair(hinge);
            SeeSubGraph = true;

            if (TransToSubGraph_1.ContainsKey(Pair[0]) || TransToSubGraph_1.ContainsKey(Pair[1]))
            {
                foreach (Transform T in TransToSubGraph_1[Pair[0]])
                {
                    T.GetComponent<Renderer>().material = SG_1;
                }

                foreach (Transform T in TransToSubGraph_2[Pair[1]])
                {
                    T.GetComponent<Renderer>().material = SG_2;
                }

                Selected_Hinge = hinge;
            }
            else if (TransToSubGraph_2.ContainsKey(Pair[0]) || TransToSubGraph_2.ContainsKey(Pair[1]))
            {
                foreach (Transform T in TransToSubGraph_1[Pair[1]])
                {
                    T.GetComponent<Renderer>().material = SG_1;
                }

                foreach (Transform T in TransToSubGraph_2[Pair[0]])
                {
                    T.GetComponent<Renderer>().material = SG_2;
                }

                Selected_Hinge = hinge;
            }
            else
            {
                SeeSubGraph = false;
            }
                
        }
    }

    public void InitializeRotate(Transform body)
    {
        if(Selected_Hinge != null)
        {
            Queue<Transform> All_The_Things = new Queue<Transform>();

            List<Transform> Pair = EdgeGraph.GetHingePair(Selected_Hinge);

            List<Transform> SG_1 = new List<Transform>();
            List<Transform> SG_2 = new List<Transform>();

            Transform SG_1_Parent;
            Transform SG_2_Parent;

            Transform Parent_H;

            if(TransToSubGraph_1.ContainsKey(Pair[0]))
            {
                SG_1 = TransToSubGraph_1[Pair[0]];
                SG_2 = TransToSubGraph_2[Pair[1]];

                SG_1_Parent = Pair[0];
                SG_2_Parent = Pair[1];

            }
            else
            {
                SG_1 = TransToSubGraph_1[Pair[1]];
                SG_2 = TransToSubGraph_2[Pair[0]];

                SG_1_Parent = Pair[1];
                SG_2_Parent = Pair[0];
            }


            if (SG_1.Contains(body))
            {
                Parent_H = SG_1_Parent;
                foreach (Transform B in SG_1)
                {
                    All_The_Things.Enqueue(B);

                    List<Transform> hinges = DualGraph.GetHingesOfNode(B);

                    foreach (Transform H in hinges)
                    {
                        if (H != Selected_Hinge)
                        {
                            All_The_Things.Enqueue(H);
                        }
                    }
                }
            }
            else
            {
                Parent_H = SG_2_Parent;
                foreach (Transform B in SG_2)
                {
                    All_The_Things.Enqueue(B);

                    List<Transform> hinges = DualGraph.GetHingesOfNode(B);

                    foreach (Transform H in hinges)
                    {
                        if (H != Selected_Hinge)
                        {
                            All_The_Things.Enqueue(H);
                        }
                    }
                }
            }

            while(All_The_Things.Count > 0)
            {
                All_The_Things.Dequeue().parent = Parent_H;
            }

            HingesToUnfoldAround.Enqueue(Parent_H);

            if(!PolyCubesToHandleRotation.Contains(this))
            {
                PolyCubesToHandleRotation.Add(this);
            }
        }
        else
        {
            Debug.Log("No unfolding line selected");
        }
    }

    public int GetCubeCount()
    {
        return cubeCount;
    }

    public void DumpAdjacency()
    {
        Debug.Log("-------DUAL GRAPH-------");
        DualGraph.DataDump();
        Debug.Log("--------END DUAL GRAPH------");
    }

    public void DumpMapOfCubes()
    {
        Debug.Log("-----MAP OF CUBES----");
        foreach (string key in MapOfCubes.Keys)
        {
            Debug.Log("-----CUBE POSITION----");
            Debug.Log(key);
            MapOfCubes[key].DumpFaces();
        }
        Debug.Log("-------END MAP OF CUBES---------");
    }

    public void DumpMapOfFaces()
    {
        Debug.Log("---------MAP OF FACES-------");
        foreach (string key in MapOfFaces.Keys)
        {
            Debug.Log(MapOfFaces[key].Trans.name + " : " + key);
        }
        Debug.Log("--------END MAP OF FACES-------");
    }

    public void DumpMapOfEdges()
    {
        EdgeGraph.EdgeDump();
    }

    public List<Transform> GetAdjacentHinges(Transform trans)
    {
        return EdgeGraph.GetAdjacentEdges(trans);
    }

    //Retrieve Collinear, Perpendicular, or Parallel edges    
    public List<Transform> GetRelatedHinges(Transform trans, string RelationshipType)
    {
        List<Transform> ListHinges;

        if (string.Compare(RelationshipType, "Collinear") == 0)
        {
            ListHinges = EdgeGraph.GetCollinearEdges(trans);
        }
        else if (string.Compare(RelationshipType, "Parallel") == 0)
        {
            ListHinges = EdgeGraph.GetParallelEdges(trans);
        }
        else if (string.Compare(RelationshipType, "Perpendicular") == 0)
        {
            ListHinges = EdgeGraph.GetPerpendicularEdges(trans);
        }
        else
            ListHinges = new List<Transform>();

        return ListHinges;
    }

    public void DoublePaintHinge(Transform hinge, Material NewColor)
    {
        List<Transform> Hinges = EdgeGraph.GetEdgeFromHinge(hinge);

        PaintHinge(Hinges[0], NewColor);
        PaintHinge(Hinges[1], NewColor);
    }

    public void PaintHinge(Transform hinge, Material NewColor)
    {
        hinge.GetComponent<Renderer>().material = NewColor;
    }

    //Removes incident faces
    private void Clean(GameObject cube)
    {

        Queue<GameObject> FacesToDestroy = new Queue<GameObject>(); //A convenient queue for faces that are incident to faces already in the polycube
        //For each face in the cube...
        foreach (Transform face in cube.transform)
        {
            //string pos = PreciseVector.Vector3ToDecimalString(face.position, 1);
            //Check if that face would already exist in a downscaling of the polycube
            if (!MapOfFaces.ContainsKey(PreciseVector.Vector3ToDecimalString(face.position, 1)))
            {
                //if it does not, then we can safely add it to the MapOfFaces
                face.name = "face_" + (++faceCount);
                SingleFace f = new SingleFace(face.position - cube.transform.position, face);
                FacesToParent.Enqueue(f);
                MapOfFaces.Add(PreciseVector.Vector3ToDecimalString(f.GetLatticePosition(), 1), f);
            }
            else
            {
                //If it does, then we need to destroy it.
                //We cannot destroy the child face of the cube we are currently checking immediately
                //as that will break the loop and cause a crash. Instead we...
                //...remove the face from the polycube from the MapOfFaces...
                string PositionofIncidence;
                SingleFace faceToDestroy;
                MapOfFaces.TryGetValue(PreciseVector.Vector3ToDecimalString(face.position, 1), out faceToDestroy);

                PositionofIncidence = PreciseVector.Vector3ToDecimalString(face.position, 1);

                SingleCube c;
                c = MapOfFaces[PositionofIncidence].Parent;
                c.RemoveFace(MapOfFaces[PositionofIncidence]);

                //Remove the cube c if it has no faces left.
                if (c.ListOfFaces.Count == 0)
                {
                    MapOfCubes.Remove(PreciseVector.Vector3ToDecimalString(c.Cube.position, 1));
                    Object.Destroy(c.Cube.gameObject);
                    cubeCount -= 1;
                }

                MapOfFaces.Remove(PositionofIncidence);

                //Remove all hinges that belong to this face from the map of hinges
                foreach (Transform edge in faceToDestroy.Trans)
                {
                    OriginalHingePos.Remove(edge);
                }

                Object.Destroy(faceToDestroy.Trans.gameObject); //...then we destroy that face in the polycube
                FacesToDestroy.Enqueue(face.gameObject); //then we queue the face from the cube we are checking for destruction.
            }
        }

        //Destroy all faces of the cube that we are checking that were incident with faces in the polycube.
        while (FacesToDestroy.Count > 0)
        {
            Object.Destroy(FacesToDestroy.Dequeue());
        }
    }

    //Adds faces to adjacency list by searching for faces that have been added to the polycube.
    //We search for the faces by taking advantage of the fact that the edge object's normal is the same as the face's normal.
    //We only need to search up or down along the normal by 0.5 * scale units from the edge's center to find another face.

    //TODO: Fix this up and add robustness to it.
    private void FindAdjacentFaces(SingleFace face)
    {
        //Debug.Log("Check for " + face.Trans.name + "'s adjacency");

        Vector3 normal = face.Trans.up;
        SingleCube Parent = face.Parent;
        List<Vector3> ParentDirections = new List<Vector3>();
        //Debug.Log("Parent cube's position: " + Parent.Position);

        ParentDirections.Add(Parent.Cube.up);
        ParentDirections.Add(Parent.Cube.right);
        ParentDirections.Add(-Parent.Cube.up);
        ParentDirections.Add(-Parent.Cube.right);
        ParentDirections.Add(Parent.Cube.forward);
        ParentDirections.Add(-Parent.Cube.forward);


        /*Check for adjacent faces within the cube*/
        foreach (SingleFace f in Parent.ListOfFaces)
        {
            if (normal + f.Trans.up != Vector3.zero && face != f)
            {
                DualGraph.AddNeighbors(face.Trans, f.Trans);
                //Debug.Log("Adjacent faces: " + f.Trans.name + " and " + face.Trans.name);

                FindAdjacentEdges(face.Trans, f.Trans);
            }
        }

        foreach (Vector3 dir in ParentDirections)
        {
            if (dir + normal != Vector3.zero)
            {
                //Debug.Log("Checking neighbor of parent cube at " + (Parent.Position + dir));
                if (MapOfCubes.ContainsKey(PreciseVector.Vector3ToDecimalString(Parent.Position + dir, 1)))
                {
                    CheckNeighborCubes(face, MapOfCubes[PreciseVector.Vector3ToDecimalString(Parent.Position + dir, 1)]);
                }
            }
        }
    }

    /*
    Input: The face that we are currently checking for adjacency
    Description: Check the neighboring cube's faces to see if they have the same normal as current_face. If so, 
    then it is an adjacent face. Afterwards, check a neighbor of the neighbor cube for adjacent faces (see CheckDiagonal).
    */
    private void CheckNeighborCubes(SingleFace current_face, SingleCube neighbor)
    {
        //Debug.Log("Checking neighbor...");
        foreach (SingleFace f in neighbor.ListOfFaces)
        {
            if (current_face.Trans.up == f.Trans.up)
            {
                //Debug.Log("Adjacent faces: " + f.Trans.name + " and " + current_face.Trans.name);
                DualGraph.AddNeighbors(current_face.Trans, f.Trans);

                FindAdjacentEdges(current_face.Trans, f.Trans);
            }
        }
        //Debug.Log("Check for neighbor's neighbor at " + (neighbor.Position + current_face.Trans.up));
        if (MapOfCubes.ContainsKey(PreciseVector.Vector3ToDecimalString(neighbor.Position + current_face.Trans.up, 1)))
            CheckDiagonal(current_face, MapOfCubes[PreciseVector.Vector3ToDecimalString(neighbor.Position + current_face.Trans.up, 1)]);
    }

    /*
    Input: The face that we are currently checking for adjacency
    Description: Check the neighbor to the face's parent cube's neighbor, which is forward on the parent cube's plane, for adjacent faces.
    Two faces are adjacent in this case if the normals of each face intersect.
    */
    private void CheckDiagonal(SingleFace current_face, SingleCube n_neighbor)
    {
        //Debug.Log("Checking neighbor of neighbor");
        foreach (SingleFace f in n_neighbor.ListOfFaces)
        {
            if (current_face.GetLatticePosition() + current_face.Trans.up * 0.5f == f.GetLatticePosition() + f.Trans.up * 0.5f)
            {
                //Debug.Log("Adjacent faces: " + f.Trans.name + " and " + current_face.Trans.name);
                DualGraph.AddNeighbors(current_face.Trans, f.Trans);

                FindAdjacentEdges(current_face.Trans, f.Trans);
            }
        }
    }

    private void FindAdjacentEdges(Transform f_1, Transform f_2)
    {
        foreach (Transform hinge in f_1)
        {
            if (hinge.name == "edge")
            {
                foreach (Transform hinge_2 in f_2)
                {
                    if (hinge_2.name == "edge")
                    {
                        if (OriginalHingePos[hinge] == OriginalHingePos[hinge_2])
                            EdgeGraph.SetEdge(hinge, hinge_2, PreciseVector.StringToVector3(OriginalHingePos[hinge]));
                    }
                }
            }
        }
    }

    //Returns normal of face on the polycube
    private Vector3 _GetFaceNormal(Transform face)
    {
        return face.up;
    }
}