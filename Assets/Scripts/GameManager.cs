using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public Tak tak;

    public UI ui;

    public bool gameOver = false;

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
    }

    // Update is called once per frame
    void Update()
    {

    }

    void StackItUp()
    {
        int pieces = 7;
        float time = 1.5f;
        for (int i = 0; i < pieces; i++)
        {
            Invoke("PlacePiece", time * i);
        }
    }

    void PlacePiece()
    {
        ui.DoPlacement(new Placement(1, PieceType.STONE, new Tile(1, 1)));
        tak.DoPlacement(new Placement(1, PieceType.STONE, new Tile(1, 1)));
    }

}
