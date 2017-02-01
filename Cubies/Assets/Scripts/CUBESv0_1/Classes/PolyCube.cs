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
    private Dictionary<string, Transform> MapOfFaces = new Dictionary<string, Transform>();
    private AdjacencyMap DualGraph = new AdjacencyMap();

    private Queue<GameObject> FacesToDestroy = new Queue<GameObject>();
    private static Queue<Transform> ParentEdges = new Queue<Transform>();

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

    /*
    Create a new object that acts as a super-hinge that its child faces will rotate around?
    public static void EnqueueParentEdge(Transform edge)
    {
        ParentEdges.Enqueue(edge);
    }

    public static void ReparentFace(Transform face)
    {
        
    }
    */

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
       foreach(Transform face in MapOfFaces.Values)
        {
           FindAdjacentFaces(face);
       }
    }

    public int GetCubeCount()
    {
        return cubeCount;
    }

    public string DumpAdjacentFaces()
    {
        return DualGraph.ToString();
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
            string pos = new PreciseVector(face.position).ToString();
            //Check if that face would already exist in a downscaling of the polycube
            if (!MapOfFaces.ContainsKey(pos))
            {  
                //if it does not, then we can safely add it to the MapOfFaces
                face.name = "face_" + (MapOfFaces.Count + 1);
                MapOfFaces.Add(pos, face);
            }
            else
            {
                //If it does, then we need to destroy it.
                //We cannot destroy the child face of the cube we are currently checking immediately
                //as that will break the loop and cause a crash. Instead we...
                //...remove the face from the polycube from the MapOfFaces...
                Transform faceToDestroy;
                MapOfFaces.TryGetValue(pos, out faceToDestroy);
                MapOfFaces.Remove(pos);

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
    private void FindAdjacentFaces(Transform face)
    {
        //Debug.Log(face.name + " " + face.position);

        //Some utility vectors that we'll be using
        Vector3 back = face.up * -1.0f * _CUBE_SCALE * 0.5f;
        Vector3 forward = face.up * _CUBE_SCALE * 0.5f;
        Vector3 left = face.right * -1.0f *_CUBE_SCALE;
        Vector3 right = face.right *_CUBE_SCALE;
        Vector3 down = face.forward * -1.0f * _CUBE_SCALE;
        Vector3 up = face.forward * _CUBE_SCALE;

        Transform adjacentFace;

        
        //Step 1: Check for adjacent faces within our own cube
        if (MapOfFaces.TryGetValue(new PreciseVector(face.position + back + right).ToString(), out adjacentFace))
        {
            Debug.Log(face.name + " is adjacent to " + adjacentFace.name);
        }

        if (MapOfFaces.TryGetValue(new PreciseVector(face.position + back + left).ToString(), out adjacentFace))
        {
            Debug.Log(face.name + " is adjacent to " + adjacentFace.name);
        }

        if (MapOfFaces.TryGetValue(new PreciseVector(face.position + back + down).ToString(), out adjacentFace))
        {
            Debug.Log(face.name + " is adjacent to " + adjacentFace.name);
        }

        if (MapOfFaces.TryGetValue(new PreciseVector(face.position + back + up).ToString(), out adjacentFace))
        {
            Debug.Log(face.name + " is adjacent to " + adjacentFace.name);
        }


        //Step 2: Check for adjacent faces to the left, right, up, and down
        /*
        if (MapOfFaces.TryGetValue(face.position + up, out adjacentFace))
        {
            Debug.Log(face.name + " is adjacent to " + adjacentFace.name);
        }
        else
        {
            Debug.Log("Nope on " + (face.position + up));
        }

        if (MapOfFaces.TryGetValue(face.position + down, out adjacentFace))
        {
            Debug.Log(face.name + " is adjacent to " + adjacentFace.name);
        }
        else
        {
            Debug.Log("Nope on " + (face.position + down));
        }

        if (MapOfFaces.TryGetValue(face.position + left, out adjacentFace))
        {
            Debug.Log(face.name + " is adjacent to " + adjacentFace.name);
        }
        else
        {
            Debug.Log("Nope on " + (face.position + left));
        }

        if (MapOfFaces.TryGetValue(face.position + right, out adjacentFace))
        {
            Debug.Log(face.name + " is adjacent to " + adjacentFace.name);
        }
        else
        {
            Debug.Log("Nope on " + (face.position + right));
        }
        */
    }

    //Returns normal of face on the polycube
    private Vector3 _GetFaceNormal(Transform face)
    {
        return face.up;
    }
}
