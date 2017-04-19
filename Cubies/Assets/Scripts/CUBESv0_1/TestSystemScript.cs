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
    public List<Material> Hinge_Material = new List<Material>();
    public List<Material> Face_Material = new List<Material>();

    //????????????????
    //  Private variables
    //????????????????
    private Dictionary<GameObject, PolyCube> PieceToPolyCube = new Dictionary<GameObject, PolyCube>();
    private float scaling;
    bool _TriggerAddCube;
    
    private Control_Cam camcamScript;

    private bool _seeSubGraphs = false;
    private Transform rotationHinge = null;

    private List<Transform> Cuts;

    private enum State
    {
        VIEW_MODE,
        CUT_MODE,
        ADJACENT_MODE,
        PERPENDICULAR_MODE,
        PARALLEL_MODE,
        COLLINEAR_MODE,
        UNFOLD_MODE
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
        PieceToPolyCube.Add(poly, p);
        //p.DumpMapOfCubes();
        //p.DumpMapOfFaces();
        p.BuildDualGraph();
        PolyCube.MapTransformsToPolyCube(p);
        RepaintPolyCube(p);
        //p.DumpMapOfEdges();
        //p.DumpAdjacency();
    }

    
    public PolyCube GetPolyCubeFromTrans(Transform trans)
    {
        PolyCube n = PolyCube.GetPolyCubeFromTransform(trans);

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


        if (Input.GetKeyDown(KeyCode.A) && Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
        {
            state = State.CUT_MODE;
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
        else if (Input.GetKeyDown(KeyCode.H) && Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
        {
            state = State.UNFOLD_MODE;
            Cursor.lockState = CursorLockMode.None;
            camcamScript.allowMove = false;
            Debug.Log(state);
        }

        if (oldState != state && state != State.VIEW_MODE)
            _OnStateChange();

        oldState = state;

        _HandlePolyCubeRotation();
    }

    void _OnStateChange()
    {
        foreach(PolyCube P in PieceToPolyCube.Values)
        {
            P.SeeSubGraph = false;
            P.Selected_Hinge = null;
            RepaintPolyCube(P);
        }
    }

    void _HandlePolyCubeRotation()
    {
        Queue<PolyCube> PolyCubeNotDone = new Queue<PolyCube>();
        
        foreach(PolyCube P in PolyCube.PolyCubesToHandleRotation)
        {
            if (PolyCube.HandleRotations(P))
                PolyCubeNotDone.Enqueue(P);
        }

        PolyCube.PolyCubesToHandleRotation = new List<PolyCube>();

        while(PolyCubeNotDone.Count > 0)
        {
            PolyCube.PolyCubesToHandleRotation.Add(PolyCubeNotDone.Dequeue());
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
            case State.CUT_MODE:
                _HandleCutMode(trans);
                break;
           
            case State.ADJACENT_MODE:
                _HandleAdjacentMode(trans);
                break;

            case State.COLLINEAR_MODE:
                _HandleCollinearMode(trans);
                break;

            case State.PERPENDICULAR_MODE:
                _HandlePerpendicularMode(trans);
                break;

            case State.PARALLEL_MODE:
                _HandleParallelEdges(trans);
                break;

            case State.UNFOLD_MODE:
                _HandleUnfoldMode(trans);
                break;

            default: break;
        }
    }

    void _HandleUnfoldMode(Transform trans)
    {
        PolyCube p = GetPolyCubeFromTrans(trans);
        if (trans.name == "edge")
        {
            p.ShowSubGraphs(trans, Face_Material[1], Face_Material[2]);
        }
        else if(trans.name == "body")
        {
            if(p.SeeSubGraph)
            {
                Debug.Log("Rotate?");
                p.InitializeRotate(trans);
            }
        }
    }

    void _HandleCollinearMode(Transform trans)
    {
        if(trans.name == "edge")
        {
            PolyCube p = GetPolyCubeFromTrans(trans);
            List<Transform> CollinearEdges = p.GetRelatedHinges(trans, "Collinear");

            RepaintPolyCube(p);

            foreach (Transform h in CollinearEdges)
            {
                p.PaintHinge(h, Hinge_Material[1]);
            }

            p.DoublePaintHinge(trans, Hinge_Material[3]);
        }
    }

    void _HandlePerpendicularMode(Transform trans)
    {
        if (trans.name == "edge")
        {
            PolyCube p = GetPolyCubeFromTrans(trans);
            List<Transform> PerpendicularEdges = p.GetRelatedHinges(trans, "Perpendicular");

            RepaintPolyCube(p);

            foreach (Transform h in PerpendicularEdges)
            {
                p.PaintHinge(h, Hinge_Material[1]);
            }

            p.DoublePaintHinge(trans, Hinge_Material[3]);
        }
    }

    void _HandleParallelEdges(Transform trans)
    {
        if (trans.name == "edge")
        {
            PolyCube p = GetPolyCubeFromTrans(trans);
            List<Transform> ParallelEdges = p.GetRelatedHinges(trans, "Parallel");

            RepaintPolyCube(p);

            foreach (Transform h in ParallelEdges)
            {
                p.PaintHinge(h, Hinge_Material[1]);
            }

            p.DoublePaintHinge(trans, Hinge_Material[3]);
        }
    }

    void _HandleCutMode(Transform trans)
    {
        if (trans.name == "edge")
        {
            PolyCube p = GetPolyCubeFromTrans(trans);
            p.CutPolyCube(trans);
            RepaintPolyCube(p);
        }
    }

    void _HandleAdjacentMode(Transform trans)
    {
        if(trans.name == "edge")
        {
            PolyCube p = GetPolyCubeFromTrans(trans);
            List<Transform> AdjacentEdges = p.GetAdjacentHinges(trans);

            RepaintPolyCube(p);

            foreach (Transform h in AdjacentEdges)
            {
                    p.PaintHinge(h, Hinge_Material[1]);
            }

            p.DoublePaintHinge(trans, Hinge_Material[3]);
        }
    }

    void RepaintPolyCube(PolyCube P)
    {
        P.Repaint(Hinge_Material[0], Hinge_Material[2], Hinge_Material[4], Hinge_Material[5], Face_Material[0]);
    }
}