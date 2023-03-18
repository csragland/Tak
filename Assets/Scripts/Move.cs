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

    public override string ToString()
    {
        string playerString = player == 1 ? "W" : "B";
        string pieceString = type == PieceType.STONE ? "S" : type == PieceType.BLOCKER ? "Ss" : "Cs";
        return playerString + "_" + pieceString;
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

    public override string ToString()
    {
        int[] direction = GetDirection();
        string directionString;
        if (direction[0] == 1)
        {
            directionString = "down";
        }
        else if (direction[0] == -1)
        {
            directionString = "up";
        }
        else if (direction[1] == 1)
        {
            directionString = "right";
        }
        else
        {
            directionString = "left";
        }
        return this.cutoff + "- " + directionString;
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

public class PoppingInfo
{
    public int[] numPieces;
    public bool doesFlatten;

    public PoppingInfo(int[] numPieces, bool doesFlatten)
    {
        this.numPieces = numPieces;
        this.doesFlatten = doesFlatten;
    }
}