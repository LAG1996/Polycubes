using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SystemsUI : MonoBehaviour {

    
    public GameObject camcam;

    private Control_Cam camcamScript;

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

        Debug.Log(state);
    }
	
	// Update is called once per frame
	void Update ()
    {
        if (Input.GetKeyDown(KeyCode.Q) || Input.GetKeyDown(KeyCode.Escape))
        {
            if (Cursor.lockState == CursorLockMode.None)
                Cursor.lockState = CursorLockMode.Locked;
            else
                Cursor.lockState = CursorLockMode.None;
            camcamScript.allowMove = !camcamScript.allowMove;
        }
        
        if(Input.GetKey(KeyCode.A) && Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
        {
            state = State.HINGE_MODE;
            Debug.Log(state);
        }
        else if(Input.GetKey(KeyCode.S) && Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
        {
            state = State.UNFOLD_MODE;
            Debug.Log(state);
        }
        else if(Input.GetKey(KeyCode.D) && Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
        {
            state = State.CUT_MODE;
            Debug.Log(state);
        }

        if(Input.GetMouseButtonDown(0))
        {
            RaycastHit hit;
            Ray ray = camcam.GetComponent<Camera>().ScreenPointToRay(Input.mousePosition);

            if(Physics.Raycast(ray, out hit, Mathf.Infinity, 1 << LayerMask.NameToLayer("Edge")))
            {
                Debug.Log("Hit edge at " + hit.transform.position);
            }
        }
    }
}
