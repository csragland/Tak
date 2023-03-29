using UnityEngine;
using System.Collections.Generic;

public class PlayerControl : MonoBehaviour
{
    public GameManager gameManager;

    float horizontalInput;
    float verticalInput;

    public int currentTileId;

    public int player;

    Quaternion snapStart;
    Quaternion snapEnd;
    float t1;
    float t2;

    Vector3 startZoom;
    Vector3 zoomDestination;
    float startDist;
    float currentDist;
    bool zoomPrepared;

    Quaternion startRotation;
    Quaternion endRotation;
    bool rotationPrepared;

    public bool isBoarding = true;
    List<GameObject> commuters = new();

    List<(bool, int)> outlineMemory = new();

    // Start is called before the first frame update
    void Awake()
    {
        Camera.main.transform.position = Settings.cameraOffset;
        player = GameManager.currentPlayer;
        gameManager = GameObject.Find("Game Manager").GetComponent<GameManager>();
        snapEnd = Quaternion.Euler(0, 0, 0);

        currentTileId = (int)Mathf.Floor(Mathf.Pow(Settings.dimension, 2) / 2);
        GameObject centerTile = GameObject.Find("Board/Tile_" + currentTileId);
        centerTile.GetComponent<cakeslice.Outline>().enabled = true;
        this.transform.position = centerTile.transform.position;
    }

    // Update is called once per frame
    void LateUpdate()
    {
        // Let the player rotate within rotation boundaries (none if "splitBoardView" is off)
        if (Input.GetKey(KeyCode.A) &&
            (!Settings.splitBoardView
                || (player == 1 && CompareRotations(Comparisons.LTE, 90))
                || (player == 2 && (CompareRotations(Comparisons.LTE, -90)
                || CompareRotations(Comparisons.GT, 0)))))
        {
            horizontalInput = 1;
            snapStart = transform.rotation;
        }
        else if (Input.GetKey(KeyCode.D) &&
            (!Settings.splitBoardView
                || (player == 1 && CompareRotations(Comparisons.GTE, -90))
                || (player == 2 && (CompareRotations(Comparisons.GTE, 90)
                || CompareRotations(Comparisons.LT, 0)))))
        {
            horizontalInput = -1;
            snapStart = transform.rotation;
        }
        // Reset rotation speed back to zero if no interaction
        else
        {
            horizontalInput = 0;
        }

        // Let the player zoom within the zoom boundaries
        if (Input.GetKey(KeyCode.W) &&
            Vector3.Distance(Camera.main.transform.position, this.transform.position) > Settings.cameraMaxZoom)
        {
            verticalInput = 1;
        }
        else if (Input.GetKey(KeyCode.S) &&
            Vector3.Distance(Camera.main.transform.position, this.transform.position) < Settings.cameraMinZoom)
        {
            verticalInput = -1;
        }
        // Reset the zoom speed back to zero if no interaction
        else
        {
            verticalInput = 0;
        }

        if (player == GameManager.currentPlayer)
        {
            // If the player isn't rotating and they are within range, snap to either 0 or 180 degrees
            if (horizontalInput == 0
                &&
                ((transform.rotation.y != 0
                        && (player == 1 || !Settings.splitBoardView)
                        && CompareRotations(Comparisons.LTE, Settings.cameraSnapRadius, true))
                   || (transform.eulerAngles.y != 180
                        && (player == 2 && Settings.splitBoardView)
                        && CompareRotations(Comparisons.GTE, Mathf.Abs(MapAngle(this.snapEnd.eulerAngles.y)) - Settings.cameraSnapRadius, true))))
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

            this.CheckMove(e);
        }
    }

    // Remap arrow keys based on rotation and update tile currently in focus
    private void CheckArrowOver(Event e)
    {
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

    // Get player's intended move based on input and check if it is legal. Do the move if it is.
    private void CheckMove(Event e)
    {
        // In the first turn of the game, players place their opponent's piece rather than their own
        int pieceOwner = gameManager.tak.turnNum > 2 ? player : player == 1 ? 2 : 1;
        Move move;
        if (e.keyCode == KeyCode.Alpha1)
        {
            move = new Placement(pieceOwner, PieceType.STONE, Utils.NumToTile(this.currentTileId));
        }
        else if (e.keyCode == KeyCode.Alpha2)
        {
            move = new Placement(pieceOwner, PieceType.BLOCKER, Utils.NumToTile(this.currentTileId));
        }
        else if (e.keyCode == KeyCode.Alpha3)
        {
            move = new Placement(pieceOwner, PieceType.CAPSTONE, Utils.NumToTile(this.currentTileId));
        }
        else if (e.keyCode == KeyCode.Space)
        {
            move = this.BuildCommute(Utils.NumToTile(this.currentTileId));
        }
        else if (e.keyCode == KeyCode.Z && gameManager.tak.moveStack.Count > 0)
        {
            gameManager.UndoMove();
            return;
        }
        else if (e.keyCode == KeyCode.X && gameManager.tak.moveBranch.Count > 0)
        {
            gameManager.RedoMove();
            return;
        }
        else
        {
            return;
        }

        if (gameManager.tak.IsLegalMove(move))
        {
            gameManager.DoMove(move);
        }
    }

    // Move camera focal point to the top of a tile's stack and outline interactable pieces
    private void SetFocus(int tileId)
    {
        GameObject tile = GameObject.Find("Board/Tile_" + tileId);
        int numChildren = tile.transform.childCount;
        // If there are pieces on the stack, focus and outline the pieces
        if (numChildren > 0)
        {
            // The "crown" is the piece at the top of a stack
            GameObject crown = tile.transform.GetChild(numChildren - 1).gameObject;
            // Move the camera focal point to the crown
            this.transform.position = crown.transform.position;
            // If the crown belongs to the current player, then apply outline to the stack
            if (crown.GetComponent<PieceUI>().player == GameManager.currentPlayer)
            {
                bool stackAlreadyClicked = this.commuters.Count > 0 && commuters[^1].transform.IsChildOf(tile.transform);
                for (int i = 0; i < numChildren; i++)
                {
                    GameObject piece = tile.transform.GetChild(i).gameObject;
                    cakeslice.Outline outline = piece.GetComponent<cakeslice.Outline>();
                    // If stack has already been clicked on, apply memorized outline
                    if (stackAlreadyClicked)
                    {
                        outline.enabled = this.outlineMemory[i].Item1;
                        outline.color = this.outlineMemory[i].Item2;
                    }
                    // Otherwise, outline everything
                    else
                    {
                        outline.enabled = true;
                        outline.color = 0;
                    }
                }
            }
        }
        // Otherwise, move focal point to center of tile and highlight tile
        else
        {
            this.transform.position = tile.transform.position;
            tile.GetComponent<cakeslice.Outline>().enabled = true;
        }
    }

    // Remove outline from tile and every piece on it
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
        if (Settings.splitBoardView)
        {
            // Setting variables to allow for automatic perspective shift
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

            // First automatically move camera to original zoom
            if (Mathf.Round(Vector3.Distance(Camera.main.transform.position, this.transform.position)) != Mathf.Round(startDist))
            {
                t1 += Time.deltaTime / Settings.cameraTransitionTime;
                Camera.main.transform.position = Vector3.Lerp(startZoom, zoomDestination, t1);
            }
            // If camera is at original zoom, then rotate to opposite side of board
            else if (this.transform.rotation.eulerAngles.y != endRotation.eulerAngles.y)
            {
                t2 += Time.deltaTime / Settings.cameraTransitionTime;
                this.transform.rotation = Quaternion.Lerp(startRotation, endRotation, t2);
            }
            // The zoom and rotation must be at the desired location, so declare ourselves done and redefine the center of view
            else
            {
                zoomPrepared = false;
                rotationPrepared = false;
                snapEnd = player == 1 ? Quaternion.Euler(0, 0, 0) : Quaternion.Euler(0, -180, 0);
            }
        }
        // Reset list of pieces queued for a commute, if any, and sync current player
        this.commuters.Clear();
        this.player = GameManager.currentPlayer;
    }

    public void ProcessClick(GameObject clicked)
    {
        Transform parent = clicked.transform.parent;
        // If I am allowing a commute to be created and I am clicking on a piece that is within the current focus...
        if (this.isBoarding && clicked.transform.IsChildOf(Utils.GetUITile(currentTileId).transform) && this.IsPlayerContolled(parent.gameObject))
        {
            // If no commuters exist OR what I am clicking on is a sibling of the other commuters but there are still less than <dimension> commuters AND I am clicking on a new tile
            if (this.commuters.Count == 0 || (clicked.transform.IsChildOf(commuters[^1].transform.parent) && commuters.Count < Settings.dimension) && !this.commuters.Contains(clicked))
            {
                int clickedIndex = clicked.transform.GetSiblingIndex();
                // Give green outline to clicked piece and add that piece to the list of commuters
                commuters.Add(clicked);
                clicked.GetComponent<cakeslice.Outline>().color = 1;
                // Remove outline from the pieces below the one just clicked if they haven't previously been clicked
                for (int i = 0; i < clickedIndex; i++)
                {
                    Transform sibling = parent.GetChild(i);
                    if (!this.commuters.Contains(sibling.gameObject))
                    {
                        sibling.GetComponent<cakeslice.Outline>().enabled = false;
                    }
                }
                this.SaveOutline(parent);
            }
            // If I am clicking on an already clicked piece, I must be trying to unqueue commuters 
            else if (this.commuters.Contains(clicked))
            {
                // Turn clicked piece back to yellow
                clicked.GetComponent<cakeslice.Outline>().color = 0;
                int index = this.commuters.IndexOf(clicked);
                int stackIndex = parent.childCount - 1;
                int bottomIndex = index < 1 ? 0 : commuters[index - 1].transform.GetSiblingIndex(); // Index of next piece currently in the list of commuters
                // From the top of the stack to the the piece right above "bottomIndex", reenable outline and make it yellow
                do
                {
                    cakeslice.Outline outline = parent.GetChild(stackIndex).GetComponent<cakeslice.Outline>();
                    outline.enabled = true;
                    if (stackIndex > bottomIndex)
                    {
                        outline.color = 0;
                    }
                    stackIndex--;

                } while (stackIndex >= bottomIndex);

                // Unqueue all commuters that lie at or above the piece that was just clicked
                for (int i = this.commuters.Count - 1; i >= index; i--)
                {
                    this.commuters.RemoveAt(i);
                }
                this.SaveOutline(parent);
            }
        }
    }

    // Take list of clicked pieces, compare that list with the stack those pieces belong to, and return the commute it represents
    private Commute BuildCommute(Tile end)
    {
        List<Jump> jumps = new();
        Tile start = Utils.NumToTile(this.commuters[0].transform.parent.GetSiblingIndex());
        int[] direction = new int[] { (end.row - start.row) / this.commuters.Count, (end.col - start.col) / this.commuters.Count };
        Tile startTile = start;
        int baseAdjustment = 0;
        for (int i = 1; i <= this.commuters.Count; i++)
        {
            int stackIndex = this.commuters[i - 1].transform.GetSiblingIndex();
            int baseIndex = stackIndex + baseAdjustment;
            Tile endTile = new Tile(start.row + direction[0] * i, start.col + direction[1] * i);
            baseAdjustment = -stackIndex + Utils.GetUITile(endTile).transform.childCount;
            jumps.Add(new Jump(baseIndex, startTile, endTile));
            startTile = endTile;
        }
        return new Commute(this.player, jumps);
    }

    // Save outline information for each piece in a stack
    private void SaveOutline(Transform parent)
    {
        this.outlineMemory.Clear();
        if (parent.childCount > 0)
        {
            for (int i = 0; i < parent.childCount; i++)
            {
                cakeslice.Outline outline = parent.GetChild(i).GetComponent<cakeslice.Outline>();
                this.outlineMemory.Add((outline.enabled, outline.color));
            }
        }
    }

    // Utility for comparing the camera's current rotation with another rotation
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

    // Checks if the crown of a stack is owned by the current player or not
    private bool IsPlayerContolled(GameObject tile)
    {
        GameObject crown = Utils.GetUICrown(tile);
        if (crown is not null && crown.GetComponent<PieceUI>().player == GameManager.currentPlayer) {
            return true;
        }
        return false;
    }

    // Maps rotations onto [-180, 180)
    private float MapAngle(float angle)
    {
        if (angle >= 180)
        {
            return angle - 360;
        }
        return angle;
    }
}
