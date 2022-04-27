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
        StackItUp();
        Invoke("Commute1", 12);
        Invoke("Commute2", 14);
        Invoke("Commute3", 16);
    }

    // Update is called once per frame
    void Update()
    {

    }

    void StackItUp()
    {
        int pieces = 10;
        float time = 1f;
        for (int i = 0; i < pieces; i++)
        {
            Invoke("PlacePiece", time * i);
        }
    }

    void PlacePiece()
    {
        int player = Random.Range(1, 3);
        ui.DoPlacement(new Placement(player, PieceType.STONE, new Tile(2, 2)));
        tak.DoPlacement(new Placement(player, PieceType.STONE, new Tile(2, 2)));
    }

    void Commute1()
    {
        List<Jump> jumps = new List<Jump>();
        jumps.Add(new Jump(0, new Tile(2, 2), new Tile(2, 1)));
        //DoCommute(new Commute(jumps));
        ui.DoCommute(new Commute(jumps));
        //currentPlayer = 2;
    }

    void Commute2()
    {
        List<Jump> jumps = new List<Jump>();
        jumps.Add(new Jump(0, new Tile(2, 1), new Tile(1, 1)));
        //DoCommute(new Commute(jumps));
        ui.DoCommute(new Commute(jumps));
        //currentPlayer = 2;
    }

    void Commute3()
    {
        List<Jump> jumps = new List<Jump>();
        jumps.Add(new Jump(0, new Tile(1, 1), new Tile(1, 2)));
        //DoCommute(new Commute(jumps));
        ui.DoCommute(new Commute(jumps));
        //currentPlayer = 2;
    }

    void DoCommute(Commute commute)
    {
        int i = 0;
        ui.DoJump(commute.jumps[0]);
        void Jump() {
            if (i < commute.jumps.Count - 1)
            {
                i++;
                ui.DoJump(commute.jumps[i]);
            }
        }
        PieceUI.jumpCompleted += Jump;
    }

}
