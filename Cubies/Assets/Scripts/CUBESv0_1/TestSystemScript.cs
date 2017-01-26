using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestSystemScript : MonoBehaviour {

    //???????????????
    //  Public variables
    //???????????????
    public GameObject cube;
    public float scale;
    public List<Vector3> cubePositions = new List<Vector3>();

    //????????????????
    //  Private variables
    //????????????????
    //private Dictionary<int, PolyCube> polycubes = new Dictionary<int, PolyCube>();
    private PolyCube p;
    bool _TriggerAddCube;

	// Use this for initialization
	void Start ()
    {
        p = new PolyCube(cube.transform.lossyScale.x);
        BuildPolyCube();

        p.DumpAdjacencyFaces();
    }
	
	// Update is called once per frame
	void Update ()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            _TriggerAddCube = true;
        }

        if (cubePositions.Count > 0 && _TriggerAddCube)
        {
            BuildPolyCube();
        }

        if(_TriggerAddCube)
        {
            _TriggerAddCube = false;
        }
    }

    void BuildPolyCube()
    {
        foreach (Vector3 pos in cubePositions)
        {
            p.AddCube(pos, Instantiate(cube, pos, Quaternion.identity, transform));
        }
        cubePositions.Clear();
        p.CreateDualGraph();
    }
}
