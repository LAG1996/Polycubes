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
    public GameObject camcam;
    public List<Vector3> cubePositions = new List<Vector3>();
    public List<Material> Materials = new List<Material>();

    //????????????????
    //  Private variables
    //????????????????
    private Dictionary<GameObject, PolyCube> PolyCubeObjectToPolyCube = new Dictionary<GameObject, PolyCube>();
    private float scaling;
    bool _TriggerAddCube;
    
    private Control_Cam camcamScript;

    private bool pickedHinge = false;
    private Transform rotationHinge = null;

    private List<Transform> Cuts;

    private enum State
    {
        VIEW_MODE,
        HINGE_MODE,
        ADJACENT_MODE,
        PERPENDICULAR_MODE,
        PARALLEL_MODE,
        COLLINEAR_MODE
    }
    private State state = State.VIEW_MODE;
    private State oldState = State.VIEW_MODE;
    // Use this for initialization
    void Start ()
    {
        Cuts = new List<Transform>();
        camcamScript = camcam.GetComponent<Control_Cam>();
        camcamScript.allowMove = false;
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

        _HandleKeyboard();
        _HandleMouse();
    }

    void BuildPolyCube()
    {
        GameObject poly = Instantiate(polycube, transform);
        PolyCube p = new PolyCube(cube.transform.lossyScale.x, spacing);
        foreach (Vector3 pos in cubePositions)
        {
            p.AddCube(pos, Instantiate(cube, pos, Quaternion.identity, poly.transform));
        }
        cubePositions.Clear();
        PolyCubeObjectToPolyCube.Add(poly, p);
        //p.DumpMapOfCubes();
        //p.DumpMapOfFaces();
        p.BuildDualGraph();
        //p.DumpMapOfEdges();
        //p.DumpAdjacency();
    }

    
    public PolyCube GetPolyCubeFromGameObject(GameObject poly)
    {
        PolyCube n;
        PolyCubeObjectToPolyCube.TryGetValue(poly, out n);

        return n;
    }

    void _HandleKeyboard()
    {
        if (state == State.VIEW_MODE)
        {
            if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Escape))
            {
                if (Cursor.lockState == CursorLockMode.None)
                    Cursor.lockState = CursorLockMode.Locked;
                else
                    Cursor.lockState = CursorLockMode.None;
                camcamScript.allowMove = !camcamScript.allowMove;
            }
        }
        else if (state == State.HINGE_MODE)
        {
            if (Input.GetKeyDown(KeyCode.Space) && rotationHinge != null)
            {
                GetPolyCubeFromGameObject(rotationHinge.parent.parent.parent.gameObject).StartRotate();
            }
        }


        if (Input.GetKeyDown(KeyCode.A) && Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
        {
            state = State.HINGE_MODE;
            Cursor.lockState = CursorLockMode.None;
            camcamScript.allowMove = false;
            Debug.Log(state);
        }
        else if (Input.GetKeyDown(KeyCode.Q) && Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
        {
            state = State.VIEW_MODE;
            Cursor.lockState = CursorLockMode.Locked;
            camcamScript.allowMove = true;
            Debug.Log(state);
        }
        else if (Input.GetKeyDown(KeyCode.S) && Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
        {
            state = State.ADJACENT_MODE;
            Cursor.lockState = CursorLockMode.None;
            camcamScript.allowMove = false;
            Debug.Log(state);
        }
        else if (Input.GetKeyDown(KeyCode.D) && Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
        {
            state = State.COLLINEAR_MODE;
            Cursor.lockState = CursorLockMode.None;
            camcamScript.allowMove = false;
            Debug.Log(state);
        }
        else if (Input.GetKeyDown(KeyCode.F) && Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
        {
            state = State.PERPENDICULAR_MODE;
            Cursor.lockState = CursorLockMode.None;
            camcamScript.allowMove = false;
            Debug.Log(state);
        }
        else if (Input.GetKeyDown(KeyCode.G) && Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
        {
            state = State.PARALLEL_MODE;
            Cursor.lockState = CursorLockMode.None;
            camcamScript.allowMove = false;
            Debug.Log(state);
        }

        if (oldState != state)
            _OnStateChange();

        oldState = state;
    }

    void _OnStateChange()
    {
        foreach(PolyCube P in PolyCubeObjectToPolyCube.Values)
        {
            P.Repaint(Materials[0], Materials[2], Materials[4]);
        }
    }

    void _HandleMouse()
    {
        if (Input.GetMouseButtonDown(0))
        {
            RaycastHit hit;
            Ray ray = camcam.GetComponent<Camera>().ScreenPointToRay(Input.mousePosition);

            if (Physics.Raycast(ray, out hit, Mathf.Infinity))
            {
                _HandleMouseClickOnState(hit.transform);
            }
        }
    }

    void _HandleMouseClickOnState(Transform trans)
    {
        switch (state)
        {
            case State.HINGE_MODE:
                _HandleHingeMode(trans);
                break;
           
            case State.ADJACENT_MODE:
                _HandleAdjacentMode(trans);
                break;

            default: break;
        }
    }

    void _HandleHingeMode(Transform trans)
    {
        if (trans.name == "edge")
        {
            PolyCube p = GetPolyCubeFromGameObject(trans.parent.parent.parent.gameObject);
            p.CutPolyCube(trans, Materials[2], Materials[4]);
            FormPerforation(p);
            pickedHinge = true;
        }
    }

    void _HandleAdjacentMode(Transform trans)
    {
        if(trans.name == "edge")
        {
            PolyCube p = GetPolyCubeFromGameObject(trans.parent.parent.parent.gameObject);
            List<Transform> AdjacentEdges = p.GetAdjacentHinges(trans);

            p.Repaint(Materials[0], Materials[2], Materials[4]);

            foreach(Transform h in AdjacentEdges)
            {
                    p.PaintHinge(h, Materials[1]);
            }

            p.DoublePaintHinge(trans, Materials[3]);
        }
    }

    void FormPerforation(PolyCube p)
    {
        if (p.CanFormPerf())
        {
            Debug.Log("Can form perforation");
            //WalkPathCuts();
        }
        else
            Debug.Log("Cannot form perforation");
    }

    void WalkPathCuts()
    {
        if((Cuts[0].position - Cuts[Cuts.Count - 1].position).normalized == Cuts[0].right)
        {
            //Start walking right
            Debug.Log("Right");
        }
        else if((Cuts[0].position - Cuts[Cuts.Count - 1].position).normalized == -Cuts[0].right)
        {
            //Start walking left
            Debug.Log("Left");
        }
        else if ((Cuts[0].position - Cuts[Cuts.Count - 1].position).normalized == Cuts[0].up)
        {
            //Start walking up
            Debug.Log("Up");
        }
        else if ((Cuts[0].position - Cuts[Cuts.Count - 1].position).normalized == -Cuts[0].up)
        {
            //Start walking down
            Debug.Log("Down");
        }

    }
}
