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

    // Update is called once per frame
    void Update()
    {

    }

    public void DoPlacement(Placement placement)
    {
        tak.DoPlacement(placement);
        ui.DoPlacement(placement);
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

}
