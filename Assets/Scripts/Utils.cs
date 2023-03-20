using UnityEngine;
using System.Collections.Generic;

public static class Utils
{
    public static bool IsEven(int num)
    {
        return num % 2 == 0;
    }

    public static int IndexToNum(int rowIndex, int colIndex)
    {
        return rowIndex * Settings.dimension + colIndex;
    }

    public static Tile NumToTile(int num)
    {
        return new Tile((int) Mathf.Floor(num / Settings.dimension), num % Settings.dimension);
    }

    public static GameObject GetUITile(Tile tile)
    {
        return GameObject.Find("Board/Tile_" + IndexToNum(tile.row, tile.col));
    }

    public static GameObject GetUITile(int tileNum)
    {
        return GameObject.Find("Board/Tile_" + tileNum);
    }

    public static bool IsOnHorizontalEdge(int tileNum, int dim)
    {
        return tileNum % dim == 0 || tileNum % dim == dim - 1;
    }

    public static bool IsOnVerticalEdge(int tileNum, int dim)
    {
        return tileNum < dim || tileNum < dim * (dim - 1);
    }

    public static int OneNorm(int[] direction)
    {
        int sum = 0;
        foreach (var number in direction)
        {
            sum += Mathf.Abs(number);
        }
        return sum;
    }

    public static float[] JumpPhysics(Vector3 start, Vector3 end)
    {
        Vector3 displacement = end - start;
        float necessaryVy = displacement.y > 0 ? Mathf.Sqrt(-2 * Physics.gravity.y * displacement.y) : 0;
        float overshootVy = Mathf.Sqrt(-2 * Physics.gravity.y * Settings.overshootHeight);
        float verticalVelocity = necessaryVy + overshootVy;
        float totalTime = (-verticalVelocity - Mathf.Sqrt(verticalVelocity * verticalVelocity + 2 * Physics.gravity.y * displacement.y)) / Physics.gravity.y;
        Debug.Assert(Settings.tileDimensions.x == Settings.tileDimensions.z);
        float horizontalVelocty = Settings.tileDimensions.x / totalTime;
        return new float[] { horizontalVelocty, verticalVelocity, totalTime };
    }

    public static void PrintArray(int[] arr)
    {
        string msg = "[";
        foreach (var item in arr)
        {
            msg += ", " + item;
        }
        msg += "]";
        Debug.Log(msg);
    }

    public static void PrintList(List<object> arr)
    {
        string msg = "[";
        foreach (var item in arr)
        {
            msg += ", " + item;
        }
        msg += "]";
        Debug.Log(msg);
    }
}