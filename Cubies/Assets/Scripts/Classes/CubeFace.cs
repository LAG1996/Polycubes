using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CubeFace : MonoBehaviour
{
    private Transform body;
    private Vector3 normalDir;

    private Transform [] hinges = new Transform[5];

    public Dictionary<Transform, string> HingeLookUpTransToWord = new Dictionary<Transform, string>();

    public CubeFace(Transform f)
    {
        body = f.Find("Body");

        hinges[0] = f.Find("TopHinge");
        hinges[1] = f.Find("BottomHinge");
        hinges[2] = f.Find("RightHinge");
        hinges[3] = f.Find("LeftHinge");
        
        HingeLookUpTransToWord.Add(hinges[0], "Top");
        HingeLookUpTransToWord.Add(hinges[1], "Bottom");
        HingeLookUpTransToWord.Add(hinges[2], "Right");
        HingeLookUpTransToWord.Add(hinges[3], "Left");
    }

    public void Rotate(string orient)
    {
        Transform joint = PickHinge(orient);

        if (joint != null)
        {
            for (int i = 0; i < 4; i++)
            {
                string hingeName;
                HingeLookUpTransToWord.TryGetValue(hinges[i], out hingeName);
                if (string.Compare(hingeName, orient) != 0)
                {
                    Debug.Log(hingeName);
                    hinges[i].SetParent(joint);
                }
            }
        }
    }

    public Transform PickHinge(string orient)
    {
        Debug.Log("Yo");
        switch(orient)
        {
            case "Top": return hinges[0];
            case "Bottom": return hinges[1];
            case "Right": return hinges[2];
            case "Left": return hinges[3];

            default:  return hinges[4];
        }
    }
}
