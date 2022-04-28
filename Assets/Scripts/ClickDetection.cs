using UnityEngine;

public class ClickDetection : MonoBehaviour
{
    CameraControl cameraControl;

    private void Start()
    {
        this.cameraControl = GameObject.Find("Camera Focus").GetComponent<CameraControl>();
    }

    private void OnMouseDown()
    {
        
    }
}
