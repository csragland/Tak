using UnityEngine;

public class UI : MonoBehaviour
{

    public GameObject gameBoard;

    public float pieceSpawnHeight;

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
        Vector3 startPoint = Settings.location + new Vector3(-Mathf.Floor(dimension / 2) * Settings.tileDimensions.x, 0, Mathf.Floor(dimension / 2) * Settings.tileDimensions.z);
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
                tile.transform.localScale = Settings.tileDimensions;
                tile.transform.SetParent(gameBoard.transform);

                tile.GetComponent<Renderer>().material = Utils.IsEven(tileNum) ? this.black : this.white;

                tilePosition.x += Settings.tileDimensions.x;
            }
            tilePosition.z -= Settings.tileDimensions.z;
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

        Quaternion spawnRotation = Quaternion.Euler(0, 0, 0);
        float pieceHeight = 0;
        if (placement.piece == PieceType.STONE)
        {
            this.pieceSpawnHeight = Settings.stoneSpawnHeight;
            spawnRotation = objToSpawn.transform.rotation;
            pieceHeight = objToSpawn.transform.localScale.y;
        }
        else if (placement.piece == PieceType.BLOCKER)
        {
            this.pieceSpawnHeight = Settings.blockerSpawnHeight;
            spawnRotation = PieceUI.type2Rotation;
            pieceHeight = objToSpawn.transform.localScale.x;
        }
        else if (placement.piece == PieceType.CAPSTONE)
        {
            this.pieceSpawnHeight = Settings.capstoneSpawnHeight;
            spawnRotation = objToSpawn.transform.rotation;
            pieceHeight = objToSpawn.transform.localScale.y * 2;
        }

        Vector3 spawnPos = tile.transform.position + this.pieceSpawnHeight * Vector3.up;

        GameObject pieceObj = Instantiate(objToSpawn, spawnPos, spawnRotation);
        pieceObj.transform.SetParent(tile.transform);

        PieceUI pieceData = pieceObj.GetComponent<PieceUI>();
        pieceData.destination = tile.transform.position + ((Settings.tileDimensions.y + pieceHeight) / 2) * Vector3.up + (tile.transform.childCount * pieceHeight) * Vector3.up;
        pieceData.type = placement.piece; pieceData.player = placement.player;
    }

}
