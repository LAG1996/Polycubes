using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Grouping_sides : MonoBehaviour {
    //++++++++++++++++++++++++++
    // Public variables go here
    //++++++++++++++++++++++++++
    public GameObject cube;
    public float faceRotationSpeed;

    //??????????????????????????
    // Private variables go here
    //??????????????????????????
    MonoCube firstBox;
    MonoCube secondBox;
    MonoCube thirdBox;

    //????????????????????????????????????
    //  Mappings of all faces of each cube
    //????????????????????????????????????
    Dictionary<Vector3, CubeFace> FaceMappings = new Dictionary<Vector3, CubeFace>();

	// Use this for initialization
	void Start ()
    {
        MonoCube.CubePref = cube;
        CubeFace.rot_speed = faceRotationSpeed;
        MonoCube.ParentSpace = transform;
        firstBox = MonoCube.CreateNewBoxAtPos(Vector3.zero);
        secondBox = MonoCube.CreateNewBoxInDirection("forward");
        thirdBox = MonoCube.CreateNewBoxInDirection("left");

        firstBox.ReparentFaceFromAnotherCube(secondBox, "TopFace", "TopFace");
        firstBox.ReparentFaceFromAnotherCube(thirdBox, "TopFace", "TopFace");
    }
	
	// Update is called once per frame
	void Update ()
    {
        if (firstBox.StartRotateFaceByHinge("TopFace", "Right") == "INCORRECT_KEY")
        {
            Debug.Log("Please check spelling");
        }
        else
        {
            firstBox.ContinueRotate();
        }
    }
}
