﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PolyCube
{
    /***********************************
    *   PUBLIC VARIABLES
    ************************************/

    /***********************************
    *   PRIVATE VARIABLES
    ************************************/
    private Dictionary<string, SingleFace> MapOfFaces = new Dictionary<string, SingleFace>();
    private Dictionary<string, SingleCube> MapOfCubes = new Dictionary<string, SingleCube>();
    private Dictionary<Transform, string> OriginalHingePos = new Dictionary<Transform, string>();
    private List<List<string>> Cut_Path;

    private AdjacencyMap DualGraph = new AdjacencyMap();

    private Queue<SingleFace> FacesToParent = new Queue<SingleFace>();
    private Transform RotateEdge;
    private Queue<Transform> FacesToRotate = new Queue<Transform>();

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
        Cut_Path = new List<List<string>>();
    }

    
    //Create a new object that acts as a super-hinge that its child faces will rotate around?
    public void SetRotationEdge(Transform edge, bool replaceHinge)
    {
        if(!replaceHinge)
           RotateEdge = edge;
        else
        {
            FacesToRotate.Clear();
            RotateEdge = edge;
        }
    }

    public void ReparentFace(Transform face)
    {
        if(!FacesToRotate.Contains(face))
            FacesToRotate.Enqueue(face.parent);
    }

    public void StartRotate()
    {
        int amt_faces = FacesToRotate.Count;

        for(int i = 0; i < amt_faces; i++)
        {
            FacesToRotate.Dequeue().RotateAround(RotateEdge.position, RotateEdge.forward, 45.0f);
        }
    }

    //Adds a new cube to the polycube structure
    public void AddCube(Vector3 position, GameObject cube)
    {
        if(position != null && cube != null)
        {

            Clean(cube);

            //Some garbage collection here. In the case that we have an empty cube object, we destroy it.
            //We also bar any attempt at adding its faces (which no longer exist) to the adjacency graph.
            if (cube.transform.childCount > 0)
            {
                cubeCount += 1;

                SingleCube c = new SingleCube(position, cube.transform);

                while(FacesToParent.Count > 0)
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

    public void BuildDualGraph()
    {
       foreach(SingleFace face in MapOfFaces.Values)
        {
           FindAdjacentFaces(face);
       }
    }

    //Input: Gets the transform of whatever hinge the user clicked on in their "cut" and the path that they are currently cutting around.
    //Description: Figures out if the next cut in the given path is a valid cut, and then adds the cut to the path if it is valid.
    //A new cut is valid if it is adjacent to the last cut in the path.
    public void CutPolycube(Transform NewCut, ref List<Transform> Path)
    {   
        //If the Path's length is greater than zero, then it already has at least one cut in it, so we need to check
        //for valid cuts (cuts are adjacent).
        if (Path.Count > 0)
        {
            Vector3 NCUTC = PreciseVector.StringToVector3(OriginalHingePos[NewCut]);
            Vector3 LCUTC = PreciseVector.StringToVector3(OriginalHingePos[Path[Path.Count - 1]]);
            if (AreAdjacentCuts(NCUTC, LCUTC))
            {
                DualGraph.DisconnectFacesByEdge(OriginalHingePos[NewCut], ref Path);

                if(Cut_Path.Count == 0 || Path.Count == 2)
                {
                    List<string> path = new List<string>();
                    path.Add(GetEdgeDirection(NCUTC - LCUTC));
                    Cut_Path.Add(path);
                }
                else
                {
                    Cut_Path[Cut_Path.Count - 1].Add(GetEdgeDirection(NCUTC - LCUTC));
                }
            }
            else
            {
                Debug.Log("Nope can't disconnect");
            }
        }
        else
            DualGraph.DisconnectFacesByEdge(OriginalHingePos[NewCut], ref Path);
        
        
        
       
    }
    //Input: Two ordered 3-tuples that represent two edges of a graph.
    //Output: Whether the edges are adjacent (they have like endpoints and are not equivalent).
    private bool AreAdjacentCuts(Vector3 NCutCenter, Vector3 LCutCenter)
    {
        
        //Need to check if the new cut is immediately adjacent to the last cut
        Vector3 Dir = NCutCenter - LCutCenter;
        string dir = GetEdgeDirection(Dir);
        Debug.Log(dir);
        if (dir != "OUT_OF_BOUNDS")
            return true;
        return false;
    }

    //In this function, we get a string that denotes the direction one edge is from another.
    //To keep generality, we use global space, making the "direction" independent of the polycube's global rotation.
    //Also, if two edges are not immediately next to each other, return "OUT_OF_BOUNDS".
    //We know to edges are not immediately next to each other if the magnitude between them is > 1.
    private string GetEdgeDirection(Vector3 Direction)
    {
        if ((int)(Direction.magnitude) > 1)
            return "OUT_OF_BOUNDS";

        if(Direction == Vector3.up)
        {
            return "UP";
        }
        if (Direction == Vector3.down)
        {
            return "DOWN";
        }
        if(Direction == Vector3.right)
        {
            return "RIGHT";
        }
        if(Direction == Vector3.left)
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
        if (Direction == Vector3.right*.5f + Vector3.up*.5f)
        {
            return "UP_RIGHT";
        }
        if(Direction == Vector3.right * .5f + Vector3.down * .5f)
        {
            return "DOWN_RIGHT";
        }
        if(Direction == Vector3.right * .5f + Vector3.forward * .5f)
        {
            return "FORWARD_RIGHT";
        }
        if(Direction == Vector3.right * .5f + Vector3.back * .5f)
        {
            return "BACK_RIGHT";
        }
        if (Direction == Vector3.left * .5f + Vector3.up * .5f)
        {
            return "UP_LEFT";
        }
        if(Direction == Vector3.left * .5f + Vector3.down * .5f)
        {
            return "DOWN_LEFT";
        }
        if(Direction == Vector3.left * .5f + Vector3.forward * .5f)
        {
            return "FORWARD_LEFT";
        }
        if(Direction == Vector3.left * .5f + Vector3.back * .5f)
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

    //Attempts to create a perforation (that is, an edge to rotate faces around)
    //If successful, returns true.
    //There are multiple checks that have to be made based on multiple cases:
    /* 
     * Check if first and last cuts are adjacent.
     *              -If yes: return FALSE. No perforation can be formed because there is no space between them.
     *              -If not: Another check must be made.
     *                  Traverse the cut path. 
     *                  -If traversal does not change directions (in other words, all we have been doing was trailing a line),
     *                  then return FALSE. No perforation can be formed because there is no material to fold around.
     *                  -Otherwise, another check must be made.
     *                      Define the possible perforation to be an infinite line containing the perforation's endpoints.
     *                      Also, define the half-spaces H1 and H2 to be the half-spaces formed by the plane PERF, where its axes are
     *                      the axes tangential to the face it is residing on.
     *                      Then traverse the cut path.
     *                          -If the cut path never crosses half-spaces, then we don't need to worry. This is a valid cut path. Return TRUE.
     *                          -If the cut path crosses half-spaces, we have to consider multiple cases.
     *                              Continue traversing the cut path. If the cut path "changes normals" (i.e., cuts through edges whose faces have different
     *                              normals than the faces of our first and last cuts) and the faces we are cutting through are "behind" the faces pertaining
     *                              to our first and last cuts, then check if the cut path ever reaches end of the polycube.
     *                              -If not, return FALSE since the faces we wish to rotate would just crash into other faces.
     *                              -If it does, return TRUE, since there would be no collision.
     *                              
     *                              -ALSO, if the cut path does not "change normals", then return FALSE since unfolding would cause faces to become incident.
     * 
     * 
     */

    public bool CanFormPerf(List<Transform> Path)
    {
        if (Path.Count <= 2)
        {
            return false;
        }
        if (IsCollinear(PreciseVector.StringToVector3(OriginalHingePos[Path[0]]), PreciseVector.StringToVector3(OriginalHingePos[Path[Path.Count - 1]])))
        {
            Debug.Log("Collinear");
            return false;
        }

        Vector3 commonNorm;
        if(DualGraph.EdgesOnSameNormal(OriginalHingePos[Path[0]], OriginalHingePos[Path[Path.Count - 1]], out commonNorm))
        {
            Debug.Log("Same normal...");
            Vector3 pos_1 = PreciseVector.StringToVector3(OriginalHingePos[Path[0]]);
            Vector3 pos_2 = PreciseVector.StringToVector3(OriginalHingePos[Path[Path.Count - 1]]);

            if(commonNorm.x == 1.0f)
            {
                if((pos_1.x - pos_2.x) != 0.0f)
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

    private bool IsCollinear(Vector3 point_1, Vector3 point_2)
    {
        string Dir = GetEdgeDirection((point_2 - point_1).normalized);

        Debug.Log(Dir);

        //Checks if two edges are collinear
        List<string> CurPath = Cut_Path[Cut_Path.Count - 1];
        int CurPathCount = CurPath.Count;
        bool DirChanged = false;
        for(int i = 0; i < CurPathCount; i++)
        {
            if (CurPath[0] != Dir)
                DirChanged = true;
        }

        return !DirChanged;
    }

    private int Diff(string endPoint_1, string endPoint_2)
    {
        int diff_count = 0;
        string []e1 = endPoint_1.Split(',');
        string[] e2 = endPoint_2.Split(',');

            for(int j = 0; j < 3; j++)
            {
                if(e1[j] != e2[j])
                {
                    diff_count++;
                }
            }

        return diff_count;
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
        foreach(string key in MapOfCubes.Keys)
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
        foreach(string key in MapOfFaces.Keys)
        {
            Debug.Log(MapOfFaces[key].Trans.name + " : " + key);
        }
        Debug.Log("--------END MAP OF FACES-------");
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
                    Object.Destroy(c.Cube);
                    cubeCount -= 1;
                }

                MapOfFaces.Remove(PositionofIncidence);
                
                //Remove all hinges that belong to this face from the map of hinges
                foreach(Transform edge in faceToDestroy.Trans)
                {
                    OriginalHingePos.Remove(edge);
                }

                Object.Destroy(faceToDestroy.Trans.gameObject); //...then we destroy that face in the polycube
                FacesToDestroy.Enqueue(face.gameObject); //then we queue the face from the cube we are checking for destruction.
            }
        }
        
        //Destroy all faces of the cube that we are checking that were incident with faces in the polycube.
        while(FacesToDestroy.Count > 0)
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

        foreach(Vector3 dir in ParentDirections)
        {
            if(dir + normal != Vector3.zero)
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
        foreach(SingleFace f in neighbor.ListOfFaces)
        {
            if(current_face.Trans.up == f.Trans.up)
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
            if (current_face.GetLatticePosition() + current_face.Trans.up*0.5f == f.GetLatticePosition() + f.Trans.up*0.5f)
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
                            DualGraph.SetNeighborHinges(hinge, hinge_2, OriginalHingePos[hinge]);
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
