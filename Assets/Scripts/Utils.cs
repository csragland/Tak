using System;
using System.Collections.Generic;

public static class Utils
{
    public static bool IsEven(int num)
    {
        return num % 2 == 0;
    }

    public static int IndexToNum(int rowIndex, int colIndex, int dim)
    {
        return rowIndex * dim + colIndex;
    }

    public static int OneNorm(int[] direction)
    {
        int sum = 0;
        foreach (var number in direction)
        {
            sum += Math.Abs(number);
        }
        return sum;
    }

    public static int Distance(int x, int y)
    {
        return Math.Abs(x - y);
    }

    public static object GetLast(List<object> list)
    {
        return list[list.Count - 1];
    }
}
