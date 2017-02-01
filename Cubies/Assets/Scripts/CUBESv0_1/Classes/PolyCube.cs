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
    private Dictionary<Vector3, Transform> MapOfFaces = new Dictionary<Vector3, Transform>();
    private AdjacencyMap DualGraph = new AdjacencyMap();

    private Queue<GameObject> FacesToDestroy = new Queue<GameObject>();
    private Transform RotateEdge;
    private Queue<Transform> FacesToRotate = new Queue<Transform>();

    private float _CUBE_SCALE;
    private float _SPACING;
    private int cubeCount;

    /************************************
    *   CONSTRUCTOR
    *************************************/
    public PolyCube(float cubeScale, float spacing)
    {
        Debug.Log(cubeScale);
        _CUBE_SCALE = cubeScale;
        _SPACING = spacing;
        cubeCount = 0;
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
        FacesToRotate.Enqueue(face.parent);
    }

    public void StartRotate()
    {
        int amt_faces = FacesToRotate.Count;

        for(int i = 0; i < amt_faces; i++)
        {
            FacesToRotate.Dequeue().RotateAround(RotateEdge.position, RotateEdge.forward, 90.0f);
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
       foreach(Vector3 key in MapOfFaces.Keys)
        {
           FindAdjacentFaces(key);
       }
    }

    public int GetCubeCount()
    {
        return cubeCount;
    }

    public void DumpFaces()
    {
        foreach(Vector3 key in MapOfFaces.Keys)
        {
            Debug.Log(MapOfFaces[key].name + ": " + MapOfFaces[key]);
        }
    }

    public void DumpAdjacency()
    {
        DualGraph.DataDump();
    }

    /*
    private Vector3 ScalePosition(Transform t)
    {
        if (_CUBE_SCALE < 1.125f)
            return t.position - t.up * (1 - _CUBE_SCALE);
        else
            return t.position;
    }
    */

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
                face.name = "face_" + (MapOfFaces.Count + 1);
                MapOfFaces.Add(face.position, face);
            }
            else
            {
                //If it does, then we need to destroy it.
                //We cannot destroy the child face of the cube we are currently checking immediately
                //as that will break the loop and cause a crash. Instead we...
                //...remove the face from the polycube from the MapOfFaces...
                Transform faceToDestroy;
                MapOfFaces.TryGetValue(face.position, out faceToDestroy);
                MapOfFaces.Remove(face.position);

                Object.Destroy(faceToDestroy.gameObject); //...then we destroy that face in the polycube
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
    private void FindAdjacentFaces(Vector3 key)
    {
        Transform currentFace = MapOfFaces[key];

        //Some utility vectors that we'll be using
        Vector3 back = currentFace.up * -1.0f * _CUBE_SCALE * 0.5f;
        Vector3 forward = currentFace.up * _CUBE_SCALE * 0.5f;
        Vector3 left = currentFace.right * -1.0f *_CUBE_SCALE;
        Vector3 right = currentFace.right *_CUBE_SCALE;
        Vector3 down = currentFace.forward * -1.0f * _CUBE_SCALE;
        Vector3 up = currentFace.forward * _CUBE_SCALE;

        Queue<Transform> adjacentFaces = new Queue<Transform>();
        //Step 1: Check for adjacent faces within our own cube
        if (MapOfFaces.ContainsKey(key + back + right*0.5f))
        {
            adjacentFaces.Enqueue(MapOfFaces[key + back + right * 0.5f]);
        }
        if (MapOfFaces.ContainsKey(key + back + left * 0.5f))
        {
            adjacentFaces.Enqueue(MapOfFaces[key + back + left * 0.5f]);
        }
        if (MapOfFaces.ContainsKey(key + back + up * 0.5f))
        {
            adjacentFaces.Enqueue(MapOfFaces[key + back + up * 0.5f]);
        }
        if (MapOfFaces.ContainsKey(key + back + down * 0.5f))
        {
            adjacentFaces.Enqueue(MapOfFaces[key + back + down * 0.5f]);
        }
        

        //Step 2: Check for adjacent faces to the left, right, up, and down. We also have to make sure that the cubes of each respective face are not diagonal of each other
        //In other words, make sure that the adjacent faces also have equal normals.    
        if (MapOfFaces.ContainsKey(key + right))
        {
            adjacentFaces.Enqueue(MapOfFaces[key + right]);
        }
        if (MapOfFaces.ContainsKey(key + left))
        {
            adjacentFaces.Enqueue(MapOfFaces[key + left]);
        }
        if (MapOfFaces.ContainsKey(key + up))
        {
            adjacentFaces.Enqueue(MapOfFaces[key + up]);
        }

        if (MapOfFaces.ContainsKey(key + down))
        {
            adjacentFaces.Enqueue(MapOfFaces[key + down]);
        }

        //Step 3: Check for adjacent faces to the forward-left, forward-right, forward-up, forward-down
        if (MapOfFaces.ContainsKey(key + forward + right * 0.5f))
        {
            adjacentFaces.Enqueue(MapOfFaces[key + forward + right * 0.5f]);
        }
        if (MapOfFaces.ContainsKey(key + forward + left * 0.5f))
        {
            adjacentFaces.Enqueue(MapOfFaces[key + forward + left * 0.5f]);
        }
        if (MapOfFaces.ContainsKey(key + forward + up * 0.5f))
        {
            adjacentFaces.Enqueue(MapOfFaces[key + forward + up * 0.5f]);
        }
        if (MapOfFaces.ContainsKey(key + forward + down * 0.5f))
        {
            adjacentFaces.Enqueue(MapOfFaces[key + forward + down * 0.5f]);
        }

        while(adjacentFaces.Count > 0)
        {
            DualGraph.AddNeighbors(adjacentFaces.Dequeue(), currentFace);
        }
    }

    //Returns normal of face on the polycube
    private Vector3 _GetFaceNormal(Transform face)
    {
        return face.up;
    }
}
