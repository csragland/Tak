using System.Collections.Generic;


public enum PieceType
{
    STONE, BLOCKER, CAPSTONE
}

public class Piece
{
    public PieceType type;
    public int player;

    public Piece(PieceType type, int player)
    {
        this.type = type;
        this.player = player;
    }
}

public class Tile
{
    public int row;
    public int col;

    public Tile(int rowNumber, int columnNumber)
    {
        this.row = rowNumber;
        this.col = columnNumber;
    }

    public override string ToString()
    {
        return "[" + this.row + ", " + this.col + "]";
    }
}

public class Jump
{
    public Tile origin;
    public Tile destination;
    public int cutoff;

    public Jump(int baseIndex, Tile fromTile, Tile toTile)
    {
        this.cutoff = baseIndex;
        this.origin = fromTile;
        this.destination = toTile;
    }

    public int[] GetDirection()
    {
        return new int[] { this.destination.row - this.origin.row, this.destination.col - this.origin.col };
    }
}

public abstract class Move
{
    public int player;
    public Tile destination;

}

public class Placement : Move
{
    public PieceType piece;

    public Placement(int playerNumber, PieceType type, Tile tile)
    {
        this.player = playerNumber;
        this.piece = type;
        this.destination = tile;
    }
}

public class Commute : Move
{
    public List<Jump> jumps;

    public Commute(int playerNumber, List<Jump> path)
    {
        this.player = playerNumber;
        this.jumps = path;
        this.destination = path[path.Count - 1].destination;
    }
}