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
        ui.DoPlacement(new Placement(1, PieceType.STONE, new Tile(1, 1)));
    }

    // Update is called once per frame
    void Update()
    {

    }

    void PlacePiece()
    {
        ui.DoPlacement(new Placement(1, PieceType.STONE, new Tile(1, 1)));
    }

}
