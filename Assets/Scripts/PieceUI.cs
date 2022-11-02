using UnityEngine;

public class PieceUI : MonoBehaviour
{

    public PieceType type;
    public int player;
    public Rigidbody rb;

    UI ui;
    PlayerControl playerControl;

    public Vector3 origin;
    public Vector3 destination;

    // Spawn Variables
    public bool isBeingSpawned = true;
    public GameObject destinationTile;
    private Vector3 spawnRotationAxis;
    private float spawnRPS;
    public float spawnTime;
    float t = 0;

    // Jump Variables
    public bool isJumping;
    private float jumpStartTime;
    private float jumpEndTime;
    private float jumpTime;
    private float jumpVy;
    private float jumpVxz;

    public PieceUI(PieceType type, int player)
    {
        this.type = type;
        this.player = player;
    }

    // Start is called before the first frame update
    void Start()
    {
        this.playerControl = GameObject.Find("Camera Focus").GetComponent<PlayerControl>();
        rb = this.GetComponent<Rigidbody>();
        isBeingSpawned = true;
        isJumping = false;
        this.origin = this.transform.position;

        if (this.type == PieceType.STONE)
        {
            this.spawnTime = Settings.stoneSpawnTime;
            this.spawnRPS = Settings.stoneRPS;
            this.spawnRotationAxis = Vector3.up;

        }
        else if (this.type == PieceType.BLOCKER)
        {
            this.spawnTime = Settings.blockerSpawnTime;
            this.spawnRPS = Settings.blockerRPS;
            this.spawnRotationAxis = Vector3.right;
        }
        else
        {
            this.spawnRPS = Settings.capstoneRPS;
            this.spawnTime = Settings.capstoneSpawnTime;
        }

        ui = GameObject.Find("Game Manager").GetComponent<UI>();
        this.GetComponent<Renderer>().material = player == 1 ? ui.white : ui.black;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (this.isBeingSpawned)
        {
            if (this.transform.position != this.destination)
            {
                t += Time.deltaTime / spawnTime;
                this.transform.position = Vector3.Lerp(origin, destination, t);
                this.transform.Rotate(360 * this.spawnRPS * Time.deltaTime * this.spawnRotationAxis);
            }
            else
            {
                this.isBeingSpawned = false;
                t = 0;
            }
        }
        else if (this.isJumping)
        {

            if (Time.time <= this.jumpEndTime)
            {

                Vector3 horizontalDirection = this.destination - this.origin;
                horizontalDirection.y = 0;
                float deltaTime = Time.time - this.jumpStartTime;
                float deltaY = this.jumpVy * deltaTime + 0.5f * Physics.gravity.y * deltaTime * deltaTime;
                Vector3 deltaPos = horizontalDirection * (deltaTime * jumpVxz) + Vector3.up * deltaY;
                this.transform.position = this.origin + deltaPos;
            }
            else
            {
                this.isJumping = false;
                this.transform.position = this.destination;
                this.transform.SetParent(destinationTile.transform);
            }
            
        }
    }

    public void SetCommute(Vector3 destination, GameObject tile, float[] jumpData)
    {
        this.isJumping = true;
        this.origin = this.transform.position;
        this.destination = destination;
        this.destinationTile = tile;
        this.jumpStartTime = Time.time;
        this.jumpVxz = jumpData[0];
        this.jumpVy = jumpData[1];
        this.jumpTime = jumpData[2];
        this.jumpEndTime = this.jumpStartTime + this.jumpTime;
    }

    private void OnMouseDown()
    {
        this.playerControl.ProcessClick(this.gameObject);
    }
}
