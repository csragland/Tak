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
        if (placement.piece == PieceType.STONE)
        {
            pieceSpawnHeight = Settings.stoneSpawnHeight;
        }
        else if (placement.piece == PieceType.BLOCKER)
        {
            objToSpawn = blocker;
            pieceSpawnHeight = Settings.blockerSpawnHeight;
        }
        else if (placement.piece == PieceType.CAPSTONE)
        {
            objToSpawn = capstone;
            pieceSpawnHeight = Settings.capstoneSpawnHeight;
        }
        float pieceHeight = GetPieceHeight(objToSpawn);

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

    public IEnumerator UndoPlacement((Placement, PoppingInfo) placementData)
    {
        Placement move = placementData.Item1;
        GameObject piece = Utils.GetUIPiece(move.destination);
        PieceUI pieceData = piece.GetComponent<PieceUI>();
        float originHeight = move.piece == PieceType.STONE ? Settings.stoneSpawnHeight : move.piece == PieceType.BLOCKER ? Settings.blockerSpawnHeight : Settings.capstoneSpawnHeight;
        Vector3 destination = Utils.GetUITile(move.destination).transform.position + originHeight * Vector3.up;
        pieceData.SetSpawn(destination, Utils.GetSpawnTime(move) / 2);
        yield return new WaitForSeconds(Settings.stoneSpawnTime);
        Destroy(piece);
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
                    endPosition.y += GetPieceHeight(PieceType.BLOCKER) - GetPieceHeight(PieceType.STONE);
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

    public void UndoCommute((Commute, PoppingInfo) commuteData)
    {
        Commute commute = commuteData.Item1;
        PoppingInfo info = commuteData.Item2;
        Jump first = commute.jumps[0];
        Jump last = commute.jumps[^1];
        List<Transform> restack = new();
        for (int i = 0; i < commute.jumps.Count; i++)
        {
            Jump jump = commute.jumps[i];
            int next = i == commute.jumps.Count - 1 ? 0 : info.numPieces[i + 1];
            int numPiecesToAdd = info.numPieces[i] - next;
            GameObject targetStack = Utils.GetUITile(jump.destination);
            for (int j = targetStack.transform.childCount - numPiecesToAdd; j < numPiecesToAdd; j++)
            {
                GameObject piece = targetStack.transform.GetChild(j).gameObject;
                PieceUI pieceData = piece.GetComponent<PieceUI>();
                int[] indecies = { jump.origin.row, jump.origin.col, j };
                piece.transform.position = GetLocation(indecies, pieceData.type);
                restack.Add(piece.transform);
            }
        }
        foreach (Transform transform in restack)
        {
            transform.SetParent(Utils.GetUITile(first.origin).transform);
        }
        if (info.doesFlatten)
        {
            GameObject lastCrown = Utils.GetUICrown(last.destination);
            PieceUI pieceData = lastCrown.GetComponent<PieceUI>();
            Piece piece = new(pieceData.type, pieceData.player);
            Destroy(lastCrown);
            int[] indecies = { last.destination.row, last.destination.col, lastCrown.transform.GetSiblingIndex() };
            SetPiece(indecies, piece);
        }
    }

    public void GenerateBoard(List<Piece>[,] board)
    {
        for (int i = 0; i < Settings.dimension; i++)
        {
            for (int j = 0; j < Settings.dimension; j++)
            {
                for (int k = 0; k < board[i, j].Count; k++)
                {
                    Piece piece = board[i, j][k];
                    int[] indecies = { i, j, k };
                    SetPiece(indecies, piece);
                }
            }
        }
    }

    public void SetPiece(int[] indecies, Piece piece)
    {
        GameObject gameObject = GetPiece(piece.type);
        Vector3 location = GetLocation(indecies, piece.type);
        GameObject pieceObj = Instantiate(gameObject, location, gameObject.transform.rotation);

        PieceUI pieceData = pieceObj.GetComponent<PieceUI>();
        pieceData.type = piece.type;
        pieceData.player = piece.player;

        pieceObj.transform.SetParent(Utils.GetUITile(Utils.IndexToNum(indecies[0], indecies[1])).transform);
    }

    public Vector3 GetLocation(Tile myTile, int stackIndex, PieceType type)
    {
        float x = Settings.location.x + (myTile.col - Mathf.Floor(Settings.dimension / 2)) * Settings.tileDimensions.x;
        float z = Settings.location.z + (-myTile.row + Mathf.Floor(Settings.dimension / 2)) * Settings.tileDimensions.z;
        return new Vector3(x, Settings.location.y, z) + ((stackIndex * GetPieceHeight(PieceType.STONE)) + (Settings.tileDimensions.y + GetPieceHeight(type)) / 2) * Vector3.up;
    }

    public Vector3 GetLocation(int[] indecies, PieceType type)
    {
        float x = Settings.location.x + (-Mathf.Floor(Settings.dimension / 2) + indecies[1]) * Settings.tileDimensions.x;
        float z = Settings.location.z + (Mathf.Floor(Settings.dimension / 2) - indecies[0]) * Settings.tileDimensions.z;
        return new Vector3(x, Settings.location.y, z) + ((indecies[2] * GetPieceHeight(PieceType.STONE)) + (Settings.tileDimensions.y + GetPieceHeight(type)) / 2) * Vector3.up;
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

    public float GetPieceHeight(PieceType pieceType)
    {
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

    private GameObject GetPiece(PieceType type)
    {
        return type == PieceType.STONE ? stone : type == PieceType.BLOCKER ? blocker : capstone;
    }
}
