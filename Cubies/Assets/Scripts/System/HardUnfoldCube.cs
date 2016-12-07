using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HardUnfoldCube : MonoBehaviour {
    public GameObject cube;

    private bool _rotated = false;
    private MonoCube firstBox;

    // Use this for initialization
    void Start()
    {
        MonoCube.CubePref = cube;
        MonoCube.ParentSpace = transform;
        firstBox = new MonoCube(Vector3.zero);
    }

    // Update is called once per frame
    void Update()
    {
        //Hardcoding an unfolding sequence
        if(!_rotated)
        {
            firstBox.RotateFaceByHinge("Top", "Top");
            _rotated = true;
        }
    }
}
