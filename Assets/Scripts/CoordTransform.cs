using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CoordTransform : MonoBehaviour
{
    public static float[,] getRotationMatrix(Quaternion quat)
    {
        float qx = quat.x;
        float qy = quat.y;
        float qz = quat.z;
        float qw = quat.w;

        float[,] rotationMatrix = new float[3,3] { 
            {1.0f - 2.0f*qz*qz - 2.0f*qw*qw, 2.0f*qy*qz - 2.0f*qw*qx, 2.0f*qy*qw + 2.0f*qz*qx},
            {2.0f*qy*qz + 2.0f*qw*qx, 1.0f - 2.0f*qy*qy - 2.0f*qw*qw, 2.0f*qz*qw - 2.0f*qy*qx},
            {2.0f*qy*qw - 2.0f*qz*qx, 2.0f*qz*qw + 2.0f*qy*qx, 1.0f - 2.0f*qy*qy - 2.0f*qz*qz}
        };

        return rotationMatrix;
    }

    public static float[,] Transpose(float[,] matrix)
    {
        int w = matrix.GetLength(0);
        int h = matrix.GetLength(1);
        float [,] result = new float[h, w];
        
        for (int i = 0; i < w; i++)
        {
            for (int j = 0; j < h; j++)
            {
                result[j, i] = matrix[i, j];
            }
        }
        return result;
    }

    public static float[,] MultiplyMatrix(float[,] A, float[,] B)
    {
        int rA = A.GetLength(0);
        int cA = A.GetLength(1);
        int rB = B.GetLength(0);
        int cB = B.GetLength(1);
        float temp = 0;
        float[,] result = new float[rA, cB];
        if (cA != rB)
        {
            Debug.Log("matrix can't be multiplied !!");
            return result;
        }
        else
        {
            for (int i = 0; i < rA; i++)
            {
                for (int j = 0; j < cB; j++)
                {
                    temp = 0;
                    for (int k = 0; k < cA; k++)
                    {
                        temp += A[i, k] * B[k, j];
                    }
                    result[i, j] = temp;
                }
            }
        return result;
        }
    }

    public static float[,] MinusMatrix(float[,] A, float[,] B)
    {
        int r = A.GetLength(0);
        int c = A.GetLength(1);
        float[,] result = new float[r, c];
        for (int i = 0; i < r; i++)
        {
            for (int j = 0; j < c; j++)
            {
                result[i, j] = A[i,j]-B[i,j];
            }
        }
        return result;
    }

    public static float[,] NegativeMatrix(float[,] matrix)
    {
        int w = matrix.GetLength(0);
        int h = matrix.GetLength(1);
        
        for (int i = 0; i < w; i++)
        {
            for (int j = 0; j < h; j++)
            {
                matrix[i, j] = -matrix[i, j];
            }
        }
        return matrix;
    }

}
