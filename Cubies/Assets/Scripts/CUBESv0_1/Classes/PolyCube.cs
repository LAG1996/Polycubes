using System.Collections;
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
    private Dictionary<Vector3, SingleFace> MapOfFaces = new Dictionary<Vector3, SingleFace>();
    private Dictionary<Vector3, SingleCube> MapOfCubes = new Dictionary<Vector3, SingleCube>(); 

    private AdjacencyMap DualGraph = new AdjacencyMap();

    private Queue<GameObject> FacesToDestroy = new Queue<GameObject>();
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
                    c.AddNewFace(f);
                    f.Parent = c;
                }

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

    public int GetCubeCount()
    {
        return cubeCount;
    }

    public void DumpAdjacency()
    {
        DualGraph.DataDump();
    }

    //Removes incident faces
    private void Clean(GameObject cube)
    {
        //For each face in the cube...
        foreach (Transform face in cube.transform)
        {
            //string pos = PreciseVector.Vector3ToDecimalString(face.position, 1);
            //Check if that face would already exist in a downscaling of the polycube
            if (!MapOfFaces.ContainsKey(face.position))
            {  
                //if it does not, then we can safely add it to the MapOfFaces
                face.name = "face_" + (++faceCount);
                SingleFace f = new SingleFace(face.position - cube.transform.position, face);
                FacesToParent.Enqueue(f);
                MapOfFaces.Add(f.GetLatticePosition(), f);
            }
            else
            {
                //If it does, then we need to destroy it.
                //We cannot destroy the child face of the cube we are currently checking immediately
                //as that will break the loop and cause a crash. Instead we...
                //...remove the face from the polycube from the MapOfFaces...
                Vector3 PositionofIncidence;
                SingleFace faceToDestroy;
                MapOfFaces.TryGetValue(face.position, out faceToDestroy);

                PositionofIncidence = face.position;

                SingleCube c;
                c = MapOfFaces[PositionofIncidence].Parent;
                c.RemoveFace(MapOfFaces[PositionofIncidence]);

                if (c.ListOfFaces.Count == 0)
                {
                    MapOfCubes.Remove(c.Cube.position);
                    Object.Destroy(c.Cube);
                    cubeCount -= 1;
                }

                MapOfFaces.Remove(PositionofIncidence);

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
        Vector3 normal = face.Trans.up;
        Vector3 right = face.Trans.right;
        Vector3 forward = face.Trans.forward;

        SingleCube Parent = face.Parent;

        /*Check for adjacent faces within the cube*/
        foreach(SingleFace f in Parent.ListOfFaces)
        {
            if (normal + f.Trans.up != Vector3.zero && face != f)
            {
                DualGraph.AddNeighbors(face.Trans, f.Trans);
                Debug.Log("Adjacent faces: " + f.Trans.name + " and " + face.Trans.name);
            }
        }

        /*Check faces immediately to the left, right, below, or above*/

    }

    //Returns normal of face on the polycube
    private Vector3 _GetFaceNormal(Transform face)
    {
        return face.up;
    }
}
