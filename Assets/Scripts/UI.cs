using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine.UI;

public class UI : MonoBehaviour
{
    public GameManager gameManager;
    
    public GameObject gameBoard;

    public GameObject stone;
    public GameObject blocker;
    public GameObject capstone;

    public Material blackTile;
    public Material whiteTile;
    public Material blackPiece;
    public Material whitePiece;

    public Button startButton;
    public TextMeshProUGUI playerText;
    public TextMeshProUGUI victoryText;

    private float pieceNum = 0;

    // Start is called before the first frame update
    private void Start()
    {
        gameManager = GameObject.Find("Game Manager").GetComponent<GameManager>();
        this.playerText.enabled = false;
        this.victoryText.enabled = false;
        startButton = GameObject.Find("Canvas/Title Screen/Start Button").GetComponent<Button>();
        startButton.onClick.AddListener(gameManager.StartGame);
    }

    // Generate game board of variable size in predetermined location
    public void InitalizeBoard(int dimension)
    {
        Debug.Assert(!Utils.IsEven(dimension));
        this.gameBoard = new GameObject("Board");
        Vector3 startPoint = Settings.location + new Vector3(-Mathf.Floor(dimension / 2) * Settings.tileDimensions.x, 0, Mathf.Floor(dimension / 2) * Settings.tileDimensions.z);
        Vector3 tilePosition = startPoint;
        int tileNum;
        // Instaniate tiles while setting transform and properties
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

                tile.GetComponent<Renderer>().material = Utils.IsEven(tileNum) ? blackTile : whiteTile;

                tilePosition.x += Settings.tileDimensions.x;
            }
            tilePosition.z -= Settings.tileDimensions.z;
            tilePosition.x = startPoint.x;
        }
    }

    // Execute placement move in UI
    public IEnumerator DoPlacement(Placement placement)
    {
        int row = placement.destination.row;
        int col = placement.destination.col;

        int tileNum = Utils.IndexToNum(row, col);
        GameObject tile = GameObject.Find("Board/Tile_" + tileNum);

        GameObject objToSpawn = stone;
        float pieceSpawnHeight = 0;
        float pieceHeight = 0;
        if (placement.piece == PieceType.STONE)
        {
            objToSpawn = stone;
            pieceSpawnHeight = Settings.stoneSpawnHeight;
            pieceHeight = GetPieceHeight(objToSpawn);
        }
        else if (placement.piece == PieceType.BLOCKER)
        {
            objToSpawn = blocker;
            pieceSpawnHeight = Settings.blockerSpawnHeight;
            pieceHeight = GetPieceHeight(objToSpawn);
        }
        else if (placement.piece == PieceType.CAPSTONE)
        {
            objToSpawn = capstone;
            pieceSpawnHeight = Settings.capstoneSpawnHeight;
            pieceHeight = GetPieceHeight(objToSpawn);
        }

        Vector3 spawnPos = tile.transform.position + pieceSpawnHeight * Vector3.up;

        GameObject pieceObj = Instantiate(objToSpawn, spawnPos, objToSpawn.transform.rotation);
        pieceObj.name = pieceNum.ToString();
        pieceNum++;

        PieceUI pieceData = pieceObj.GetComponent<PieceUI>();
        Vector3 destination = tile.transform.position + ((Settings.tileDimensions.y + pieceHeight) / 2) * Vector3.up + ((tile.transform.childCount) * pieceHeight) * Vector3.up;
        pieceData.SetSpawn(destination);
        pieceData.type = placement.piece;
        pieceData.player = placement.player;

        pieceObj.transform.SetParent(tile.transform);

        yield return new WaitForSeconds(Utils.GetSpawnTime(placement) + Settings.spawnCooldown);
    }

    // Execute Commute move in UI
    public IEnumerator DoCommute(Commute commute)
    {
        bool flatten = gameManager.tak.CommuteWillFlatten(commute);
        for (int i = 0; i < commute.jumps.Count; i++)
        {
            Jump jump = commute.jumps[i];
            GameObject tile = Utils.GetUITile(jump.origin);
            GameObject endStack = Utils.GetUITile(jump.destination);
            int newStackIndex = 0;
            float timeToWait = 0;
            bool baseTimeSet = false;
            List<GameObject> jumpers = new();
            if (i < commute.jumps.Count - 1)
            {
                timeToWait += Settings.jumpCooldown;
            }
            for (int j = jump.cutoff; j < tile.transform.childCount; j++)
            {
                GameObject pieceObj = tile.transform.GetChild(j).gameObject;
                jumpers.Add(pieceObj);
                PieceUI piece = pieceObj.GetComponent<PieceUI>();
                Vector3 endPosition = endStack.transform.position + ((Settings.tileDimensions.y + GetPieceHeight(piece.gameObject)) / 2) * Vector3.up + (newStackIndex * GetPieceHeight(stone) + (endStack.transform.childCount) * this.GetPieceHeight(stone)) * Vector3.up;
                if (flatten && i == commute.jumps.Count - 1)
                {
                    endPosition.y += GetPieceHeight(blocker) - GetPieceHeight(stone);
                    piece.GetComponent<Collider>().isTrigger = true;
                    timeToWait += Settings.flattenTime;
                }
                float[] jumpData = Utils.JumpPhysics(piece.transform.position, endPosition);
                if (!baseTimeSet)
                {
                    timeToWait += jumpData[2];
                    baseTimeSet = true;
                }
                piece.SetCommute(endPosition, jumpData);
                newStackIndex++;
            }
            yield return new WaitForSeconds(timeToWait);
            if (flatten)
            {
                jumpers[0].GetComponent<Animator>().enabled = false;
                GameObject temp = GameObject.Find("temp");
                Destroy(temp);
            }
            foreach (var jumper in jumpers)
            {
                jumper.transform.SetParent(endStack.transform);
            }
        }
    }

    // Return in-game height of a certain type of piece
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
}
