using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Piece : MonoBehaviour
{

    public int type;
    public int player;

    GameManager gameManager;

    public bool isBeingSpawned;
    public Vector3 spawnPos;
    public Vector3 destination;
    private Vector3 spawnRotationAxis;
    public float spawnSpeed;
    public static Quaternion type2Rotation = Quaternion.Euler(new Vector3(90, 45, 0));

    float t;

    public Piece(int type, int player)
    {
        this.type = type;
        this.player = player;
    }

    // Start is called before the first frame update
    void Start()
    {
        isBeingSpawned = true;
        this.spawnPos = this.transform.position;

        if (this.type == 1)
        {
            this.spawnSpeed = 1.5f;
            this.spawnRotationAxis = Vector3.up;

        }
        else if (this.type == 2)
        {
            this.spawnSpeed = 1.5f;
            this.spawnRotationAxis = Vector3.right;
        }
        else
        {
            this.spawnSpeed = .2f;
        }

        gameManager = GameObject.Find("Game Manager").GetComponent<GameManager>();
        this.GetComponent<Renderer>().material = player == 1 ? gameManager.white : gameManager.black;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (this.isBeingSpawned)
        {
            if (this.transform.position != this.destination)
            {
                t += Time.deltaTime / spawnSpeed;
                this.transform.position = Vector3.Lerp(spawnPos, destination, t);
                this.transform.Rotate((720 / spawnSpeed) * Time.deltaTime * this.spawnRotationAxis);
            }
            else
            {
                this.isBeingSpawned = false;
            }
        }
    }
}
