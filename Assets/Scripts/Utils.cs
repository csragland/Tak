using UnityEngine;

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

    public static GameObject GetUITile(Tile tile)
    {
        return GameObject.Find("Board/Tile_" + IndexToNum(tile.row, tile.col));
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
}

public enum Comparisons
{
    LT, LTE, E, GTE, GT
}
