using UnityEngine;

public static class Settings
{

    [Header("Board Settings")]
    public static readonly int dimension = 5;
    public static readonly Vector3 location = Vector3.zero;
    public static readonly Vector3 tileDimensions = new Vector3(1, .25f, 1);

    [Header("Camera Settings")]
    public static readonly Vector3 cameraOffset = new Vector3(0, 3, -4);
    public static readonly float cameraRotateSpeed = 2f;
    public static readonly float cameraZoomSpeed = .1f;
    public static readonly float cameraSnapRadius = 7;
    public static readonly float cameraSnapSpeed = .2f;

    [Header("Stone Settings")]
    public static readonly float stoneRPS = 1.5f;
    public static readonly float stoneSpawnTime = 1.5f;
    public static readonly float stoneSpawnHeight = 6;

    [Header("Blocker Settings")]
    public static readonly float blockerRPS = 1.5f;
    public static readonly float blockerSpawnTime = 1.5f;
    public static readonly float blockerSpawnHeight = 6;

    [Header("Capstone Settings")]
    public static readonly float capstoneRPS = 0;
    public static readonly float capstoneSpawnTime = .2f;
    public static readonly float capstoneSpawnHeight = 6;



}
