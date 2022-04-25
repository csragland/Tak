using UnityEngine;

public class UI : MonoBehaviour
{
    
    public GameObject gameBoard;

    public float pieceSpawnHeight;

    [SerializeField] private GameObject stone;
    [SerializeField] private GameObject blocker;
    [SerializeField] private GameObject capstone;

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
                tileNum = Utils.IndexToNum(i, j);
                tile.name = "Tile_" + tileNum;
                tile.transform.position = tilePosition;
                tile.transform.localScale = Settings.tileDimensions;
                tile.transform.SetParent(gameBoard.transform);
                tile.AddComponent<cakeslice.Outline>();

                tile.GetComponent<Renderer>().material = Utils.IsEven(tileNum) ? this.black : this.white;

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

        GameObject objToSpawn = this.stone;
        float pieceHeight = 0;
        if (placement.piece == PieceType.STONE)
        {
            objToSpawn = this.stone;
            this.pieceSpawnHeight = Settings.stoneSpawnHeight;
            pieceHeight = objToSpawn.transform.localScale.y;
        }
        else if (placement.piece == PieceType.BLOCKER)
        {
            objToSpawn = this.blocker;
            this.pieceSpawnHeight = Settings.blockerSpawnHeight;
            pieceHeight = objToSpawn.transform.localScale.x;
        }
        else if (placement.piece == PieceType.CAPSTONE)
        {
            objToSpawn = this.capstone;
            this.pieceSpawnHeight = Settings.capstoneSpawnHeight;
            pieceHeight = objToSpawn.transform.localScale.y * 2;
        }

        Vector3 spawnPos = tile.transform.position + this.pieceSpawnHeight * Vector3.up;

        GameObject pieceObj = Instantiate(objToSpawn, spawnPos, objToSpawn.transform.rotation);
        pieceObj.transform.SetParent(tile.transform);

        PieceUI pieceData = pieceObj.GetComponent<PieceUI>();
        pieceData.destination = tile.transform.position + ((Settings.tileDimensions.y + pieceHeight) / 2) * Vector3.up + ((tile.transform.childCount - 1) * pieceHeight) * Vector3.up;
        pieceData.type = placement.piece; pieceData.player = placement.player;
    }

    public void DoCommute(Commute commute)
    {
        foreach (var jump in commute.jumps) 
        {
            GameObject tile = Utils.GetUITile(jump.origin);
            for (int i = 0; i < tile.transform.childCount - jump.cutoff; i++)
            {
                GameObject endStack = Utils.GetUITile(jump.destination);
                Vector3 endPosition = endStack.transform.position + ((Settings.tileDimensions.y + GetPieceHeight(stone)) / 2) * Vector3.up + (i * GetPieceHeight(stone)) * Vector3.up;
                tile.transform.GetChild(i).GetComponent<PieceUI>().SetCommute(endPosition);
            }
   
        }
    }

    public float GetPieceHeight(GameObject piece)
    {
        PieceType pieceType = piece.GetComponent<PieceUI>().type;
        if (pieceType == PieceType.CAPSTONE)
        {
            return this.capstone.transform.localScale.y * 2;
        }
        return this.stone.transform.localScale.y;
    }
}
