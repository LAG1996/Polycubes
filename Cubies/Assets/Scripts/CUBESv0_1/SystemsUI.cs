using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SystemsUI : MonoBehaviour {
     
    public GameObject camcam;

    private Control_Cam camcamScript;
    private TestSystemScript systemScript;

    private bool pickedHinge = false;
    private Transform rotationHinge = null;

    private List<Transform> Cuts;

    private enum State {
        VIEW_MODE,
        HINGE_MODE,
        CUT_MODE,
        UNFOLD_MODE
    }
    private State state = State.VIEW_MODE;
    // Use this for initialization
    void Start () {
        camcamScript = camcam.GetComponent<Control_Cam>();
        camcamScript.allowMove = false;

        systemScript = gameObject.GetComponent<TestSystemScript>();

        Cuts = new List<Transform>();

        Debug.Log(state);
    }
	
	// Update is called once per frame
	void Update ()
    {
        _HandleKeyboard();
        _HandleMouse();

    }

    void _HandleKeyboard()
    {
        if(state == State.VIEW_MODE)
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
        else if(state == State.HINGE_MODE)
        {
            if(Input.GetKeyDown(KeyCode.Space) && rotationHinge != null)
            {
                systemScript.GetPolyCubeFromGameObject(rotationHinge.parent.parent.parent.gameObject).StartRotate();
            }
        }
        

        if (Input.GetKeyDown(KeyCode.A) && Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
        {
            state = State.HINGE_MODE;
            Cursor.lockState = CursorLockMode.None;
            camcamScript.allowMove = false;
            Debug.Log(state);
        }
        else if (Input.GetKeyDown(KeyCode.S) && Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
        {
            state = State.UNFOLD_MODE;
            Cursor.lockState = CursorLockMode.None;
            camcamScript.allowMove = false;
            Debug.Log(state);
        }
        else if (Input.GetKeyDown(KeyCode.D) && Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
        {
            state = State.CUT_MODE;
            Cursor.lockState = CursorLockMode.None;
            camcamScript.allowMove = false;
            Debug.Log(state);
        }
        else if(Input.GetKeyDown(KeyCode.Q) && Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
        {
            state = State.VIEW_MODE;
            Cursor.lockState = CursorLockMode.Locked;
            camcamScript.allowMove = true;
            Debug.Log(state);
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
        switch(state)
        {
            case State.HINGE_MODE: _HandleHingeMode(trans);
                break;
            default: break;
        }
    }

    void _HandleHingeMode(Transform trans)
    {
        rotationHinge = trans;
        if(trans.name == "edge")
        {
            PolyCube p = systemScript.GetPolyCubeFromGameObject(trans.parent.parent.parent.gameObject);
            Cuts.Add(trans);

            FormPerforation(p, Cuts[0], Cuts[Cuts.Count - 1]);
            pickedHinge = true;
        }
    }

    void FormPerforation(PolyCube p, Transform first_cut, Transform last_cut)
    {
        if(p.FindPerforation(first_cut, last_cut))
        {
            Debug.Log("Can form perforation");

            WalkPathCuts();
        }
        else
            Debug.Log("Cannot form perforation");
    }

    void WalkPathCuts()
    {

    }
}
