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
    public bool isBeingSpawned = false;
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

    //private Animator animator

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
            this.spawnRotationAxis = Vector3.up;
        }
        else
        {
            this.spawnRPS = Settings.capstoneRPS;
            this.spawnTime = Settings.capstoneSpawnTime;
        }

        ui = GameObject.Find("Game Manager").GetComponent<UI>();
        this.GetComponent<Renderer>().material = player == 1 ? ui.whitePiece : ui.blackPiece;
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
                //this.transform.SetParent(destinationTile.transform);
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

    private void OnTriggerEnter(Collider other)
    {
        // Assume only capstone is trigger
        if (this.type == PieceType.BLOCKER)
        {
            Vector3 stonePos = this.transform.position + .5f * (ui.GetPieceHeight(ui.stone) - ui.GetPieceHeight(ui.blocker)) * Vector3.up;
            GameObject stone = Instantiate(ui.stone, stonePos, ui.stone.transform.rotation);
            Animator stoneAnimator = stone.GetComponent<Animator>();
            stoneAnimator.enabled = true;
            stoneAnimator.SetBool("isExpanding", true);
            PieceUI stoneData = stone.GetComponent<PieceUI>();
            stoneData.player = this.player;
            stoneData.type = PieceType.STONE;
            // This is a race condition with the capstone (should win by good margin though)
            stone.transform.SetParent(this.transform.parent);
            stone.name = this.name;
            other.isTrigger = false;

            GameObject shell = new("temp");
            shell.transform.position = this.transform.position;
            this.transform.SetParent(shell.transform);
            Animator animator = gameObject.GetComponent<Animator>();
            animator.enabled = true;
            // Object destroyed when done
            animator.SetBool("isFlattening", true);

            GameObject shellOther = new("temp");
            shellOther.transform.position = other.gameObject.transform.position;
            other.gameObject.transform.SetParent(shellOther.transform);
            Animator animatorOther = other.gameObject.GetComponent<Animator>();
            animatorOther.enabled = true;
            // Object destroyed when done
            animatorOther.SetBool("isSinking", true);
        }
    }
}
