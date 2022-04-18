using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{

    public GameObject gameBoard;
    public int boardDimension = 5;
    public List<Piece>[,] boardData;
    private Vector3 boardLocation = Vector3.zero;

    public int pieceSpawnHeight = 6;
    private Vector3 tileDimensions = new Vector3(1, .25f, 1);

    public GameObject stone;
    public GameObject capstone;

    public Material black;
    public Material white;


    // Start is called before the first frame update
    void Start()
    {
        this.boardData = new List<Piece>[boardDimension,boardDimension];
        for (int i = 0; i < boardDimension; i++)
        {
            for (int j = 0; j < boardDimension; j++)
            {
                boardData[i, j] = new List<Piece>();
            }
        }
        this.InitalizeBoard(boardDimension);
        placePiece(1, 1, 1, 1);
        Invoke("ppContainer", 2);
        Invoke("ppContainer2", 4);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void InitalizeBoard(int dimension)
    {
        Debug.Assert(dimension % 2 == 1);
        this.gameBoard = new GameObject("Board");
        Vector3 startPoint = this.boardLocation + new Vector3(-Mathf.Floor(dimension / 2)*tileDimensions.x, 0, Mathf.Floor(dimension / 2)*tileDimensions.z);
        Vector3 tilePosition = startPoint;
        int tileNum;
        for (int i = 0; i < dimension; i++)
        {
            for (int j = 0; j < dimension; j++)
            {
                GameObject tile = GameObject.CreatePrimitive(PrimitiveType.Cube);
                tileNum = IndexToNum(i, j, dimension);
                tile.name = "Tile_" + tileNum;
                tile.transform.position = tilePosition;
                tile.transform.localScale = tileDimensions;
                tile.transform.SetParent(gameBoard.transform);

                tile.GetComponent<Renderer>().material = IsEven(tileNum) ? this.black : this.white;

                tilePosition.z -= tileDimensions.z;
            }
            tilePosition.x += tileDimensions.x;
            tilePosition.z = startPoint.z;
        }
    }

    void placePiece(int type, int player, int i, int j)
    {
        int tileNum = i * this.boardDimension + j;
        Vector3 tilePos = gameBoard.transform.Find("Tile_" + tileNum).position;


        GameObject objToSpawn = type == 3 ? this.capstone : this.stone;
        Vector3 spawnPos = tilePos + 3 * Vector3.up;
        Quaternion spawnRotation = type == 2 ? Piece.type2Rotation : objToSpawn.transform.rotation;
        float pieceHeight = type == 2 ? objToSpawn.transform.localScale.x : objToSpawn.transform.localScale.y;
        if (type == 3)
        {
            // Cylinders' height is double the scale
            pieceHeight *= 2;
        }


        GameObject pieceObj = Instantiate(objToSpawn, spawnPos, spawnRotation);

        Piece pieceData = pieceObj.GetComponent<Piece>();
        pieceData.destination = tilePos + ((tileDimensions.y + pieceHeight) / 2) * Vector3.up + (this.boardData[i, j].Count * tileDimensions.y) * Vector3.up;
        pieceData.type = type; pieceData.player = player;
        this.boardData[i,j].Add(pieceData);
    }

    void ppContainer()
    {
        placePiece(2, 1, 2, 2);
    }

    void ppContainer2()
    {
        placePiece(3, 1, 3, 3);
    }

    bool IsEven(int num)
    {
        return num % 2 == 0;
    }

    int IndexToNum(int rowIndex, int colIndex, int dim)
    {
        return rowIndex * dim + colIndex;
    }
}
