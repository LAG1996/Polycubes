using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SingleFace {

    private SingleCube ParentCube;
    private Vector3 localPosition;
    private Vector3 latticePosition;
    private Transform face;
    

    public Transform body;
    public List<Transform> edges;

    public SingleCube Parent { get { return ParentCube; } set { ParentCube = value; } }
    public Transform Trans { get { return face; } }

    public SingleFace(SingleCube parent, Vector3 localPosition, Transform face)
    {
        ParentCube = parent;
        this.localPosition = localPosition;
        latticePosition = face.position;
        this.face = face;
        body = face.Find("body");
    }

    public SingleFace(Vector3 localPosition, Transform face)
    {
        ParentCube = null;
        latticePosition = face.position;
        this.localPosition = localPosition;
        this.face = face;

        edges = new List<Transform>();
        foreach (Transform hinge in face)
        {
            if (hinge.name == "edge")
            {
                edges.Add(hinge);
            }
        }

        body = face.Find("body");
    }

    public Vector3 GetLatticePosition()
    {
        return latticePosition;
    }

    public Vector3 GetLocalPosition()
    {
        return localPosition;
    }

    public SingleCube GetParentCube()
    {
        return ParentCube;
    }

    public bool IsTransform(Transform face)
    {
        return this.face == face;
    }
}
