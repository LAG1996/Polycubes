using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PreciseVector {

    public static decimal SetPrecision(float n, int precise)
    {
        return decimal.Round((decimal)n, precise);
    }

    public static string Vector3ToDecimalString(Vector3 vector, int precision)
    {
        decimal X = PreciseVector.SetPrecision(vector.x, precision);
        decimal Y = PreciseVector.SetPrecision(vector.y,  precision);
        decimal Z = PreciseVector.SetPrecision(vector.z, precision);

        return X + "," + Y + "," + Z;
    }

    public static string FloatToDecimalString(float n, int precision)
    {
        return PreciseVector.SetPrecision(n, 1) + "";
    }
}
