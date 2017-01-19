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
    private Dictionary<Vector3, GameObject> MapOfCubes = new Dictionary<Vector3, GameObject>();
    private Dictionary<Vector3, GameObject> MapOfFaces = new Dictionary<Vector3, GameObject>();

    private Queue<GameObject> FacesToDestroy = new Queue<GameObject>();

    /************************************
    *   CONSTRUCTOR
    *************************************/
    public PolyCube()
    {}

    //Adds a new cube to the polycube structure
    public void AddCube(Vector3 position, GameObject cube)
    {
        MapOfCubes.Add(position, cube);

        Clean(cube);

        cube.SetActive(true);
    }


    //Removes concurrent faces
    public void Clean(GameObject cube)
    {
        foreach (Transform child in cube.transform)
        {  
            if (!MapOfFaces.ContainsKey(child.position))
                MapOfFaces.Add(child.position, child.gameObject);
            else
            {
                GameObject faceToDestroy;
                MapOfFaces.TryGetValue(child.position, out faceToDestroy);
                MapOfFaces.Remove(child.position);

                Object.Destroy(faceToDestroy);
                FacesToDestroy.Enqueue(child.gameObject);
            }
        }
        
        while(FacesToDestroy.Count > 0)
        {
            Object.Destroy(FacesToDestroy.Dequeue());
        }
    }
}
