using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestSystemScript : MonoBehaviour {

    //???????????????
    //  Public variables
    //???????????????
    public GameObject cube;
    public GameObject polycube;
    public float spacing;
    public List<Vector3> cubePositions = new List<Vector3>();

    //????????????????
    //  Private variables
    //????????????????
    private Dictionary<GameObject, PolyCube> PolyCubeObjectToPolyCube = new Dictionary<GameObject, PolyCube>();
    private PolyCube p;
    private float scaling;
    bool _TriggerAddCube;

	// Use this for initialization
	void Start ()
    {
        p = new PolyCube(cube.transform.lossyScale.x, spacing);
        BuildPolyCube();
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
        GameObject poly = Instantiate(polycube, transform);
        foreach (Vector3 pos in cubePositions)
        {
            p.AddCube(pos, Instantiate(cube, pos, Quaternion.identity, poly.transform));
        }
        cubePositions.Clear();
        PolyCubeObjectToPolyCube.Add(poly, p);
        //p.DumpMapOfCubes();
        //p.DumpMapOfFaces();
        p.BuildDualGraph();
        //p.DumpAdjacency();
    }

    
    public PolyCube GetPolyCubeFromGameObject(GameObject poly)
    {
        PolyCube n;
        PolyCubeObjectToPolyCube.TryGetValue(poly, out n);

        return n;
    }
}
