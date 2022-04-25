using UnityEngine;

public class PieceUI : MonoBehaviour
{

    public PieceType type;
    public int player;

    UI ui;

    private bool isBeingSpawned;
    private bool isCommuting;
    public Vector3 origin;
    public Vector3 destination;
    private Vector3 spawnRotationAxis;
    private float spawnRPS;
    public float spawnTime;

    float t = 0;

    Rigidbody rb;
    bool isGrounded;

    public PieceUI(PieceType type, int player)
    {
        this.type = type;
        this.player = player;
    }

    // Start is called before the first frame update
    void Start()
    {
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
                this.ToggleRb();
                t = 0;
            }
        }
        else if (this.isCommuting)
        {
            if(this.transform.position.x != this.destination.x)
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
            }
            
        }
    }

    public void SetCommute(Vector3 destination)
    {
        this.isCommuting = true;
        this.origin = this.transform.position;
        this.destination = destination;
        this.rb.isKinematic = false;
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
            this.rb.isKinematic = true;
            this.transform.position = this.destination;
        }
        Debug.Log("Hit");
    }

    private void ToggleRb()
    {
        this.rb.isKinematic = !this.rb.isKinematic;
    }
}