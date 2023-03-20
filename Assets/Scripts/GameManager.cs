using System.Collections;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public Tak tak;

    public UI ui;

    public static int currentPlayer = 1;

    private static bool gameOver = false;

    private bool controlsLocked = false;


    void Start()
    {
        ui = GameObject.Find("Game Manager").GetComponent<UI>();

        tak = new Tak(Settings.dimension);

    }

    // Generate board, change view from title screen to gameplay
    public void StartGame()
    {
        ui.InitalizeBoard(Settings.dimension);
        GameObject.Find("Title Camera").SetActive(false);
        ui.playerText.enabled = true;
        GameObject cameraFocus = FindObjectsOfType<PlayerControl>(true)[0].gameObject;
        cameraFocus.SetActive(true);
        GameObject.Find("Canvas/Title Screen").SetActive(false);
    }

    // Display victory screen and freeze game
    public void EndGame(int victor)
    {
        ui.victoryText.enabled = true;
        ui.victoryText.text = "Player " + victor + " wins!";
        controlsLocked = true;
    }

    // Do move if controls not locked
    public void DoMove(Move move)
    {
        if (controlsLocked)
        {
            return;
        }
        StartCoroutine(ExecuteMove(move));
    }

    // Change from game view to home screen
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

    // Coroutine for all move logic
    private IEnumerator ExecuteMove(Move move)
    {
        // Check if the move about to be executed will conclude the game
        if (tak.EndsGame(move))
        {
            gameOver = true;
        }

        // Lock controls so that the player cannot do a move while the current move is in progress
        controlsLocked = true;
        // First do move in the UI, which takes time, then update the game state in the tak object
        if (move.GetType() == typeof(Placement))
        {
            yield return StartCoroutine(ui.DoPlacement((Placement)move));
        }
        if (move.GetType() == typeof(Commute))
        {
            yield return StartCoroutine(ui.DoCommute((Commute)move));
        }
        tak.DoMove(move);
        controlsLocked = false;

        // If the game is over, do endgame actions. Else, move on to the next turn.
        if (gameOver)
        {
            this.EndGame(currentPlayer);
        }
        else
        {
            this.NextPlayer();
        }
    }

    // Do logic for cycling to the next player
    private void NextPlayer()
    {
        currentPlayer = currentPlayer == 1 ? 2 : 1;
        ui.playerText.text = "Player " + currentPlayer;
    }

}

/*
 * BUGS:
 * - EndsGame function only checks if commute finishes at winning tile
 * - I somehow skipped player 2 when spawning pieces by going quickly
 * - I locked up the wasd controlls earlier
 * - Animation is still bad; lag after first one
 * - Parent setting for animation must still be bad since I couldn't commute with the right pieces at one point.
 */

/*
 * TODO:
 * - Victory effects (screen, highlight, road animation??)
 * - Textures
 * - Sounds effects
 * - Apply matricies to CheckArrowOver?
 */