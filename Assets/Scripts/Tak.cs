using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;

public class Tak
{
    public List<Piece>[,] board;

    public int[,] piecesSpawned = new int[,] { {0, 0}, {0, 0} };

    public int[] maxPieces = new int[] { 17, 1 };
    
    public Tak(List<Piece>[,] takBoard)
    {
        this.board = takBoard;
    }

    public void DoMove(Move move)
    {
        if (move.GetType() == typeof(Placement))
        {
            this.DoPlacement((Placement)move);
        }
        if (move.GetType() == typeof(Commute))
        {
            this.DoCommute((Commute)move);
        }
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

    public bool EndsGame(Move move)
    {
        bool[,] visited = new bool[Settings.dimension, Settings.dimension];
        int[] spans = this.FindSpan(move.player, move.destination, visited, move.destination);
        return Math.Max(spans[1] - spans[0], spans[3] - spans[2]) >= Settings.dimension - 1;
    }

    public void DoPlacement(Placement move)
    {
        if (IsLegalMove(move))
        {
            Piece piece = new Piece(move.piece, move.player);
            this.board[move.destination.row, move.destination.col].Add(piece);
            int pieceIndex = move.piece == PieceType.CAPSTONE ? 1 : 0;
            this.piecesSpawned[move.player - 1, pieceIndex] += 1;
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
        while (startStack.Count - jump.cutoff > 0)
        {
            endStack.Add(startStack[jump.cutoff]);
            startStack.RemoveAt(jump.cutoff);
        }
    }

    private bool IsLegalMove(Placement move)
    {
        int pieceIndex = move.piece == PieceType.CAPSTONE ? 1 : 0;
        return this.board[move.destination.row, move.destination.col].Count == 0 && piecesSpawned[move.player - 1, pieceIndex] < maxPieces[pieceIndex];
    }

    private bool IsLegalMove(Commute move)
    {
        int[] direction = move.jumps[0].GetDirection();
        Piece startCrown = GetCrown(move.jumps[0].origin);


        if (Utils.OneNorm(direction) != 1)
        {
            Debug.Log("1");
            return false;
        }

        for (int i = 0; i < move.jumps.Count; i++)
        {
            Jump jump = move.jumps[i];

            if (!jump.GetDirection().SequenceEqual(direction))
            {
                return false;
            }

            List<Piece> startStack = this.board[jump.origin.row, jump.origin.col];
            if (i == 0 && startStack.Count == 0)
            {
                Debug.Log("3");
                return false;
            }

            if ((jump.cutoff < 1 && i > 0) || startStack.Count - jump.cutoff > Settings.dimension)
            {
                Debug.Log("4");
                return false;
            }

            if (i == 0 && startCrown.player != move.player)
            {
                Debug.Log("5");
                Debug.Log(startCrown.player + " vs " + move.player);
                return false;
            }

            if (this.board[jump.destination.row, jump.destination.col].Count == 0)
            {
                continue;
            }

            Piece endCrown = this.GetCrown(jump.destination);
            if (endCrown.type == PieceType.CAPSTONE)
            {
                Debug.Log("6");
                return false;
            }
            if (endCrown.type == PieceType.BLOCKER && !(startCrown.type == PieceType.CAPSTONE && jump.cutoff == startStack.Count - 1))
            {
                Debug.Log("7");
                return false;
            }
        }
        return true;
    }

    public int[] FindSpan(int player, Tile tile, bool[,] visited, Tile origin)
    {
        visited[tile.row, tile.col] = true;
        int[] most = { int.MaxValue, int.MinValue, int.MaxValue, int.MinValue };

        foreach (var neighbor in this.GetNeighbors(player,tile))
        {
            if (!visited[neighbor.row, neighbor.col])
            {
                most = FindSpan(player, neighbor, visited, origin);
                most[0] = Math.Min(most[0], neighbor.row);
                most[1] = Math.Max(most[1], neighbor.row);
                most[2] = Math.Min(most[2], neighbor.col);
                most[3] = Math.Max(most[3], neighbor.col);
            }
        }

        return most;
    }

    public List<Tile> GetNeighbors(int player, Tile tile)
    {
        List<Tile> neighbors = new List<Tile>();

        if (tile.row < Settings.dimension - 1)
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
        if (tile.col < Settings.dimension - 1)
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
