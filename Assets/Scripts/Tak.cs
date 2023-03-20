using System;
using System.Linq;
using System.Collections.Generic;

public class Tak
{
    private List<Piece>[,] board;

    private Stack<(Move, PoppingInfo)> moveStack = new();

    private int[,] numPiecesSpawned = new int[,] { {0, 0}, {0, 0} };

    private readonly int[] maxNumPieces = new int[] { 17, 1 };

    public int turnNum = 1;
    
    public Tak(int dimension)
    {
        List<Piece>[,] board = new List<Piece>[dimension, dimension];
        for (int i = 0; i < dimension; i++)
        {
            for (int j = 0; j < dimension; j++)
            {
                board[i, j] = new List<Piece>();
            }
        }
        this.board = board;
    }

    public void DoMove(Move move)
    {
        if (move.GetType() == typeof(Placement))
        {
            this.DoPlacement((Placement)move);
            this.moveStack.Push((move, null));
        }
        if (move.GetType() == typeof(Commute))
        {
            this.DoCommute((Commute)move);
        }
        this.turnNum += 1;
    }

    public void UndoMove()
    {
        (Move, PoppingInfo) previous = moveStack.Pop();
        if (previous.Item1.GetType() == typeof(Placement))
        {
            this.UndoPlacement(previous);
        }
        if (previous.Item1.GetType() == typeof(Commute))
        {
            this.UndoCommute(((Commute, PoppingInfo))previous);
        }
        this.turnNum -= 1;
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

    public bool CommuteWillFlatten(Commute commute)
    {
        Piece jumper = this.GetCrown(commute.jumps[0].origin);
        Piece victim = this.GetCrown(commute.jumps[^1].destination);
        return (jumper is not null) && jumper.type == PieceType.CAPSTONE
            && (victim is not null) && victim.type == PieceType.BLOCKER;
    }

    public bool EndsGame(Move move)
    {
        bool[,] visited = new bool[Settings.dimension, Settings.dimension];
        int[] spans = this.FindSpan(move.player, move.destination, visited);
        // Check if there exists a path connecting opposites sides of the board that passes
        // through the tile where the Player just did an action
        return Math.Max(spans[1] - spans[0], spans[3] - spans[2]) >= Settings.dimension - 1;
    }

    private void DoPlacement(Placement move)
    {
        Piece piece = new(move.piece, move.player);
        this.board[move.destination.row, move.destination.col].Add(piece);
        int pieceIndex = move.piece == PieceType.CAPSTONE ? 1 : 0;
        this.numPiecesSpawned[move.player - 1, pieceIndex] += 1;
    }

    private void UndoPlacement((Move, PoppingInfo) placementData)
    {
        Move move = placementData.Item1;
        List<Piece> stack = this.board[move.destination.row, move.destination.col];
        stack.RemoveAt(stack.Count - 1);
    }

    private void DoCommute(Commute move)
    {

        PoppingInfo fullInfo = new(new int[move.jumps.Count], false);
        int numPiecesJumped;
        for (int i = 0; i < move.jumps.Count; i++)
        {
            numPiecesJumped = DoJump(move.jumps[i]);
            fullInfo.numPieces[i] = numPiecesJumped;
        }
        fullInfo.doesFlatten = CommuteWillFlatten(move);
        if (fullInfo.doesFlatten)
        {
            Jump last = move.jumps[^1];
            List<Piece> endStack = this.board[last.destination.row, last.destination.col];
            endStack[^1].type = PieceType.STONE;
        }
        this.moveStack.Push((move, fullInfo));
    }


    private void UndoCommute((Commute, PoppingInfo) commuteData)
    {
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
    
    private int DoJump(Jump jump)
    {
        List<Piece> startStack = this.board[jump.origin.row, jump.origin.col];
        List<Piece> endStack = this.board[jump.destination.row, jump.destination.col];
        int numPieces = startStack.Count - jump.cutoff;
        endStack.AddRange(startStack.GetRange(jump.cutoff, numPieces));
        startStack.RemoveRange(jump.cutoff, numPieces);
        return numPieces;
    }

    private bool IsLegalMove(Placement move)
    {
        int pieceIndex = move.piece == PieceType.CAPSTONE ? 1 : 0;
        if (this.turnNum < 3 && move.piece != PieceType.STONE)
        {
            return false;
        }
        return this.board[move.destination.row, move.destination.col].Count == 0 && numPiecesSpawned[move.player - 1, pieceIndex] < maxNumPieces[pieceIndex];
    }

    private bool IsLegalMove(Commute move)
    {
        int[] direction = move.jumps[0].GetDirection();
        Piece startCrown = GetCrown(move.jumps[0].origin);

        // Player can only jump one tile away
        if (Utils.OneNorm(direction) != 1)
        {
            return false;
        }

        int[] numPiecesMoved = new int[move.jumps.Count];
        for (int i = 0; i < move.jumps.Count; i++)
        {
            Jump jump = move.jumps[i];
            List<Piece> startStack = this.board[jump.origin.row, jump.origin.col];

            if (i > 0)
            {
                numPiecesMoved[i] += numPiecesMoved[i - 1];
            }
            numPiecesMoved[i] += startStack.Count - jump.cutoff;

            // All jumps have to be in the same direction as the first
            if (!jump.GetDirection().SequenceEqual(direction))
            {
                return false;
            }

            // Pieces must exist in order to do jump
            if (i == 0 && startStack.Count == 0)
            {
                return false;
            }

            // Player must leave one piece behind after each jump, and they can't carry more than <dimension> in a jump
            if ((jump.cutoff < 1 && i > 0) || startStack.Count - jump.cutoff > Settings.dimension)
            {
                return false;
            }

            // Player can only jump with a stack they control
            if (i == 0 && startCrown.player != move.player)
            {
                return false;
            }

            // This was a legal jump if the player is jumping to an empty tile
            if (this.board[jump.destination.row, jump.destination.col].Count == 0)
            {
                continue;
            }

            Piece endCrown = this.GetCrown(jump.destination);
            // Nothing can jump on a Capstone
            if (endCrown.type == PieceType.CAPSTONE)
            {
                return false;
            }
            // Only Capstones can jump on standing stones if they are travelling alone
            if (endCrown.type == PieceType.BLOCKER && !(startCrown.type == PieceType.CAPSTONE && numPiecesMoved[i] == 1))
            {
                return false;
            }
        }

        return true;
    }

    // Use DFS to find the maximum and minimun indecies reachable from a tile via a chain of tiles the player controls
    private int[] FindSpan(int player, Tile tile, bool[,] visited)
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

    // Check the 4 2D directions and see which tiles in that direction have pieces you own as crowns
    private List<Tile> GetNeighbors(int player, Tile tile)
    {
        List<Tile> neighbors = new();

        if (tile.row < Settings.dimension - 1)
        {
            Tile tileUp = new(tile.row + 1, tile.col);
            Piece crown = this.GetCrown(tileUp);
            if (crown is not null && crown.player == player && crown.type != PieceType.BLOCKER)
            {
                neighbors.Add(tileUp);
            }
        }
        if (tile.row > 0)
        {
            Tile tileDown = new(tile.row - 1, tile.col);
            Piece crown = this.GetCrown(tileDown);
            if (crown is not null && crown.player == player && crown.type != PieceType.BLOCKER)
            {
                neighbors.Add(tileDown);
            }
        }
        if (tile.col < Settings.dimension - 1)
        {
            Tile tileRight = new(tile.row, tile.col + 1);
            Piece crown = this.GetCrown(tileRight);
            if (crown is not null && crown.player == player && crown.type != PieceType.BLOCKER)
            {
                neighbors.Add(tileRight);
            }
        }
        if (tile.col > 0)
        {
            Tile tileLeft = new(tile.row, tile.col - 1);
            Piece crown = this.GetCrown(tileLeft);
            if (crown is not null && crown.player == player && crown.type != PieceType.BLOCKER)
            {
                neighbors.Add(tileLeft);
            }
        }
        return neighbors;
    }

    // Return the top piece of a stack
    private Piece GetCrown(Tile tile)
    {
        List<Piece> stack = this.board[tile.row, tile.col];
        if (stack.Count == 0)
        {
            return null;
        }
        return stack[^1];
    }

    private string StringifyStack(List<Piece> stack)
    {
        if (stack.Count == 0)
        {
            return "_";
        }
        string s = "";
        foreach (Piece piece in stack)
        {
            s += piece + ", ";
        }
        return s;
    }
}
