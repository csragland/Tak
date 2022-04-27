using UnityEngine;

public class CameraControl : MonoBehaviour
{

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

    // Start is called before the first frame update
    void Start()
    {
        Camera.main.transform.position = Settings.cameraOffset;
        player = GameManager.currentPlayer;
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
    }

    private bool CompareRotations(Comparisons operation, float rotation, bool abs=false)
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
            startDist = Vector3.Magnitude(Settings.cameraOffset);
            currentDist = Vector3.Magnitude(Camera.main.transform.position);
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

        if (Mathf.Round(Vector3.Magnitude(Camera.main.transform.position)) != Mathf.Round(startDist))
        {
            t1 += Time.deltaTime / Settings.cameraTransitionTime;
            Camera.main.transform.position = Vector3.Lerp(startZoom, zoomDestination, t1);
        }
        else if (this.transform.rotation.eulerAngles.y != endRotation.eulerAngles.y)
        {
            zoomPrepared = false;
            t2 += Time.deltaTime / Settings.cameraTransitionTime;
            this.transform.rotation = Quaternion.Lerp(startRotation, endRotation, t2);
        }
        else
        {
            rotationPrepared = false;
            this.player = GameManager.currentPlayer;
            snapEnd = player == 1 ? Quaternion.Euler(0, 0, 0) : Quaternion.Euler(0, -180, 0);
        }
    }

    private float MapAngle(float angle)
    {
        if (angle >= 180)
        {
            return angle - 360;
        }
        return angle;
    }

}
