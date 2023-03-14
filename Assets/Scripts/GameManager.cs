using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public Tak tak;

    public UI ui;

    public static int currentPlayer = 1;

    private bool controlsLocked = false;

    public static bool gameOver = false;

    // Start is called before the first frame update
    void Start()
    {
        ui = GameObject.Find("Game Manager").GetComponent<UI>();

        List<Piece>[,] boardData = new List<Piece>[Settings.dimension, Settings.dimension];
        for (int i = 0; i < Settings.dimension; i++)
        {
            for (int j = 0; j < Settings.dimension; j++)
            {
                boardData[i, j] = new List<Piece>();
            }
        }
        tak = new Tak(boardData);

    }

    public void DoPlacement(Placement move)
    {
        if (!controlsLocked)
        {
            StartCoroutine(PlacePiece(move));
        }
    }

    private IEnumerator PlacePiece(Placement placement)
    {
        controlsLocked = true;
        if (tak.EndsGame(placement))
        {
            gameOver = true;
        }
        ui.DoPlacement(placement);
        tak.DoPlacement(placement);
        yield return new WaitForSeconds(GetSpawnTime(placement) + Settings.spawnCooldown);
        if (gameOver)
        {
            this.EndGame(currentPlayer);
        }
        else
        {
            this.NextPlayer();
        }
        controlsLocked = false;
    }


    public void DoCommute(Commute move)
    {
        if (!controlsLocked)
        {
            StartCoroutine(StartCommute(move));
        }
    }

    public IEnumerator StartCommute(Commute move)
    {
        controlsLocked = true;
        if (tak.EndsGame(move))
        {
            gameOver = true;
        }
        yield return StartCoroutine(ui.DoCommute(move));
        tak.DoCommute(move);
        if (gameOver)
        {
            this.EndGame(currentPlayer);
        }
        else
        {
            this.NextPlayer();
        }
        controlsLocked = false;
    }

    public void StartGame()
    {
        ui.InitalizeBoard(Settings.dimension);
        GameObject.Find("Title Camera").SetActive(false);
        ui.playerText.enabled = true;
        GameObject cameraFocus = FindObjectsOfType<PlayerControl>(true)[0].gameObject;
        cameraFocus.SetActive(true);
        GameObject.Find("Canvas/Title Screen").SetActive(false);
    }

    public void EndGame(int victor)
    {
        ui.victoryText.enabled = true;
        ui.victoryText.text = "Player " + victor + " wins!";

    }

    public void HomeScreen()
    {
        GameObject.Find("Canvas/End Screen").SetActive(false);
        GameObject board = GameObject.Find("Board");
        Destroy(board);
        GameObject.Find("Title Camera").SetActive(true);
        GameObject cameraFocus = FindObjectsOfType<PlayerControl>(true)[0].gameObject;
        cameraFocus.SetActive(true);
        GameObject.Find("Canvas/Title Screen").SetActive(true);


    }

    void NextPlayer()
    {
        currentPlayer = currentPlayer == 1 ? 2 : 1;
        ui.playerText.text = "Player " + currentPlayer;
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
 * - Animation is still bad; lag after first one
 * - Parent setting for animation must still be bad since I couldn't commute with the right pieces at one point.
 * - Couldnt make 3 jumps to flatten: W-B-Wc _ _ Bss
 */

/*
 * TODO:
 * - Victory effects (screen, highlight, road animation??)
 * - Textures
 * - Sounds effects
 * - Car drive across path
 * - DoMove usless in Tak
 */