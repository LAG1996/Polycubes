using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GenerateCube : MonoBehaviour {

    public GameObject cube;

	// Use this for initialization
	void Start ()
    {
        MonoCube.CubePref = cube;

        MonoCube firstBox = new MonoCube(Vector3.zero);
        MonoCube.CreateNewBoxInDirection("forward");
        MonoCube.CreateNewBoxInDirection("left");
        MonoCube.CreateNewBoxInDirection("BACKWARD");
    }
	
	// Update is called once per frame
	void Update ()
    {
    }
}
