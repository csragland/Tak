using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public Tak tak;

    public UI ui;

    public static int currentPlayer = 1;

    public static bool gameOver = false;

    // Start is called before the first frame update
    void Start()
    {
        ui = GameObject.Find("Game Manager").GetComponent<UI>();

        List<Piece>[,] boardData = new List<Piece>[Settings.dimension,Settings.dimension];
        for (int i = 0; i < Settings.dimension; i++)
        {
            for (int j = 0; j < Settings.dimension; j++)
            {
                boardData[i, j] = new List<Piece>();
            }
        }
        tak = new Tak(boardData);
        ui.InitalizeBoard(Settings.dimension);
    }

    public void DoPlacement(Placement move)
    {
        StartCoroutine(PlacePiece(move));
    }

    private IEnumerator PlacePiece(Placement placement)
    {
        tak.DoPlacement(placement);
        ui.DoPlacement(placement);
        yield return new WaitForSeconds(GetSpawnTime(placement) + Settings.spawnCooldown);
        this.NextPlayer();
    }

    public void DoCommute(Commute commute)
    {
        ui.DoCommute(commute);
        tak.DoCommute(commute);
        this.NextPlayer();
    }

    void NextPlayer()
    {
        currentPlayer = currentPlayer == 1 ? 2 : 1;
    }

    private float GetSpawnTime(Placement placement)
    {
        if (placement.piece == PieceType.STONE)
        {
            return Settings.stoneSpawnTime;
        }
        else if (placement.piece == PieceType.BLOCKER)
        {
            return Settings.blockerSpawnTime;
        }
        return Settings.capstoneSpawnTime;
    }

}

/*
 * BUGS:
 * - I somehow skipped player 2 when spawning pieces by going quickly
 * - I locked up the wasd controlls earlier
 * - The wrong piece somehow did not end up on top (even though it looks that way). happens when big stack jumps on small
 * - Choosing the top three on a stack of 5+ makes it crap out and swap around. This is becasue of timing for setting parent, it seems.
 */