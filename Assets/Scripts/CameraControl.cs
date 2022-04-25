using UnityEngine;

public class CameraControl : MonoBehaviour
{

    float horizontalInput;
    float verticalInput;

    int currentTileId;

    bool initialized = false;

    Quaternion snapStart;
    float t;

    // Start is called before the first frame update
    void Start()
    {
        Camera.main.transform.position = Settings.cameraOffset;
        
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (!this.initialized)
        {
            currentTileId = (int)Mathf.Floor(Mathf.Pow(Settings.dimension, 2) / 2);
            GameObject centerTile = GameObject.Find("Board/Tile_" + currentTileId);
            centerTile.GetComponent<cakeslice.Outline>().enabled = true;
            this.transform.position = centerTile.transform.position;
            this.initialized = true;
        }
        if (Input.GetKey(KeyCode.A) && CompareRotations(Comparisons.LTE, 90))
        {
            horizontalInput = 1;
            snapStart = transform.rotation;
        }
        else if (Input.GetKey(KeyCode.D) && CompareRotations(Comparisons.GTE, -90))
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

        if (transform.rotation.y != 0 && CompareRotations(Comparisons.LTE, Settings.cameraSnapRadius, true) && horizontalInput == 0)
        {
            t += Time.deltaTime / Settings.cameraSnapSpeed;
            this.transform.rotation = Quaternion.Lerp(snapStart, Quaternion.Euler(0, 0, 0), t);
        }

        transform.Rotate(Settings.cameraRotateSpeed * horizontalInput * Vector3.up);
        Camera.main.transform.Translate(Settings.cameraZoomSpeed * verticalInput * Vector3.forward);

    }

    private void OnGUI()
    {
        Event e = Event.current;
        if (e.type == EventType.KeyDown)
        {
            if (((e.keyCode == KeyCode.RightArrow && CompareRotations(Comparisons.LTE, 45, true))
                || (e.keyCode == KeyCode.UpArrow && CompareRotations(Comparisons.GT, 45)) 
                || (e.keyCode == KeyCode.DownArrow && CompareRotations(Comparisons.LTE, -45)))
                && !(currentTileId % Settings.dimension == Settings.dimension - 1))
            {
                this.Unfocus(currentTileId);
                currentTileId++;
                this.SetFocus(currentTileId);
            }
            if (((e.keyCode == KeyCode.LeftArrow && CompareRotations(Comparisons.LTE, 45, true))
                || (e.keyCode == KeyCode.DownArrow && CompareRotations(Comparisons.GT, 45))
                || (e.keyCode == KeyCode.UpArrow && CompareRotations(Comparisons.LTE, -45)))
                && !(currentTileId % Settings.dimension == 0))
            {
                this.Unfocus(currentTileId);
                currentTileId--;
                this.SetFocus(currentTileId);
            }
            if (((e.keyCode == KeyCode.UpArrow && CompareRotations(Comparisons.LTE, 45, true))
                || (e.keyCode == KeyCode.LeftArrow && CompareRotations(Comparisons.GT, 45))
                || (e.keyCode == KeyCode.RightArrow && CompareRotations(Comparisons.LTE, -45)))
                && !(currentTileId < Settings.dimension))
            {
                this.Unfocus(currentTileId);
                currentTileId -= Settings.dimension;
                this.SetFocus(currentTileId);
            }
            if (((e.keyCode == KeyCode.DownArrow && CompareRotations(Comparisons.LTE, 45, true))
                || (e.keyCode == KeyCode.RightArrow && CompareRotations(Comparisons.GT, 45))
                || (e.keyCode == KeyCode.LeftArrow && CompareRotations(Comparisons.LTE, -45)))
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
                return this.transform.rotation.y < Quaternion.Euler(0, rotation, 0).y;

            }
            else
            {
                return Mathf.Abs(this.transform.rotation.y) < Quaternion.Euler(0, rotation, 0).y;
            }
        }
        if (operation == Comparisons.LTE)
        {
            if (!abs)
            {
                return this.transform.rotation.y <= Quaternion.Euler(0, rotation, 0).y;

            }
            else
            {
                return Mathf.Abs(this.transform.rotation.y) <= Quaternion.Euler(0, rotation, 0).y;
            }
        }
        if (operation == Comparisons.GTE)
        {
            if (!abs)
            {
                return this.transform.rotation.y >= Quaternion.Euler(0, rotation, 0).y;

            }
            else
            {
                return Mathf.Abs(this.transform.rotation.y) >= Quaternion.Euler(0, rotation, 0).y;
            }
        }
        if (operation == Comparisons.GT)
        {
            if (!abs)
            {
                return this.transform.rotation.y > Quaternion.Euler(0, rotation, 0).y;

            }
            else
            {
                return Mathf.Abs(this.transform.rotation.y) > Quaternion.Euler(0, rotation, 0).y;
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
        else // Doesn't work as intended upon spawning piece
        {
            tile.GetComponent<cakeslice.Outline>().enabled = false;
        }
    }

}
