using UnityEngine;

public class CameraControl : MonoBehaviour
{
    float horizontalInput;
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

        if (transform.rotation.y != 0 && CompareRotations(Comparisons.LTE, Settings.cameraSnapRadius, true) && horizontalInput == 0)
        {
            t += Time.deltaTime / Settings.cameraSnapSpeed;
            this.transform.rotation = Quaternion.Lerp(snapStart, Quaternion.Euler(0, 0, 0), t);
        }

        transform.Rotate(Settings.cameraSpeed * horizontalInput * Vector3.up);

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
                currentTileId++;
                this.SetFocus(currentTileId);
            }
            if (((e.keyCode == KeyCode.LeftArrow && CompareRotations(Comparisons.LTE, 45, true))
                || (e.keyCode == KeyCode.DownArrow && CompareRotations(Comparisons.GT, 45))
                || (e.keyCode == KeyCode.UpArrow && CompareRotations(Comparisons.LTE, -45)))
                && !(currentTileId % Settings.dimension == 0))
            {
                currentTileId--;
                this.SetFocus(currentTileId);
            }
            if (((e.keyCode == KeyCode.UpArrow && CompareRotations(Comparisons.LTE, 45, true))
                || (e.keyCode == KeyCode.LeftArrow && CompareRotations(Comparisons.GT, 45))
                || (e.keyCode == KeyCode.RightArrow && CompareRotations(Comparisons.LTE, -45)))
                && !(currentTileId < Settings.dimension))
            {
                currentTileId -= Settings.dimension;
                this.SetFocus(currentTileId);
            }
            if (((e.keyCode == KeyCode.DownArrow && CompareRotations(Comparisons.LTE, 45, true))
                || (e.keyCode == KeyCode.RightArrow && CompareRotations(Comparisons.GT, 45))
                || (e.keyCode == KeyCode.LeftArrow && CompareRotations(Comparisons.LTE, -45)))
                && !(currentTileId >= Settings.dimension * (Settings.dimension - 1)))
            {
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
            this.transform.position = tile.transform.GetChild(numChildren - 1).position;
        }
        else
        {
            this.transform.position = tile.transform.position;
        }
    }

}
