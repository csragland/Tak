using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;

public class Tak
{
    public List<Piece>[,] board;

    public List<(Move, PoppingInfo)> moveStack;

    public int[,] piecesSpawned = new int[,] { {0, 0}, {0, 0} };

    public int[] maxPieces = new int[] { 17, 1 };

    public int turnNum = 1;
    
    public Tak(List<Piece>[,] takBoard)
    {
        this.board = takBoard;
    }

    public void DoMove(Move move)
    {
        if (move.GetType() == typeof(Placement))
        {
            this.DoPlacement((Placement)move);
            this.moveStack.Add((move, null));
        }
        if (move.GetType() == typeof(Commute))
        {
            this.DoCommute((Commute)move);
        }
        this.turnNum += 1;
    }

    public void UndoMove(Move move)
    {
        if (move.GetType() == typeof(Placement))
        {
            this.DoPlacement((Placement)move);
        }
        if (move.GetType() == typeof(Commute))
        {
            this.DoCommute((Commute)move);
        }
        this.turnNum += 1;
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
        int[] spans = this.FindSpan(move.player, move.destination, visited);
        return Math.Max(spans[1] - spans[0], spans[3] - spans[2]) >= Settings.dimension - 1;
    }

    public void DoPlacement(Placement move)
    {
        if (IsLegalMove(move))
        {
            //int pieceOwner = this.turnNum > 2 ? move.player : move.player == 1 ? 2 : 1;
            Piece piece = new Piece(move.piece, move.player);
            this.board[move.destination.row, move.destination.col].Add(piece);
            int pieceIndex = move.piece == PieceType.CAPSTONE ? 1 : 0;
            this.piecesSpawned[move.player - 1, pieceIndex] += 1;
        }
    }

    public void UndoPlacement(Placement move)
    {
        List<Piece> stack = this.board[move.destination.row, move.destination.col];
        stack.RemoveAt(stack.Count - 1);
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

    public void UndoCommute((Commute, PoppingInfo) commuteData)
    {
        // commute, int[] numJumpers, flatten
        Commute commute = commuteData.Item1;
        PoppingInfo info = commuteData.Item2;
        Jump first = commute.jumps[0];
        Jump last = commute.jumps[^1];
        List<Piece> restack = new();
        for (int i = 0; i < commute.jumps.Count; i++)
        {
            Jump jump = commute.jumps[i];
            int next = i == commute.jumps.Count - 1 ? 0 : info.numPieces[i + 1];
            int numPiecesToAdd = info.numPieces[i] - next;
            List<Piece> targetStack = this.board[jump.destination.row, jump.destination.col];
            restack.AddRange(targetStack.GetRange(targetStack.Count - numPiecesToAdd, numPiecesToAdd));
            targetStack.RemoveRange(targetStack.Count - numPiecesToAdd, numPiecesToAdd);
        }
        this.board[first.origin.row, first.origin.col].AddRange(restack);
        if (info.doesFlatten)
        {
            List<Piece> final = this.board[last.destination.row, last.destination.col];
            final[^1].type = PieceType.BLOCKER;
        }
    }

    private void DoJump(Jump jump)
    {
        List<Piece> startStack = this.board[jump.origin.row, jump.origin.col];
        List<Piece> endStack = this.board[jump.destination.row, jump.destination.col];
        // Flatten Standing Stones with lone Capstones
        if (this.JumpWillFlatten(jump))
        {
            endStack[^1].type = PieceType.STONE;
        }
        endStack.AddRange(startStack.GetRange(jump.cutoff, startStack.Count - 1));
        startStack.RemoveRange(jump.cutoff, startStack.Count - 1);
        //while (startStack.Count - jump.cutoff > 0)
        //{
        //    endStack.Add(startStack[jump.cutoff]);
        //    startStack.RemoveAt(jump.cutoff);
        //}
    }

    public bool JumpWillFlatten(Jump jump)
    {
        Piece jumper = this.GetCrown(jump.origin);
        Piece victim = this.GetCrown(jump.destination);
        return this.board[jump.origin.row, jump.origin.col].Count - 1 == jump.cutoff
            && (jumper is not null) && jumper.type == PieceType.CAPSTONE
            && (victim is not null) && victim.type == PieceType.BLOCKER;
    }

    private bool IsLegalMove(Placement move)
    {
        int pieceIndex = move.piece == PieceType.CAPSTONE ? 1 : 0;
        if (this.turnNum < 3 && move.piece != PieceType.STONE)
        {
            return false;
        }
        return this.board[move.destination.row, move.destination.col].Count == 0 && piecesSpawned[move.player - 1, pieceIndex] < maxPieces[pieceIndex];
    }

    private bool IsLegalMove(Commute move)
    {
        int[] direction = move.jumps[0].GetDirection();
        Piece startCrown = GetCrown(move.jumps[0].origin);


        if (Utils.OneNorm(direction) != 1)
        {
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
            Debug.Log(startStack.Count);
            if (i == 0 && startStack.Count == 0)
            {
                return false;
            }

            if ((jump.cutoff < 1 && i > 0) || startStack.Count - jump.cutoff > Settings.dimension)
            {
                return false;
            }

            if (i == 0 && startCrown.player != move.player)
            {
                return false;
            }

            if (this.board[jump.destination.row, jump.destination.col].Count == 0)
            {
                continue;
            }

            Piece endCrown = this.GetCrown(jump.destination);
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

    public int[] FindSpan(int player, Tile tile, bool[,] visited)
    {
        visited[tile.row, tile.col] = true;
        int[] span = { tile.row, tile.row, tile.col, tile.col };

        foreach (var neighbor in this.GetNeighbors(player,tile))
        {
            if (!visited[neighbor.row, neighbor.col])
            {
                int [] neighborSpan = FindSpan(player, neighbor, visited);
                span[0] = Math.Min(neighborSpan[0], span[0]);
                span[1] = Math.Max(neighborSpan[1], span[1]);
                span[2] = Math.Min(neighborSpan[2], span[2]);
                span[3] = Math.Max(neighborSpan[3], span[3]);
            }
        }
        return span;
    }

    public List<Tile> GetNeighbors(int player, Tile tile)
    {
        List<Tile> neighbors = new List<Tile>();

        if (tile.row < Settings.dimension - 1)
        {
            Tile tileUp = new Tile(tile.row + 1, tile.col);
            Piece crown = this.GetCrown(tileUp);
            if (crown is not null && crown.player == player && crown.type != PieceType.BLOCKER)
            {
                neighbors.Add(tileUp);
            }
        }
        if (tile.row > 0)
        {
            Tile tileDown = new Tile(tile.row - 1, tile.col);
            Piece crown = this.GetCrown(tileDown);
            if (crown is not null && crown.player == player && crown.type != PieceType.BLOCKER)
            {
                neighbors.Add(tileDown);
            }
        }
        if (tile.col < Settings.dimension - 1)
        {
            Tile tileRight = new Tile(tile.row, tile.col + 1);
            Piece crown = this.GetCrown(tileRight);
            if (crown is not null && crown.player == player && crown.type != PieceType.BLOCKER)
            {
                neighbors.Add(tileRight);
            }
        }
        if (tile.col > 0)
        {
            Tile tileLeft = new Tile(tile.row, tile.col - 1);
            Piece crown = this.GetCrown(tileLeft);
            if (crown is not null && crown.player == player && crown.type != PieceType.BLOCKER)
            {
                neighbors.Add(tileLeft);
            }
        }
        return neighbors;
    }

    public Piece GetCrown(Tile tile)
    {
        List<Piece> stack = this.board[tile.row, tile.col];
        if (stack.Count == 0)
        {
            return null;
        }
        return stack[^1];
    }

}
