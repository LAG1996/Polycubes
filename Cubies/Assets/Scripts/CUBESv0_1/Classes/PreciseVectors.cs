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

    public static Vector3 StringToVector3(string vector)
    {
        Debug.Log("String of vec: " + vector);
        string [] vectorPieces = vector.Split(',');
        Vector3 vec = Vector3.zero;

        for(int i = 0; i < 3; i++)
        {
            string[] splitWholeFromDec = vectorPieces[i].Split('.');
            int wholeNumLength = splitWholeFromDec[0].Length;
            bool is_negative = false;
            int tens_place = 0;
            for (int j = 0; j < wholeNumLength; j++)
            {
                char digit = splitWholeFromDec[0][wholeNumLength - 1 - j];
               
                if(digit == '-')
                {
                    is_negative = true;
                    continue;
                }

                Debug.Log((int)digit - 48 + " : " + digit);
                if (i == 0)
                {
                    vec.x += (float)(((int)digit - 48) * Mathf.Pow(10, tens_place));
                }
                else if (i == 1)
                {
                    vec.y += (float)(((int)digit - 48) * Mathf.Pow(10, tens_place));
                }
                else if (i == 2)
                {
                    vec.z += (float)(((int)digit - 48) * Mathf.Pow(10, tens_place));
                }
                else
                    Debug.Log("Something's really, really wrong.");

                tens_place++;
            }

            char tenth = splitWholeFromDec[1][0];
            Debug.Log("." + tenth);
            if (i == 0)
            {
                vec.x += (float)(((int)tenth - 48) / 10.0f);
                if(is_negative)
                {
                    vec.x *= -1;
                }
            }
            else if (i == 1)
            {
                vec.y += (float)(((int)tenth - 48) / 10.0f);
                if (is_negative)
                {
                    vec.y *= -1;
                }
            }
            else if (i == 2)
            {
                vec.z += (float)(((int)tenth - 48) / 10.0f);
                if (is_negative)
                {
                    vec.z *= -1;
                }
            }
            
        }

        Debug.Log("Resulting vector: " + vec);

        return vec;
    }

    public static string FloatToDecimalString(float n, int precision)
    {
        return PreciseVector.SetPrecision(n, 1) + "";
    }
}
