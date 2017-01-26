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
    private Dictionary<Vector3, GameObject> MapOfFaces = new Dictionary<Vector3, GameObject>();
    private AdjacencyMap DualGraph = new AdjacencyMap();

    private Queue<GameObject> FacesToDestroy = new Queue<GameObject>();

    private float _CUBE_SCALE;
    private int cubeCount;

    /************************************
    *   CONSTRUCTOR
    *************************************/
    public PolyCube(float cubeScale)
    {
        _CUBE_SCALE = 1.0f;
        cubeCount = 0;
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
                cube.SetActive(true);
            }
            else
            {
                cubeCount -= 1;
                Object.Destroy(cube);
            }
        }
    }

    public void CreateDualGraph()
    {
       foreach(GameObject face in MapOfFaces.Values)
       {
           DefineAdjacentFaces(face.transform);
       }
    }

    public int GetCubeCount()
    {
        return cubeCount;
    }

    public string DumpAdjacencyFaces()
    {
        return DualGraph.ToString();
    }


    //Removes incident faces
    private void Clean(GameObject cube)
    {
        //For each face in the cube...
        foreach (Transform face in cube.transform)
        {  
            //Check if that face already exists in the polycube.
            if (!MapOfFaces.ContainsKey(face.position)) //if it does not, then we can safely add it to the MapOfFaces
                MapOfFaces.Add(face.position, face.gameObject);
            else
            {
                //If it does, then we need to destroy it.
                //We cannot destroy the child face of the cube we are currently checking immediately
                //as that will break the loop and cause a crash. Instead we...
                //...remove the face from the polycube from the MapOfFaces...
                GameObject faceToDestroy;
                MapOfFaces.TryGetValue(face.position, out faceToDestroy);
                MapOfFaces.Remove(face.position);

                Object.Destroy(faceToDestroy); //...then we destroy that face in the polycube
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
    private void DefineAdjacentFaces(Transform face)
    {
        foreach(Transform edge in face)
        {
            if(edge.name != "body")
            {
                Vector3 up = edge.position + edge.up * 0.5f;
                Vector3 down = edge.position - edge.up * 0.5f;
                if (MapOfFaces.ContainsKey(up))
                {
                    GameObject adjacentFace;
                    MapOfFaces.TryGetValue(up, out adjacentFace);
                    DualGraph.AddAdjacentFaces(face, adjacentFace.transform);
                }

                if (MapOfFaces.ContainsKey(down))
                {
                    GameObject adjacentFace;
                    MapOfFaces.TryGetValue(down, out adjacentFace);
                    DualGraph.AddAdjacentFaces(face, adjacentFace.transform);
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
