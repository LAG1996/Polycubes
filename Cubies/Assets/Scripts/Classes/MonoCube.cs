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


    public static Queue<MonoCube> CubesToRender = new Queue<MonoCube>();

    public GameObject CubeObject { get { return cube; } }
    public Vector3 CubePosition { get { return position; } }
    public static GameObject CubePref { set { cubePrefab = value;  } get { return cubePrefab; } }
    public static Transform ParentSpace { set { parentSpace = value; } }

    public MonoCube(Vector3 pos)
    {
        cube = Instantiate(MonoCube.cubePrefab, pos, Quaternion.identity, parentSpace);
        position = pos;
        MonoCube.vect2Cube.Add(position, this);
        //MonoCube.CubesToRender.Enqueue(this);

        front = new CubeFace(cube.transform.Find("ForeFace"));
        back = new CubeFace(cube.transform.Find("BackFace"));
        left = new CubeFace(cube.transform.Find("LeftFace"));
        right = new CubeFace(cube.transform.Find("RightFace"));
        top = new CubeFace(cube.transform.Find("TopFace"));
        bottom = new CubeFace(cube.transform.Find("BottomFace"));
    }

    public void RotateFaceByHinge(string faceName, string hinge)
    {
        CubeFace face = PickFace(faceName);

        face.Rotate(hinge);
    }

    public void ParentFacesToHinge(string [] faces, string baseFace, string hinge)
    {}

    public CubeFace PickFace(string faceName)
    {
        switch(faceName)
        {
            case "Front":
            case "FRONT":
            case "front": return front;

            case "Back":
            case "BACK":
            case "back": return back;

            case "Left":
            case "LEFT":
            case "left": return left;

            case "Right":
            case "RIGHT":
            case "right": return right;

            case "Top":
            case "TOP":
            case "top": return top;

            case "Bottom":
            case "BOTTOM":
            case "bottom": return bottom;

            default: return null;
        }
    }

    public static void CreateNewBoxInDirection(string direction)
    {
        //Instantiate a cube from a cube prefab and add it to the dictionary of cubes with their position as their key
        Vector3 point = directionToPoint(direction);
        NewCubeLoca = NewCubeLoca + point;
        MonoCube box = new MonoCube(NewCubeLoca);
    }

    public static void CreateNewBoxAtPos(Vector3 position)
    {
        NewCubeLoca = position;
        MonoCube box = new MonoCube(NewCubeLoca);
    }

    private static Vector3 directionToPoint(string dir)
    {
        const float OFFSET = 2.25f;
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