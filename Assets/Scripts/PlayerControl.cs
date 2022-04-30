using UnityEngine;
using System.Collections.Generic;

public class PlayerControl : MonoBehaviour
{
    public GameManager gameManager;

    float horizontalInput;
    float verticalInput;

    public int currentTileId;

    bool initialized = false;

    public int player;

    Quaternion snapStart;
    Quaternion snapEnd;
    float t1;
    float t2;

    Vector3 startZoom;
    bool zoomPrepared;
    float startDist;
    float currentDist;
    Vector3 zoomDestination;

    Quaternion startRotation;
    Quaternion endRotation;
    bool rotationPrepared;

    public bool isBoarding = true;
    List<GameObject> commuters = new List<GameObject>();

    // Start is called before the first frame update
    void Start()
    {
        Camera.main.transform.position = Settings.cameraOffset;
        player = GameManager.currentPlayer;
        gameManager = GameObject.Find("Game Manager").GetComponent<GameManager>();
        snapEnd = Quaternion.Euler(0, 0, 0);
        
    }

    // Update is called once per frame
    void LateUpdate()
    {
        if (!this.initialized)
        {
            currentTileId = (int)Mathf.Floor(Mathf.Pow(Settings.dimension, 2) / 2);
            GameObject centerTile = GameObject.Find("Board/Tile_" + currentTileId);
            centerTile.GetComponent<cakeslice.Outline>().enabled = true;
            this.transform.position = centerTile.transform.position;
            this.initialized = true;
        }
        if (  Input.GetKey(KeyCode.A) && (!Settings.splitBoardView || (player == 1 && CompareRotations(Comparisons.LTE, 90)) || (player == 2 && (CompareRotations(Comparisons.LTE, -90) || CompareRotations(Comparisons.GT, 0))))  )
        {
            horizontalInput = 1;
            snapStart = transform.rotation;
        }
        else if (Input.GetKey(KeyCode.D) && (!Settings.splitBoardView || (player == 1 && CompareRotations(Comparisons.GTE, -90)) || (player == 2 && (CompareRotations(Comparisons.GTE, 90) || CompareRotations(Comparisons.LT, 0)))))
        {
            horizontalInput = -1;
            snapStart = transform.rotation;
        }
        else
        {
            horizontalInput = 0;
        }

        if (Input.GetKey(KeyCode.W) && Vector3.Distance(Camera.main.transform.position, this.transform.position) > 1)
        {
            verticalInput = 1;
        }
        else if (Input.GetKey(KeyCode.S) && Vector3.Distance(Camera.main.transform.position, this.transform.position) < 10)
        {
            verticalInput = -1;
        }
        else
        {
            verticalInput = 0;
        }

        if (player == GameManager.currentPlayer)
        {
            if (horizontalInput == 0 && ((transform.rotation.y != 0 && player == 1 && CompareRotations(Comparisons.LTE, Settings.cameraSnapRadius, true)) || (transform.eulerAngles.y != 180 && player == 2 && CompareRotations(Comparisons.GTE, Mathf.Abs(MapAngle(this.snapEnd.eulerAngles.y)) - Settings.cameraSnapRadius, true))))
            {
                t1 += Time.deltaTime / Settings.cameraSnapSpeed;
                this.transform.rotation = Quaternion.Lerp(snapStart, snapEnd, t1);
            }
            transform.Rotate(Settings.cameraRotateSpeed * horizontalInput * Vector3.up);
            Camera.main.transform.Translate(Settings.cameraZoomSpeed * verticalInput * Vector3.forward);
        }
        else
        {
            this.SwapPlayer(GameManager.currentPlayer);
        }
        

    }

    private void OnGUI()
    {
        Event e = Event.current;
        if (e.type == EventType.KeyDown && player == GameManager.currentPlayer)
        {

            this.CheckArrowOver(e);

            if (e.keyCode == KeyCode.Alpha1 && gameManager.tak.IsLegalMove(new Placement(player, PieceType.STONE, Utils.NumToTile(this.currentTileId))))
            {
                gameManager.DoPlacement(new Placement(player, PieceType.STONE, Utils.NumToTile(this.currentTileId)));
            }
            else if (e.keyCode == KeyCode.Alpha2 && gameManager.tak.IsLegalMove(new Placement(player, PieceType.BLOCKER, Utils.NumToTile(this.currentTileId))))
            {
                gameManager.DoPlacement(new Placement(player, PieceType.BLOCKER, Utils.NumToTile(this.currentTileId)));
            }
            else if (e.keyCode == KeyCode.Alpha3 && gameManager.tak.IsLegalMove(new Placement(player, PieceType.CAPSTONE, Utils.NumToTile(this.currentTileId))))
            {
                gameManager.DoPlacement(new Placement(player, PieceType.CAPSTONE, Utils.NumToTile(this.currentTileId)));
            }

            else if (e.keyCode == KeyCode.Space)
            {
                Commute commute = this.BuildCommute(Utils.NumToTile(this.currentTileId));
                if (gameManager.tak.IsLegalMove(commute))
                {
                    gameManager.DoCommute(commute);
                }
                this.commuters.Clear();
            }
        }
    }

    private void CheckArrowOver(Event e)
    {
        // I can maybe make a function to map between quadrants and buttons (with matrices?)
        if (((e.keyCode == KeyCode.RightArrow && CompareRotations(Comparisons.LTE, 45, true))
            || (e.keyCode == KeyCode.UpArrow && CompareRotations(Comparisons.GT, 45) && CompareRotations(Comparisons.LT, 135))
            || (e.keyCode == KeyCode.DownArrow && CompareRotations(Comparisons.LT, -45) && CompareRotations(Comparisons.GT, -135))
            || (e.keyCode == KeyCode.LeftArrow && CompareRotations(Comparisons.GTE, 135, true)))
            && !(currentTileId % Settings.dimension == Settings.dimension - 1))
        {
            this.Unfocus(currentTileId);
            currentTileId++;
            this.SetFocus(currentTileId);
        }
        if (((e.keyCode == KeyCode.LeftArrow && CompareRotations(Comparisons.LTE, 45, true))
            || (e.keyCode == KeyCode.DownArrow && CompareRotations(Comparisons.GT, 45) && CompareRotations(Comparisons.LT, 135))
            || (e.keyCode == KeyCode.UpArrow && CompareRotations(Comparisons.LT, -45) && CompareRotations(Comparisons.GT, -135))
            || (e.keyCode == KeyCode.RightArrow && CompareRotations(Comparisons.GTE, 135, true)))
            && !(currentTileId % Settings.dimension == 0))
        {
            this.Unfocus(currentTileId);
            currentTileId--;
            this.SetFocus(currentTileId);
        }
        if (((e.keyCode == KeyCode.UpArrow && CompareRotations(Comparisons.LTE, 45, true))
            || (e.keyCode == KeyCode.LeftArrow && CompareRotations(Comparisons.GT, 45) && CompareRotations(Comparisons.LT, 135))
            || (e.keyCode == KeyCode.RightArrow && CompareRotations(Comparisons.LT, -45) && CompareRotations(Comparisons.GT, -135))
            || (e.keyCode == KeyCode.DownArrow && CompareRotations(Comparisons.GTE, 135, true)))
            && !(currentTileId < Settings.dimension))
        {
            this.Unfocus(currentTileId);
            currentTileId -= Settings.dimension;
            this.SetFocus(currentTileId);
        }
        if (((e.keyCode == KeyCode.DownArrow && CompareRotations(Comparisons.LTE, 45, true))
            || (e.keyCode == KeyCode.RightArrow && CompareRotations(Comparisons.GT, 45) && CompareRotations(Comparisons.LT, 135))
            || (e.keyCode == KeyCode.LeftArrow && CompareRotations(Comparisons.LT, -45) && CompareRotations(Comparisons.GT, -135))
            || (e.keyCode == KeyCode.UpArrow && CompareRotations(Comparisons.GTE, 135, true)))
            && !(currentTileId >= Settings.dimension * (Settings.dimension - 1)))
        {
            this.Unfocus(currentTileId);
            currentTileId += Settings.dimension;
            this.SetFocus(currentTileId);
        }
    }

    private void SetFocus(int tileId)
    {
        GameObject tile = GameObject.Find("Board/Tile_" + tileId);
        int numChildren = tile.transform.childCount;
        if (numChildren > 0)
        {
            GameObject crown = tile.transform.GetChild(numChildren - 1).gameObject;
            this.transform.position = crown.transform.position;
            if (crown.GetComponent<PieceUI>().player == GameManager.currentPlayer)
            {
                for (int i = 0; i < numChildren; i++)
                {
                    tile.transform.GetChild(i).gameObject.GetComponent<cakeslice.Outline>().enabled = true;
                }
            }
        }
        else
        {
            this.transform.position = tile.transform.position;
            tile.GetComponent<cakeslice.Outline>().enabled = true;
        }
    }

    private void Unfocus(int tileId)
    {
        GameObject tile = GameObject.Find("Board/Tile_" + tileId);
        int numChildren = tile.transform.childCount;
        if (numChildren > 0)
        {
            for (int i = 0; i < numChildren; i++)
            {
                tile.transform.GetChild(i).gameObject.GetComponent<cakeslice.Outline>().enabled = false;
            }
        }
        tile.GetComponent<cakeslice.Outline>().enabled = false;
    }

    private void SwapPlayer(int player)
    {
        Debug.Assert(Vector3.Magnitude(Settings.cameraOffset) % 1f == 0);
        if (!zoomPrepared)
        {
            t1 = 0;
            startDist = Vector3.Distance(Settings.cameraOffset, Settings.location);
            currentDist = (Vector3.Distance(Camera.main.transform.position, this.transform.position));
            startZoom = Camera.main.transform.position;
            zoomDestination = Camera.main.transform.position + (currentDist - startDist) * Camera.main.transform.forward;
            zoomPrepared = true;
        }
        if (!rotationPrepared)
        {
            t2 = 0;
            startRotation = this.transform.rotation;
            endRotation = Quaternion.Euler(0, (360 / player) % 360, 0);
            rotationPrepared = true;
        }

        if (Mathf.Round(Vector3.Distance(Camera.main.transform.position, this.transform.position)) != Mathf.Round(startDist))
        {
            t1 += Time.deltaTime / Settings.cameraTransitionTime;
            Camera.main.transform.position = Vector3.Lerp(startZoom, zoomDestination, t1);
        }
        else if (this.transform.rotation.eulerAngles.y != endRotation.eulerAngles.y)
        {
            t2 += Time.deltaTime / Settings.cameraTransitionTime;
            this.transform.rotation = Quaternion.Lerp(startRotation, endRotation, t2);
        }
        else
        {
            zoomPrepared = false;
            rotationPrepared = false;
            this.commuters.Clear();
            this.player = GameManager.currentPlayer;
            snapEnd = player == 1 ? Quaternion.Euler(0, 0, 0) : Quaternion.Euler(0, -180, 0);
        }
    }

    public void ProcessClick(GameObject clicked)
    {
        Transform parent = clicked.transform.parent;
        if (this.isBoarding && clicked.transform.IsChildOf(Utils.GetUITile(Utils.NumToTile(currentTileId)).transform) && this.IsPlayerContolled(parent.gameObject))
        {
            if (this.commuters.Count == 0 || (clicked.transform.IsChildOf(commuters[commuters.Count - 1].transform.parent) && commuters.Count <= Settings.dimension) && !this.commuters.Contains(clicked))
            {
                commuters.Add(clicked);
                for (int i = 0; i <= clicked.transform.GetSiblingIndex(); i++)
                {
                    parent.GetChild(i).GetComponent<cakeslice.Outline>().enabled = false;
                }
            }
            else if (this.commuters.Contains(clicked))
            {
                int index = this.commuters.IndexOf(clicked);
                int stackIndex = parent.childCount - 1;
                int bottomIndex = index < 1 ? 0 : commuters[index - 1].transform.GetSiblingIndex();
                do
                {
                    parent.GetChild(stackIndex).GetComponent<cakeslice.Outline>().enabled = true;
                    stackIndex--;

                } while (stackIndex >= bottomIndex);

                for (int i = this.commuters.Count - 1; i >= index; i--)
                {
                    this.commuters.RemoveAt(i);
                }
            }
        }
    }

    private bool CompareRotations(Comparisons operation, float rotation, bool abs = false)
    {
        if (operation == Comparisons.LT)
        {
            if (!abs)
            {
                return MapAngle(this.transform.eulerAngles.y) < rotation;

            }
            else
            {
                return Mathf.Abs(MapAngle(this.transform.eulerAngles.y)) < rotation;
            }
        }
        if (operation == Comparisons.LTE)
        {
            if (!abs)
            {
                return MapAngle(this.transform.eulerAngles.y) <= rotation;

            }
            else
            {
                return Mathf.Abs(MapAngle(this.transform.eulerAngles.y)) <= rotation;
            }
        }
        if (operation == Comparisons.GTE)
        {
            if (!abs)
            {
                return MapAngle(this.transform.eulerAngles.y) >= rotation;

            }
            else
            {
                return Mathf.Abs(MapAngle(this.transform.eulerAngles.y)) >= rotation;
            }
        }
        if (operation == Comparisons.GT)
        {
            if (!abs)
            {
                return MapAngle(this.transform.eulerAngles.y) > rotation;

            }
            else
            {
                return Mathf.Abs(MapAngle(this.transform.eulerAngles.y)) > rotation;
            }
        }
        return false;
    }

    private float MapAngle(float angle)
    {
        if (angle >= 180)
        {
            return angle - 360;
        }
        return angle;
    }

    private bool IsPlayerContolled(GameObject tile)
    {
        int numChildren = tile.transform.childCount;
        if (tile.transform.childCount > 0)
        {
            GameObject crown = tile.transform.GetChild(numChildren - 1).gameObject;
            this.transform.position = crown.transform.position;
            return crown.GetComponent<PieceUI>().player == GameManager.currentPlayer;
        }
        return false;
    }

    private Commute BuildCommute(Tile end)
    {
        List<Jump> jumps = new List<Jump>();
        Tile start = Utils.NumToTile(this.commuters[0].transform.parent.GetSiblingIndex());
        int[] direction = new int[] { (end.row - start.row) / this.commuters.Count, (end.col - start.col) / this.commuters.Count };
        Tile startTile = start;
        int baseAdjustment = 0;
        for (int i = 1; i <= this.commuters.Count; i++)
        {
            int baseIndex = this.commuters[i - 1].transform.GetSiblingIndex() - baseAdjustment;
            //Debug.Log(baseIndex);
            baseAdjustment = this.commuters[i - 1].transform.GetSiblingIndex();
            Tile endTile = new Tile(start.row + direction[0] * i, start.col + direction[1] * i);
            jumps.Add(new Jump(baseIndex, startTile, endTile));
            startTile = endTile;
        }
        return new Commute(this.player, jumps);
    }

}
