using UnityEngine;
using System.Collections;
using TMPro;
using UnityEngine.UI;
//using System;

public class UI : MonoBehaviour
{
    public GameManager gameManager;
    
    public GameObject gameBoard;

    public float pieceSpawnHeight;
    public float pieceNum = 0;

    public GameObject stone;
    public GameObject blocker;
    public GameObject capstone;

    public Material black;
    public Material white;

    public Button startButton;
    public TextMeshProUGUI playerText;

    // Start is called before the first frame update
    private void Start()
    {
        gameManager = GameObject.Find("Game Manager").GetComponent<GameManager>();
        this.playerText.enabled = false;
        startButton = GameObject.Find("Canvas/Title Screen/Start Button").GetComponent<Button>();
        startButton.onClick.AddListener(gameManager.StartGame);
    }

    public void InitalizeBoard(int dimension)
    {
        Debug.Assert(!Utils.IsEven(dimension));
        this.gameBoard = new GameObject("Board");
        Vector3 startPoint = Settings.location + new Vector3(-Mathf.Floor(dimension / 2) * Settings.tileDimensions.x, 0, Mathf.Floor(dimension / 2) * Settings.tileDimensions.z);
        Vector3 tilePosition = startPoint;
        int tileNum;
        for (int i = 0; i < dimension; i++)
        {
            for (int j = 0; j < dimension; j++)
            {
                GameObject tile = GameObject.CreatePrimitive(PrimitiveType.Cube);
                tileNum = Utils.IndexToNum(i, j);
                tile.name = "Tile_" + tileNum;
                tile.transform.position = tilePosition;
                tile.transform.localScale = Settings.tileDimensions;
                tile.transform.SetParent(gameBoard.transform);
                tile.AddComponent<cakeslice.Outline>().enabled = false;

                tile.GetComponent<Renderer>().material = Utils.IsEven(tileNum) ? black : white;

                tilePosition.x += Settings.tileDimensions.x;
            }
            tilePosition.z -= Settings.tileDimensions.z;
            tilePosition.x = startPoint.x;
        }
        Rigidbody rb = gameBoard.AddComponent<Rigidbody>();
        rb.isKinematic = true;
    }

    public void DoPlacement(Placement placement)
    {
        int row = placement.destination.row;
        int col = placement.destination.col;

        int tileNum = Utils.IndexToNum(row, col);
        GameObject tile = GameObject.Find("Board/Tile_" + tileNum);

        GameObject objToSpawn = stone;
        float pieceHeight = 0;
        if (placement.piece == PieceType.STONE)
        {
            objToSpawn = stone;
            this.pieceSpawnHeight = Settings.stoneSpawnHeight;
            pieceHeight = objToSpawn.transform.localScale.y;
        }
        else if (placement.piece == PieceType.BLOCKER)
        {
            objToSpawn = blocker;
            this.pieceSpawnHeight = Settings.blockerSpawnHeight;
            pieceHeight = objToSpawn.transform.localScale.x;
        }
        else if (placement.piece == PieceType.CAPSTONE)
        {
            objToSpawn = capstone;
            this.pieceSpawnHeight = Settings.capstoneSpawnHeight;
            pieceHeight = objToSpawn.transform.localScale.y * 2;
        }

        Vector3 spawnPos = tile.transform.position + this.pieceSpawnHeight * Vector3.up;

        GameObject pieceObj = Instantiate(objToSpawn, spawnPos, objToSpawn.transform.rotation);
        pieceObj.name = pieceNum.ToString();
        pieceNum++;
        pieceObj.transform.SetParent(tile.transform);

        PieceUI pieceData = pieceObj.GetComponent<PieceUI>();
        pieceData.destination = tile.transform.position + ((Settings.tileDimensions.y + pieceHeight) / 2) * Vector3.up + ((tile.transform.childCount) * pieceHeight) * Vector3.up;
        pieceData.type = placement.piece; pieceData.player = placement.player;
    }

    public void DoCommute(Commute commute)
    {
        StartCoroutine(StartCommute(commute));
    }

    public IEnumerator StartCommute(Commute commute)
    {
        foreach (var jump in commute.jumps)
        {
            GameObject tile = Utils.GetUITile(jump.origin);
            GameObject endStack = Utils.GetUITile(jump.destination);
            for (int i = jump.cutoff; i < tile.transform.childCount; i++)
            {
                PieceUI piece = tile.transform.GetChild(i).GetComponent<PieceUI>();
                //Debug.Log((endStack.transform.childCount) * this.GetPieceHeight(stone));
                Vector3 endPosition = endStack.transform.position + ((Settings.tileDimensions.y + GetPieceHeight(piece.gameObject)) / 2) * Vector3.up + (i * GetPieceHeight(stone) + (endStack.transform.childCount) * this.GetPieceHeight(stone)) * Vector3.up;
                //Debug.Log(endPosition);
                piece.SetCommute(endPosition, endStack);
            }
            //Func<bool> Done = new Func<bool>(() => JumpIsDone(jump));
            //yield return new WaitUntil(Done);
            //Func<bool> M = new Func<bool>(() => Migrated(jump));
            //yield return new WaitUntil(M);
            yield return new WaitForSeconds(2);
        }
    }

    public float GetPieceHeight(GameObject piece)
    {
        PieceType pieceType = piece.GetComponent<PieceUI>().type;
        if (pieceType == PieceType.CAPSTONE)
        {
            return capstone.transform.localScale.y * 2;
        }
        return stone.transform.localScale.y;
    }

    //private bool JumpIsDone(Jump jump)
    //{
    //    GameObject tile = Utils.GetUITile(jump.origin);
    //    for (int i = jump.cutoff; i < tile.transform.childCount; i++)
    //    {
    //        //GameObject endStack = Utils.GetUITile(jump.destination);
    //        PieceUI pieceData = tile.transform.GetChild(i).GetComponent<PieceUI>();
    //        if (pieceData.isCommuting)
    //        {
    //            Debug.Log("Still commuting");
    //            return false;
    //        }
    //    }
    //    return true;
    //}

    //private bool Migrated(Jump jump)
    //{
    //    GameObject tile = Utils.GetUITile(jump.destination);
    //    for (int i = jump.cutoff; i < tile.transform.childCount; i++)
    //    {
    //        //GameObject endStack = Utils.GetUITile(jump.destination);
    //        PieceUI pieceData = tile.transform.GetChild(i).GetComponent<PieceUI>();
    //        if (!pieceData.transform.IsChildOf(tile.transform))
    //        {
    //            Debug.Log("Not migrated");
    //            return false;
    //        }
    //    }
    //    return true;
    //}
}
