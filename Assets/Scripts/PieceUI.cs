using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PieceUI : MonoBehaviour
{

    public PieceType type;
    public int player;

    UI ui;

    public bool isBeingSpawned;
    public Vector3 spawnPos;
    public Vector3 destination;
    private Vector3 spawnRotationAxis;
    public float spawnSpeed;
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
            this.spawnSpeed = 1.5f;
            this.spawnRotationAxis = Vector3.up;

        }
        else if (this.type == PieceType.BLOCKER)
        {
            this.spawnSpeed = 1.5f;
            this.spawnRotationAxis = Vector3.right;
        }
        else
        {
            this.spawnSpeed = .2f;
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