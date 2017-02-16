using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HingeMap {

    private Dictionary<Transform, List<Transform>> HingeToPair;
    private Dictionary<Transform, SingleFace> HingeToParentFace;

    public HingeMap()
    {
        HingeToPair = new Dictionary<Transform, List<Transform>>();
        HingeToParentFace = new Dictionary<Transform, SingleFace>();
    }

    public void SetAdjacentEdges(Transform hinge_1, Transform hinge_2, SingleFace face_1, SingleFace face_2)
    {
        List<Transform> L = new List<Transform>();
        L.Add(hinge_1);
        L.Add(hinge_2);

        HingeToPair.Add(hinge_1, L);
        HingeToPair.Add(hinge_2, L);

        HingeToParentFace.Add(hinge_1, face_1);
        HingeToParentFace.Add(hinge_2, face_2);
    }

    public List<Transform> GetHingePair(Transform hinge)
    {
        return HingeToPair[hinge];
    }

    public SingleFace GetParentFace(Transform hinge)
    {
        return HingeToParentFace[hinge];
    }
}
