using System.Collections;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public Tak tak;

    public UI ui;

    public static int currentPlayer = 1;

    private static bool gameOver = false;

    private bool controlsLocked = false;


    // Start is called before the first frame update
    void Start()
    {
        ui = GameObject.Find("Game Manager").GetComponent<UI>();

        tak = new Tak(Settings.dimension);

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
        controlsLocked = true;
    }

    public void DoMove(Move move)
    {
        if (controlsLocked)
        {
            return;
        }
        StartCoroutine(ExecuteMove(move));
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

    private IEnumerator ExecuteMove(Move move)
    {

        if (tak.EndsGame(move))
        {
            gameOver = true;
        }

        if (move.GetType() == typeof(Placement))
        {
            yield return StartCoroutine(PlacePiece((Placement)move));
        }
        if (move.GetType() == typeof(Commute))
        {
            yield return StartCoroutine(StartCommute((Commute)move));
        }
        tak.DoMove(move);

        if (gameOver)
        {
            this.EndGame(currentPlayer);
        }
        else
        {
            this.NextPlayer();
        }
    }

    private IEnumerator PlacePiece(Placement placement)
    {
        controlsLocked = true;
        ui.DoPlacement(placement);
        yield return new WaitForSeconds(GetSpawnTime(placement) + Settings.spawnCooldown);
        controlsLocked = false;
    }

    private IEnumerator StartCommute(Commute move)
    {
        controlsLocked = true;
        yield return StartCoroutine(ui.DoCommute(move));
        controlsLocked = false;
    }

    private void NextPlayer()
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
 */