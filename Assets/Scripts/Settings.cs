using UnityEngine;

public static class Settings
{

    [Header("Board Settings")]
    public static readonly int dimension = 5;
    public static readonly Vector3 location = Vector3.zero;
    public static readonly Vector3 tileDimensions = new(1, .25f, 1);

    [Header("Camera Settings")]
    public static readonly Vector3 cameraOffset = new(0, 3, -4);
    public static readonly float cameraRotateSpeed = 3f;
    public static readonly float cameraZoomSpeed = .2f;
    public static readonly float cameraSnapRadius = 7;
    public static readonly float cameraSnapSpeed = .2f;
    public static readonly float cameraMinZoom = 12f;
    public static readonly float cameraMaxZoom = 1f;
    public static readonly float cameraTransitionTime = .5f;
    public static readonly float spawnCooldown = .25f;
    public static readonly bool splitBoardView = false;

    [Header("Stone Settings")]
    public static readonly float stoneRPS = 1.5f;
    public static readonly float stoneSpawnTime = 1f;
    public static readonly float stoneSpawnHeight = 4;

    [Header("Blocker Settings")]
    public static readonly float blockerRPS = 1.5f;
    public static readonly float blockerSpawnTime = 1f;
    public static readonly float blockerSpawnHeight = 4;

    [Header("Capstone Settings")]
    public static readonly float capstoneRPS = 0;
    public static readonly float capstoneSpawnTime = .2f;
    public static readonly float capstoneSpawnHeight = 6;

    public static readonly float overshootHeight = 3 * tileDimensions.y;
    public static readonly float jumpCooldown =  .25f;

    public static readonly float flattenTime = 80f / 60f;
}
