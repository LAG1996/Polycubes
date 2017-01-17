using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CubeFace : MonoBehaviour
{
    private Transform face;
    private Transform body; //The actual face, represented by a rectangular prism
    private Vector3 normalDir; //Why is this here?
    private string _name; //The name of the face (TopFace, BottomFace, etc.)
    private Transform _jointToRotateAround;
    private Vector3 _rotation;
    private Vector3 MAX_ROT = new Vector3(0.0f, 0.0f, 90.0f);
    private MonoCube _originalCube;

    private Transform [] hinges = new Transform[5];
    private bool isLocked; //Sets whether this face could be directly manipulated or is "locked" (i.e., is not supposed to be visible or has already been directly manipulated)

    public Dictionary<Transform, string> HingeLookUpTransToWord = new Dictionary<Transform, string>();
    public List<CubeFace> AdjacentFaces = new List<CubeFace>();
    
    public static float rot_speed;

    public bool IsLocked { get { return isLocked; } }
    public Transform Object { get { return face; } }

    public CubeFace(Transform f, string n, MonoCube origin)
    {
        _name = n;
        _originalCube = origin;
        body = f.Find("Body");

        hinges[0] = f.Find("TopHinge");
        hinges[1] = f.Find("BottomHinge");
        hinges[2] = f.Find("RightHinge");
        hinges[3] = f.Find("LeftHinge");

        face = f;
        
        HingeLookUpTransToWord.Add(hinges[0], "Top");
        HingeLookUpTransToWord.Add(hinges[1], "Bottom");
        HingeLookUpTransToWord.Add(hinges[2], "Right");
        HingeLookUpTransToWord.Add(hinges[3], "Left");

        isLocked = false;
        _jointToRotateAround = null;
    }

    public void InitializeDirectRotate(string orient)
    {
        Debug.Log("Preparing " + _name);
        Transform joint = PickHinge(orient);
        _jointToRotateAround = joint;

        Debug.Log("Joint picked: " + joint.gameObject.name + " of " + _name);

        if (joint != null)
        {
            for (int i = 0; i < 4; i++)
            {
                string hingeName;
                HingeLookUpTransToWord.TryGetValue(hinges[i], out hingeName);
                if (string.Compare(hingeName, orient) != 0)
                {
                    hinges[i].SetParent(joint);
                }
            }

            foreach(CubeFace f in AdjacentFaces)
            {
                f.face.SetParent(joint);
            }

            body.SetParent(joint);
        }
    }

    public string DirectRotate()
    {
        if (_jointToRotateAround == null)
        {
            return "FAILURE";
        }

        isLocked = true;

        _jointToRotateAround.Rotate(0.0f, 0.0f, CubeFace.rot_speed);
        if(_name == "TopFace" || _name == "BottomFace")
        {
            if (Mathf.Abs(_jointToRotateAround.localEulerAngles.z) >= 90.0f)
            {
                return "SUCCESS";
            }
        }
        else
        {
            if (_jointToRotateAround.name == "RightHinge")
            {
                if (Mathf.Abs(_jointToRotateAround.localEulerAngles.y) <= 270.0f)
                {
                    return "SUCCESS";
                }
            }
            else if (_jointToRotateAround.name == "LeftHinge")
            {
                if (Mathf.Abs(_jointToRotateAround.localEulerAngles.y) >= 90.0f)
                {
                    return "SUCCESS";
                }
            }
            else
            {
                if (Mathf.Abs(_jointToRotateAround.localEulerAngles.z) >= 90.0f)
                {
                    return "SUCCESS";
                }
            }
        }
        
        return "IN_PROGRESS";
        
    }

    public Transform PickHinge(string orient)
    {
        switch(orient)
        {
            case "Top": return hinges[0];
            case "Bottom": return hinges[1];
            case "Right": return hinges[2];
            case "Left": return hinges[3];

            default:  return hinges[4];
        }
    }

    public void HideFace()
    {
        face.gameObject.SetActive(false);
        isLocked = true;
    }
}
