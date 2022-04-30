using UnityEngine;

public class PieceUI : MonoBehaviour
{

    public PieceType type;
    public int player;

    UI ui;

    CameraControl cameraControl;

    public bool isBeingSpawned = true;
    public bool isCommuting;
    public Vector3 origin;
    public Vector3 destination;
    public GameObject destinationTile;
    private Vector3 spawnRotationAxis;
    private float spawnRPS;
    public float spawnTime;

    float t = 0;

    public Rigidbody rb;
    bool isGrounded;

    public PieceUI(PieceType type, int player)
    {
        this.type = type;
        this.player = player;
    }

    // Start is called before the first frame update
    void Start()
    {
        this.cameraControl = GameObject.Find("Camera Focus").GetComponent<CameraControl>();
        rb = this.GetComponent<Rigidbody>();
        this.GetComponent<Rigidbody>().isKinematic = true;
        isBeingSpawned = true;
        isCommuting = false;
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
                this.rb.isKinematic = false;
                t = 0;
            }
        }
        else if (this.isCommuting)
        {
            this.rb.isKinematic = false;

            if (this.transform.position != this.destination)
            {
                if (this.isGrounded)
                {
                    Vector3 direction = this.destination - this.origin;
                    this.rb.AddForce(Vector3.up * GetVerticalImpulse(1, 1) + direction * this.GetHorizontalImpulse(1, 1), ForceMode.Impulse);
                    this.isGrounded = false;
                }
            }
            else
            {
                this.isCommuting = false;
                this.transform.position = this.destination;
                this.transform.SetParent(destinationTile.transform);
            }
            
        }
    }

    public void SetCommute(Vector3 destination, GameObject tile)
    {
        this.isCommuting = true;
        this.origin = this.transform.position;
        this.destination = destination;
        this.rb.isKinematic = false;
        this.destinationTile = tile;
    }

    float GetVerticalImpulse(float height, float mass)
    {
        return Mathf.Sqrt(-2 * Physics.gravity.y * height) * mass;
    }

    float GetHorizontalImpulse(float height, float mass)
    {
        float time = -GetVerticalImpulse(height, mass) / Physics.gravity.y;
        return Settings.tileDimensions.x / (2 * time);
    }

    private void OnCollisionEnter(Collision collision)
    {
        this.isGrounded = true;
        if (this.isCommuting)
        {
            this.transform.position = this.destination;
            this.rb.isKinematic = true;
        }
    }

    private void OnMouseDown()
    {
        this.cameraControl.ProcessClick(this.gameObject);
    }
}
