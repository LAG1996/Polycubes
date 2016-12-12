using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MonoCube : MonoBehaviour
{
    private GameObject cube;
    private Vector3 position;

    private CubeFace front;
    private CubeFace back;
    private CubeFace left;
    private CubeFace right;
    private CubeFace top;
    private CubeFace bottom;

    private static Transform parentSpace;
    private static GameObject cubePrefab;
    private static Dictionary<Vector3, MonoCube> vect2Cube = new Dictionary<Vector3, MonoCube>();
    private static Vector3 NewCubeLoca = Vector3.zero;
    private const float OFFSET = 2.25f;


    public static Queue<MonoCube> CubesToRender = new Queue<MonoCube>();

    public GameObject CubeObject { get { return cube; } }
    public Vector3 CubePosition { get { return position; } }
    public static GameObject CubePref { set { cubePrefab = value;  } get { return cubePrefab; } }
    public static Transform ParentSpace { set { parentSpace = value; } }
    public Dictionary<string, bool> IsFaceRotating = new Dictionary<string, bool>();

    public MonoCube(Vector3 pos)
    {
        cube = Instantiate(MonoCube.cubePrefab, pos, Quaternion.identity, parentSpace);
        position = pos;
        MonoCube.vect2Cube.Add(position, this);
        //MonoCube.CubesToRender.Enqueue(this); cubes automatically render now so this is unnecessary

        front = new CubeFace(cube.transform.Find("FrontFace"), "FrontFace");
        IsFaceRotating.Add("FrontFace", false);

        back = new CubeFace(cube.transform.Find("BackFace"), "BackFace");
        IsFaceRotating.Add("BackFace", false);

        left = new CubeFace(cube.transform.Find("LeftFace"), "LeftFace");
        IsFaceRotating.Add("LeftFace", false);

        right = new CubeFace(cube.transform.Find("RightFace"), "RightFace");
        IsFaceRotating.Add("RightFace", false);

        top = new CubeFace(cube.transform.Find("TopFace"), "TopFace");
        IsFaceRotating.Add("TopFace", false);

        bottom = new CubeFace(cube.transform.Find("BottomFace"), "BottomFace");
        IsFaceRotating.Add("BottomFace", false);
    }

    public string StartRotateFaceByHinge(string faceName, string hinge)
    {
        if (IsFaceRotating.ContainsKey(faceName))
        {
            if(!PickFace(faceName).Rotated)
            {
                bool inRotate = false;
                IsFaceRotating.TryGetValue(faceName, out inRotate);
                if (!inRotate)
                {
                    CubeFace face = PickFace(faceName);

                    face.InitializeRotate(hinge);

                    IsFaceRotating[faceName] = true;
                }
            }
        }
        else
        {
            return "INCORRECT_KEY";
        }
        return "IN_PROGRESS";
    }

    public void ContinueRotate()
    {
        List<string> keys = new List<string>(IsFaceRotating.Keys);
        foreach (string k in keys)
        {
            bool inRotate;
            IsFaceRotating.TryGetValue(k, out inRotate);
            if (inRotate)
            {
                CubeFace face = PickFace(k);

                if(string.Compare(face.Rotate(), "SUCCESS") == 0)
                {
                    IsFaceRotating[k] = false;
                }
            }
        }
    }

    public CubeFace PickFace(string faceName)
    {
        switch (faceName)
        {
            case "FrontFace": return front;
            case "BackFace": return back;
            case "LeftFace": return left;
            case "RightFace": return right;
            case "TopFace": return top;
            case "BottomFace": return bottom;

            default: return null;
        }
    }

    public static MonoCube CreateNewBoxInDirection(string direction)
    {
        //Instantiate a cube from a cube prefab and add it to the dictionary of cubes with their position as their key
        Vector3 point = directionToPoint(direction);
        NewCubeLoca = NewCubeLoca + point;
        MonoCube box = new MonoCube(NewCubeLoca);

        return box;
    }

    public static MonoCube CreateNewBoxAtPos(Vector3 position)
    {
        NewCubeLoca = position * OFFSET;
        MonoCube box = new MonoCube(NewCubeLoca);

        return box;
    }

    private static Vector3 directionToPoint(string dir)
    {
        switch(dir)
        {
            case "FORWARD":
            case "forward":
            case "Forward": return Vector3.forward * OFFSET;

            case "BACKWARD":
            case "Backward":
            case "backward": return Vector3.back * OFFSET;

            case "LEFT":
            case "left":
            case "Left": return Vector3.left * OFFSET;

            case "RIGHT":
            case "right":
            case "Right": return Vector3.right * OFFSET;

            case "UP":
            case "up":
            case "Up": return Vector3.up * OFFSET;

            case "Down":
            case "DOWN":
            case "down": return Vector3.down * OFFSET;

            default: return Vector3.zero;
        }
    }
}