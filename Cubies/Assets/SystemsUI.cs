using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SystemsUI : MonoBehaviour {

    
    public GameObject camcam;

    private Control_Cam camcamScript;
    // Use this for initialization
    void Start () {
        camcamScript = camcam.GetComponent<Control_Cam>();
        camcamScript.allowMove = false;
    }
	
	// Update is called once per frame
	void Update ()
    {
        if (Input.GetKeyDown(KeyCode.T) || Input.GetKeyDown(KeyCode.Escape))
        {
            if (Cursor.lockState == CursorLockMode.None)
                Cursor.lockState = CursorLockMode.Locked;
            else
                Cursor.lockState = CursorLockMode.None;
            camcamScript.allowMove = !camcamScript.allowMove;
        }
    }
}
