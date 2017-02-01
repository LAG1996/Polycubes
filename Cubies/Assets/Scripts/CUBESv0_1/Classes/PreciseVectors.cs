using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PreciseVector {

    public decimal x;
    public decimal y;
    public decimal z;

    public PreciseVector(Vector3 vector)
    {
        this.x = SetPrecision(vector.x, 1);
        this.y = SetPrecision(vector.y, 1);
        this.z = SetPrecision(vector.z, 1);
    }

    public static decimal SetPrecision(float n, int precise)
    {
        return decimal.Round((decimal)n, 1);
    }

    public override string ToString()
    {
        return "(" + x + ", " + y + ", " + z + ")";
    }
}
