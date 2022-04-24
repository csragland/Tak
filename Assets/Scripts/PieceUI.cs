using UnityEngine;

public class PieceUI : MonoBehaviour
{

    public PieceType type;
    public int player;

    UI ui;

    private bool isBeingSpawned;
    public Vector3 spawnPos;
    public Vector3 destination;
    private Vector3 spawnRotationAxis;
    private float spawnRPS;
    public float spawnTime;
    public static Quaternion type2Rotation = Quaternion.Euler(new Vector3(90, 45, 0));

    float t;

    public PieceUI(PieceType type, int player)
    {
        this.type = type;
        this.player = player;
    }

    // Start is called before the first frame update
    void Start()
    {
        isBeingSpawned = true;
        this.spawnPos = this.transform.position;

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
                this.transform.position = Vector3.Lerp(spawnPos, destination, t);
                this.transform.Rotate(360 * this.spawnRPS * Time.deltaTime * this.spawnRotationAxis);
            }
            else
            {
                this.isBeingSpawned = false;
            }
        }
    }
}