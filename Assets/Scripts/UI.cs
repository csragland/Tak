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
            pieceHeight = GetPieceHeight(objToSpawn);
        }
        else if (placement.piece == PieceType.BLOCKER)
        {
            objToSpawn = blocker;
            this.pieceSpawnHeight = Settings.blockerSpawnHeight;
            pieceHeight = GetPieceHeight(objToSpawn);
        }
        else if (placement.piece == PieceType.CAPSTONE)
        {
            objToSpawn = capstone;
            this.pieceSpawnHeight = Settings.capstoneSpawnHeight;
            pieceHeight = GetPieceHeight(objToSpawn);
        }

        Vector3 spawnPos = tile.transform.position + this.pieceSpawnHeight * Vector3.up;

        GameObject pieceObj = Instantiate(objToSpawn, spawnPos, objToSpawn.transform.rotation);
        pieceObj.name = pieceNum.ToString();
        pieceNum++;

        PieceUI pieceData = pieceObj.GetComponent<PieceUI>();
        pieceData.isBeingSpawned = true;
        pieceData.destination = tile.transform.position + ((Settings.tileDimensions.y + pieceHeight) / 2) * Vector3.up + ((tile.transform.childCount) * pieceHeight) * Vector3.up;
        pieceData.type = placement.piece;
        pieceData.player = placement.player;

        pieceObj.transform.SetParent(tile.transform);
    }

    public IEnumerator DoCommute(Commute commute)
    {
        for (int i = 0; i < commute.jumps.Count; i++)
        {
            Jump jump = commute.jumps[i];
            GameObject tile = Utils.GetUITile(jump.origin);
            GameObject endStack = Utils.GetUITile(jump.destination);
            int newStackIndex = 0;
            float timeToWait = 0;
            bool flatten = false;
            if (i < commute.jumps.Count - 1)
            {
                timeToWait += Settings.jumpCooldown;
            }
            else
            {
                flatten = gameManager.tak.JumpWillFlatten(jump);
            }
            for (int j = jump.cutoff; j < tile.transform.childCount; j++)
            {
                PieceUI piece = tile.transform.GetChild(j).GetComponent<PieceUI>();
                Vector3 endPosition = endStack.transform.position + ((Settings.tileDimensions.y + GetPieceHeight(piece.gameObject)) / 2) * Vector3.up + (newStackIndex * GetPieceHeight(stone) + (endStack.transform.childCount) * this.GetPieceHeight(stone)) * Vector3.up;
                if (flatten)
                {
                    endPosition.y += GetPieceHeight(blocker) - GetPieceHeight(stone);
                    piece.GetComponent<Collider>().isTrigger = true;
                }
                float[] jumpData = Utils.JumpPhysics(piece.transform.position, endPosition);
                if (timeToWait == 0)
                {
                    timeToWait = jumpData[2];
                }
                piece.SetCommute(endPosition, endStack, jumpData);
                newStackIndex++;
            }
            if (i < commute.jumps.Count - 1)
            {
                timeToWait += Settings.jumpCooldown;
            }
            yield return new WaitForSeconds(timeToWait);
        }
    }

    public float GetPieceHeight(GameObject piece)
    {
        PieceType pieceType = piece.GetComponent<PieceUI>().type;
        if (pieceType == PieceType.STONE)
        {
            return stone.transform.localScale.y;
        }
        else if (pieceType == PieceType.BLOCKER)
        {
            return blocker.transform.localScale.y;
        }
        else if (pieceType == PieceType.CAPSTONE)
        {
            return .815f * capstone.transform.localScale.z;
        }
        else
        {
            return 0;
        }
    }

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
