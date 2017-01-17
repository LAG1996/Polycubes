using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HardUnfoldCube : MonoBehaviour {
    public GameObject cube;
    public float faceRotationSpeed;
    //private bool _rotated = false;
    private MonoCube firstBox;
    private MonoCube secondBox;

    // Use this for initialization
    void Start()
    {
        MonoCube.CubePref = cube;
        CubeFace.rot_speed = faceRotationSpeed;
        MonoCube.ParentSpace = transform;
        firstBox = MonoCube.CreateNewBoxAtPos(Vector3.zero);
        secondBox = MonoCube.CreateNewBoxInDirection("forward");
    }

    // Update is called once per frame
    void Update()
    {
        if(firstBox.StartRotateFaceByHinge("TopFace", "Right") == "INCORRECT_KEY")
        {
            Debug.Log("Please check spelling");
        }
        else
        {
            firstBox.ContinueRotate();
        }

        if(secondBox.StartRotateFaceByHinge("FrontFace", "Left") == "INCORRECT_KEY")
        {
            Debug.Log("Please check spelling");
        }
        else
        {
            secondBox.ContinueRotate();

        }
    }
}
