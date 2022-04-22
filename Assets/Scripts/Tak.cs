using System;
using System.Collections.Generic;

public class Tak
{
    public List<Piece>[,] board;


    public Tak(List<Piece>[,] takBoard)
    {
        this.board = takBoard;
    }

    public bool EndsGame(Move move)
    {
        return false;
    }

    public bool IsLegalMove(Move move)
    { 
        if (move.GetType() == typeof(Placement))
        {
            return this.IsLegalMove((Placement) move);

        }
        if (move.GetType() == typeof(Commute))
        {
            return this.IsLegalMove((Commute) move);
        }
        return false;
    }

    public void DoPlacement(Placement move)
    {
        if (IsLegalMove(move))
        {
            Piece piece = new Piece(move.piece, move.player);
            this.board[move.destination.row, move.destination.col].Add(piece);
        }
    }

    public void DoCommute(Commute move)
    {
        if (this.IsLegalMove(move))
        {
            foreach (var jump in move.jumps)
            {
                this.DoJump(jump);
            }
        }
    }

    public void DoJump(Jump jump)
    {
        List<Piece> startStack = this.board[jump.origin.row, jump.origin.col];
        List<Piece> endStack = this.board[jump.destination.row, jump.destination.col];
        while (startStack.Count - jump.cutoff >= 0)
        {
            endStack.Add(startStack[jump.cutoff]);
            startStack.RemoveAt(jump.cutoff);
        }
    }

    private bool IsLegalMove(Placement move)
    {
        return this.board[move.destination.row, move.destination.col].Count == 0;
    }

    private bool IsLegalMove(Commute move)
    {
        int[] direction = move.jumps[0].GetDirection();
        if (Utils.OneNorm(direction) != 1)
        {
            return false;
        }

        for (int i = 0; i < move.jumps.Count; i++)
        {
            Jump jump = move.jumps[i];

            if (jump.GetDirection() != direction)
            {
                return false;
            }

            List<Piece> startStack = this.board[jump.origin.row, jump.origin.col];
            if (startStack.Count == 0)
            {
                return false;
            }

            if ((jump.cutoff < 1 && i > 0) || startStack.Count - jump.cutoff > GameManager.dimension)
            {
                return false;
            }

            List<Piece> endStack = this.board[jump.destination.row, jump.destination.col];
            Piece startCrown = startStack[startStack.Count - 1];
            if (startCrown.player != move.player)
            {
                return false;
            }

            Piece endCrown = endStack[endStack.Count - 1];
            if (endCrown.type == PieceType.CAPSTONE)
            {
                return false;
            }
            if (endCrown.type == PieceType.BLOCKER && !(startCrown.type == PieceType.CAPSTONE && jump.cutoff == startStack.Count - 1))
            {
                return false;
            }
        }
        return true;
    }

    public bool ReachesEdge(int player, Tile tile, bool[,] visited, Tile origin)
    {
        if (Utils.Distance(tile.row, origin.row) >= GameManager.dimension - 1 || Utils.Distance(tile.col, origin.col) >= GameManager.dimension - 1)
        {
            return true;
        }

        visited[tile.row, tile.col] = true;

        foreach (var neighbor in this.GetNeighbors(player, tile))
        {
            if (!visited[neighbor.row, neighbor.col])
            {
                if (ReachesEdge(player, neighbor, visited, origin))
                {
                    return true;
                }
                //    return true
                //return false || ReachesEdge(player, neighbor, visited, origin);
            }
        }

        return false;
    }

    public List<Tile> GetNeighbors(int player, Tile tile)
    {
        List<Tile> neighbors = new List<Tile>();

        if (tile.row < GameManager.dimension - 1)
        {
            Tile tileUp = new Tile(tile.row + 1, tile.col);
            if (this.GetCrown(tileUp).player == player)
            {
                neighbors.Add(tileUp);
            }
        }
        if (tile.row > 0)
        {
            Tile tileDown = new Tile(tile.row - 1, tile.col);
            if (this.GetCrown(tileDown).player == player)
            {
                neighbors.Add(tileDown);
            }
        }
        if (tile.col < GameManager.dimension - 1)
        {
            Tile tileRight = new Tile(tile.row, tile.col + 1);
            if (this.GetCrown(tileRight).player == player)
            {
                neighbors.Add(tileRight);
            }
        }
        if (tile.col > 0)
        {
            Tile tileLeft = new Tile(tile.row, tile.col - 1);
            if (this.GetCrown(tileLeft).player == player)
            {
                neighbors.Add(tileLeft);
            }
        }

        return neighbors;
    }

    public Piece GetCrown(Tile tile)
    {
        List<Piece> stack = this.board[tile.row, tile.col];
        return stack[stack.Count - 1];
    }

}
