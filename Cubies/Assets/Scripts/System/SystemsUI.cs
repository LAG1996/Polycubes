using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SystemsUI : MonoBehaviour {
     
    public GameObject camcam;

    private Control_Cam camcamScript;
    private TestSystemScript systemScript;

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
        if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Escape))
        {
            if (Cursor.lockState == CursorLockMode.None)
                Cursor.lockState = CursorLockMode.Locked;
            else
                Cursor.lockState = CursorLockMode.None;
            camcamScript.allowMove = !camcamScript.allowMove;
        }

        if (Input.GetKeyDown(KeyCode.A) && Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
        {
            state = State.HINGE_MODE;
            Debug.Log(state);
        }
        else if (Input.GetKeyDown(KeyCode.S) && Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
        {
            state = State.UNFOLD_MODE;
            Debug.Log(state);
        }
        else if (Input.GetKeyDown(KeyCode.D) && Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
        {
            state = State.CUT_MODE;
            Debug.Log(state);
        }
        else if(Input.GetKeyDown(KeyCode.Q) && Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
        {
            state = State.VIEW_MODE;
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
        if(trans.name == "edge")
        {
            PolyCube.EnqueueParentEdge(trans);
        }
        else if(trans.name == "body")
        {
            PolyCube.ReparentFace(trans);
        }
        else
        {
            Debug.Log("Something's wrong: " + name);
        }
    }
}
