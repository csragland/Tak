using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class UI : MonoBehaviour
{

    public GameObject gameBoard;
    public Vector3 boardLocation = Vector3.zero;

    public Vector3 tileDimensions = new Vector3(1, .25f, 1);

    public int pieceSpawnHeight = 6;

    public GameObject stone;
    public GameObject capstone;

    public Material black;
    public Material white;

    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void InitalizeBoard(int dimension)
    {
        Debug.Assert(!Utils.IsEven(dimension));
        this.gameBoard = new GameObject("Board");
        Vector3 startPoint = this.boardLocation + new Vector3(-Mathf.Floor(dimension / 2) * tileDimensions.x, 0, Mathf.Floor(dimension / 2) * tileDimensions.z);
        Vector3 tilePosition = startPoint;
        int tileNum;
        for (int i = 0; i < dimension; i++)
        {
            for (int j = 0; j < dimension; j++)
            {
                GameObject tile = GameObject.CreatePrimitive(PrimitiveType.Cube);
                tileNum = Utils.IndexToNum(i, j, dimension);
                tile.name = "Tile_" + tileNum;
                tile.transform.position = tilePosition;
                tile.transform.localScale = tileDimensions;
                tile.transform.SetParent(gameBoard.transform);

                tile.GetComponent<Renderer>().material = Utils.IsEven(tileNum) ? this.black : this.white;

                tilePosition.x += tileDimensions.x;
            }
            tilePosition.z -= tileDimensions.z;
            tilePosition.x = startPoint.x;
        }
    }

    public void DoPlacement(Placement placement)
    {
        int row = placement.destination.row;
        int col = placement.destination.col;

        int tileNum = Utils.IndexToNum(row, col, Settings.dimension);
        GameObject tile = GameObject.Find("Board/Tile_" + tileNum);

        GameObject objToSpawn = placement.piece == PieceType.CAPSTONE ? this.capstone : this.stone;
        Vector3 spawnPos = tile.transform.position + this.pieceSpawnHeight * Vector3.up;
        Quaternion spawnRotation = placement.piece == PieceType.BLOCKER ? PieceUI.type2Rotation : objToSpawn.transform.rotation;
        float pieceHeight = placement.piece == PieceType.BLOCKER ? objToSpawn.transform.localScale.x : objToSpawn.transform.localScale.y;
        if (placement.piece == PieceType.CAPSTONE)
        {
            // Cylinders' height is double the scale
            pieceHeight *= 2;
        }

        GameObject pieceObj = Instantiate(objToSpawn, spawnPos, spawnRotation);
        pieceObj.transform.SetParent(tile.transform);

        PieceUI pieceData = pieceObj.GetComponent<PieceUI>();
        pieceData.destination = tile.transform.position + ((tileDimensions.y + pieceHeight) / 2) * Vector3.up;
        pieceData.type = placement.piece; pieceData.player = placement.player;
    }

}
