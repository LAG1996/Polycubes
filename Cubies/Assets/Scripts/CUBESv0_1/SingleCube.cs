using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SingleCube {

    private Vector3 position;
    private List<SingleFace> Faces;
    private Transform cube;

    public List<SingleFace> ListOfFaces { get { return Faces; } }
    public Transform Cube { get { return cube; } }
    public Vector3 Position { get { return position; } }

	public SingleCube(Vector3 position, Transform cube)
    {
        this.position = position;
        Faces = new List<SingleFace>();
        this.cube = cube;
    }

    public void AddNewFace(Transform face, ref Dictionary<Transform, string> HingeToPos)
    {
        ListOfFaces.Add(new SingleFace(this, this.position - face.position, face));
        foreach(Transform edge in face)
        {
            if(edge.name == "edge")
            {
                HingeToPos.Add(edge, PreciseVector.Vector3ToDecimalString(edge.position, 1));
            }
        }
    }
    public void AddNewFace(SingleFace face, ref Dictionary<Transform, string> HingeToPos)
    {
        ListOfFaces.Add(face);

        foreach (Transform edge in face.Trans)
        {
            if (edge.name == "edge")
            {
                HingeToPos.Add(edge, PreciseVector.Vector3ToDecimalString(edge.position, 1));
            }
        }
    }

    public void RemoveFace(SingleFace face)
    {
        Faces.Remove(face);
    }

    public void DumpFaces()
    {
        Debug.Log("--------FACES---------");
        string faces = "";
        foreach(SingleFace f in ListOfFaces)
        {
            faces += f.Trans.name + ", ";
        }
        Debug.Log("{ " + faces + "}");
    }
}
